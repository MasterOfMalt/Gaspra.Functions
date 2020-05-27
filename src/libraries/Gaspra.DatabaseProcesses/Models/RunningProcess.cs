using Gaspra.DatabaseProcesses.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Gaspra.DatabaseProcesses.Models
{
    public class RunningProcess
    {
        public short SessionId { get; set; }
        //public DateTime StartTime { get; set; }
        public string Status { get; set; }
        public string Command { get; set; }
        public string Database { get; set; }
        public string Schema { get; set; }
        public string Object { get; set; }
        public int WaitTime { get; set; }

        public RunningProcess(
            short sessionId,
            //DateTime startTime,
            string status,
            string command,
            string database,
            string schema,
            string @object,
            int waitTime)
        {
            SessionId = sessionId;
            //StartTime = startTime;
            Status = status;
            Command = command;
            Database = database;
            Schema = schema;
            Object = @object;
            WaitTime = waitTime;
        }

        public async static Task<IEnumerable<RunningProcess>> FromDataReader(SqlDataReader dataReader)
        {
            var runningProcesses = new List<RunningProcess>();

            while(await dataReader.ReadAsync())
            {
                runningProcesses.Add(new RunningProcess(
                    dataReader[nameof(SessionId)].GetValue<short>(),
                    //dataReader[nameof(StartTime)].GetValue<DateTime>(),
                    dataReader[nameof(Status)].GetValue<string>(),
                    dataReader[nameof(Command)].GetValue<string>(),
                    dataReader[nameof(Database)].GetValue<string>(),
                    dataReader[nameof(Schema)].GetValue<string>(),
                    dataReader[nameof(Object)].GetValue<string>(),
                    dataReader[nameof(WaitTime)].GetValue<int>()
                ));
            }

            return runningProcesses;
        }
    }
}
