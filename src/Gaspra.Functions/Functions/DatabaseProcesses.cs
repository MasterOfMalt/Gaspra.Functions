using Gaspra.DatabaseProcesses;
using Gaspra.DatabaseProcesses.Models;
using Gaspra.Functions.Correlation;
using Gaspra.Functions.Extensions;
using Gaspra.Functions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gaspra.Functions.Functions
{
    public class DatabaseProcesses : IFunction
    {
        private readonly IDatabaseProcessesService databaseProcessesService;

        private readonly TimeSpan WaitTime = new TimeSpan(5 * 1000);

        public DatabaseProcesses(IDatabaseProcessesService databaseProcessesService)
        {
            this.databaseProcessesService = databaseProcessesService;
        }

        public IEnumerable<string> FunctionAliases => new[] { "databaseprocesses", "dbp" };

        public string FunctionHelp => "";

        public bool ValidateParameters(IEnumerable<IFunctionParameter> parameters)
        {
            var connectionStringParameter = parameters.Where(p => p.Key.Equals("c")).FirstOrDefault();

            if(connectionStringParameter == null ||
                connectionStringParameter.Values.FirstOrDefault() == null ||
                string.IsNullOrWhiteSpace(connectionStringParameter.Values.FirstOrDefault().ToString()))
            {
                return false;
            }

            return true;
        }

        public async Task Run(CancellationToken cancellationToken, IEnumerable<IFunctionParameter> parameters)
        {
            var connectionString = parameters
                .Where(p => p.Key.Equals("c"))
                .FirstOrDefault()
                .Values
                .FirstOrDefault()
                .ToString();

            var headline = new ConsoleLine(
                $"{PadAndWrap("SID", 5)} {PadAndWrap("COMMAND", 7)} {PadAndWrap("DATABASE", 16)} {PadAndWrap("SCHEMA", 16)} {PadAndWrap("OBJECT", 16)} {PadAndWrap("WAIT TIME", 10)}");

            var underline = new ConsoleLine(
                $"{PadAndWrap("-", 85, padChar:'-', wrapL:"-", wrapR: "-")}");

            var consoleLines = new List<ConsoleLine>();

            while (!cancellationToken.IsCancellationRequested)
            {
                var runningProcesses = (await databaseProcessesService.GetRunningProcesses(connectionString)).ToList();

                var runningProcessesCount = runningProcesses.Count();

                var consoleLinesCount = consoleLines.Count();

                var iterator = runningProcessesCount > consoleLinesCount ?
                    runningProcessesCount : consoleLinesCount;

                for(var i = 0;  i < iterator; i++)
                {
                    if(i < consoleLinesCount)
                    {
                        if(i < runningProcessesCount)
                        {
                            consoleLines[i].Rewrite(RunningProcessString(runningProcesses[i]));
                        }
                        else
                        {
                            consoleLines[i].Clear();
                        }
                    }
                    else
                    {
                        consoleLines.Add(new ConsoleLine(RunningProcessString(runningProcesses[i])));
                    }
                }

                Thread.Sleep(WaitTime);
            }
        }

        private string RunningProcessString(RunningProcess runningProcess)
        {
            var sessionId = PadAndWrap(runningProcess.SessionId.ToString(), 5);

            var command = PadAndWrap(runningProcess.Command, 7);

            var database = PadAndWrap(runningProcess.Database, 16);

            var schema = PadAndWrap(runningProcess.Schema, 16);

            var obj = PadAndWrap(runningProcess.Object, 16);

            var waitTime = PadAndWrap(runningProcess.WaitTime.ToString(), 10);

            return $"{sessionId} {command} {database} {schema} {obj} {waitTime}";
        }

        private string PadAndWrap(
            string toPad,
            int length,
            char padChar = ' ',
            string wrapL = "[",
            string wrapR = "]")
        {
            var value = string.IsNullOrWhiteSpace(toPad) ? "NULL" : toPad;

            if(value.Length < length)
            {
                value = $"{wrapL}{value.PadLeft(length, padChar)}{wrapR}";
            }
            else
            {
                value = $"{wrapL}{value}{wrapR}";
            }

            return value;
        }
    }
}
