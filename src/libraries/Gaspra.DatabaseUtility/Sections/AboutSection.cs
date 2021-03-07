using Gaspra.DatabaseUtility.Interfaces;
using Gaspra.DatabaseUtility.Models.Merge;
using Gaspra.DatabaseUtility.Models.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            var matchOn = variables.MergeIdentifierColumns.Select(c => c.Name);

            var deleteOn = variables.DeleteIdentifierColumns.Select(c => c.Name);

            var deleteOnFactId = matchOn.Where(m => !deleteOn.Any(d => d.Equals(m))).FirstOrDefault();

            var inserts = true;

            var updates = !matchOn.Count().Equals(variables.Table.Columns.Count) &&
                !variables.Table.Columns.Where(c => !c.IdentityColumn).Select(c => c.Name).All(n => matchOn.Any(m => m.Equals(n, StringComparison.InvariantCultureIgnoreCase)));

            var deletes = !string.IsNullOrWhiteSpace(deleteOnFactId) && deleteOn.Any();

            var retention = variables.RetentionPolicy != null;

            var retentionAmount = retention ? $"({variables.RetentionPolicy.RetentionMonths} months)" : "";

            var aboutText = new List<string>
            {
                $" ** [{variables.SchemaName}].[{variables.ProcedureName()}]",
                $" **",
                $" ** Generated stored procedure for the table: [{variables.SchemaName}].[{variables.Table.Name}]",
                $"#pad",
                $" ** Inserts: {inserts}",
                $" ** Updates: {updates}",
                $" ** Deletes: {deletes}",
                $" ** Retention policy: {retention} {retentionAmount}",
                $"#pad",
                $" ** Gaspra.Functions v{Assembly.GetEntryAssembly().GetName().Version}"
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
