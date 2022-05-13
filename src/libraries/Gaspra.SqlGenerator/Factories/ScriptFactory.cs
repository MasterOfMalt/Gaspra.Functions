using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.SqlGenerator.Extensions;
using Gaspra.SqlGenerator.Interfaces;

namespace Gaspra.SqlGenerator.Factories
{
    public class MergeScriptFactory : IScriptFactory<IMergeScriptVariableSet>
    {
        private readonly IReadOnlyCollection<IScriptSection<IMergeScriptVariableSet>> _scriptSections;

        public MergeScriptFactory(IEnumerable<IScriptSection<IMergeScriptVariableSet>> scriptSections)
        {
            _scriptSections = scriptSections
                .ToList()
                .OrderSections();
        }

        public async Task<string> ScriptFrom(IMergeScriptVariableSet scriptVariableSet)
        {
            var script = "";

            foreach (var section in _scriptSections)
            {
                if (await section.Valid(scriptVariableSet))
                {
                    script += $"{await section.Value(scriptVariableSet)}{Environment.NewLine}{Environment.NewLine}";
                }
            }

            return script;
        }
    }

    public class DeltaScriptFactory : IScriptFactory<IDeltaScriptVariableSet>
    {
        private readonly IReadOnlyCollection<IScriptSection<IDeltaScriptVariableSet>> _scriptSections;

        public DeltaScriptFactory(IEnumerable<IScriptSection<IDeltaScriptVariableSet>> scriptSections)
        {
            _scriptSections = scriptSections
                .ToList()
                .OrderSections();
        }

        public async Task<string> ScriptFrom(IDeltaScriptVariableSet scriptVariableSet)
        {
            var script = "";

            foreach (var section in _scriptSections)
            {
                if (await section.Valid(scriptVariableSet))
                {
                    script += $"{await section.Value(scriptVariableSet)}{Environment.NewLine}{Environment.NewLine}";
                }
            }

            return script;
        }
    }
}
