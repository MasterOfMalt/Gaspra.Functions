using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Gaspra.Database.Extensions;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Delta
{
    public class AboutSection : IScriptSection<IDeltaScriptVariableSet>
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order => new ScriptOrder(new[] { 0, 3 });

        public AboutSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IDeltaScriptVariableSet variableSet)
        {
            return Task.FromResult(true);
        }

        public async Task<string> Value(IDeltaScriptVariableSet variableSet)
        {
            var aboutText = new List<string>
            {
                $" ** [{variableSet.Schema.Name}].[{variableSet.ScriptName}]",
                $" **",
                $" ** About:",
                $" **     Retrieve the identifiers for any data points changed (inserted, updated, deleted)",
                $" **     over a given period of time. Use the ignore parameter table type to ignore specific",
                $" **     tables when looking at the changed identifiers.",
                $" **",
                $" ** Parameters:",
                $" **     @Delta [DATETIME],",
                $" **     @{variableSet.TableTypeVariableName} [{variableSet.Schema.Name}].[{variableSet.TableTypeName}] (",
                $" **         [Ignore] NVARCHAR(50) NOT NULL",
                $" **     )",
                $" **",
                $" ** Tables covered by delta:"
            };

            var tablesCovered = variableSet.TablePaths.SelectMany(tp => tp.Select(t => t.Name)).Distinct().OrderBy(dt => dt).ToList();

            foreach (var tableCovered in tablesCovered)
            {
                var trailingComma = tableCovered.Equals(tablesCovered.Last()) ? "" : ",";

                aboutText.Add($" **     {tableCovered}{trailingComma}");
            }

            var assemblyValue = Assembly
                .GetEntryAssembly()?
                .CustomAttributes
                .FirstOrDefault(a => a.AttributeType == typeof(AssemblyInformationalVersionAttribute))?
                .ConstructorArguments
                .FirstOrDefault()
                .Value;

            aboutText.AddRange(new List<string> {
                $"#pad",
                $" ** Gaspra.Functions v{assemblyValue}"
            });

            var affix = " **";

            var longestLine = aboutText
                .Select(t => t.Length)
                .OrderByDescending(t => t)
                .First();

            var start = "/" + new string('*', longestLine + 2);

            var end = " " + new string('*', longestLine + 2) + "/";

            var aboutLines = new List<string>
            {
                start
            };

            aboutLines
                .AddRange(aboutText.Select(t =>
                {
                    if (!t.Equals("#pad"))
                    {
                        return t + new string(' ', longestLine - t.Length) + affix;
                    }
                    else
                    {
                        return " " + new string('*', longestLine + 2 );
                    }
                }));

            aboutLines.Add(end);

            var scriptLines = await _scriptLineFactory.LinesFrom(
                0,
                aboutLines.ToArray());

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
