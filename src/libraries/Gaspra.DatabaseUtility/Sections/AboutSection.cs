using Gaspra.DatabaseUtility.Interfaces;
using Gaspra.DatabaseUtility.Models.Merge;
using Gaspra.DatabaseUtility.Models.Script;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gaspra.DatabaseUtility.Sections
{
    public class AboutSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order => new ScriptOrder(new[] { 0, 3 });

        public AboutSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IScriptVariables variables)
        {
            return Task.FromResult(true);
        }

        public async Task<string> Value(IScriptVariables variables)
        {
            var aboutText = new List<string>
            {
                $" ** Generated stored procedure: [{variables.SchemaName}].[{variables.ProcedureName()}]",
                $" ** For table: [{variables.SchemaName}].[{variables.Table.Name}]",
            };

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
                .AddRange(aboutText
                    .Select(t => t + new string(' ', longestLine - t.Length) + affix));

            aboutLines.Add(end);

            var scriptLines = await _scriptLineFactory.LinesFrom(
                0,
                aboutLines.ToArray());

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
