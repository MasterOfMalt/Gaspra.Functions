using System.Collections.Generic;
using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Merge.Procedure
{
    public class RetentionSection : IScriptSection<IMergeScriptVariableSet>
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 1, 2, 3 });

        public RetentionSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IMergeScriptVariableSet variableSet)
        {
            return Task.FromResult(variableSet.RetentionPolicy.RetentionMonths != null);
        }

        public async Task<string> Value(IMergeScriptVariableSet variableSet)
        {
            var mergeStatement = new List<string>
            {
                $"WHEN NOT MATCHED BY SOURCE AND t.[{variableSet.RetentionPolicy.ComparisonColumn}] < DATEADD(MONTH, -{variableSet.RetentionPolicy.RetentionMonths}, GETUTCDATE()) AND t.[Deleted] IS NULL",
                "    THEN UPDATE SET",
                "        t.[Deleted]= GETUTCDATE()",
                "",

                $"WHEN NOT MATCHED BY SOURCE AND t.[Deleted] < DATEADD(MONTH, -{variableSet.RetentionPolicy.RetentionMonths}, GETUTCDATE())",
                "    THEN DELETE"
            };

            var scriptLines = await _scriptLineFactory.LinesFrom(
                1,
                mergeStatement.ToArray()
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
