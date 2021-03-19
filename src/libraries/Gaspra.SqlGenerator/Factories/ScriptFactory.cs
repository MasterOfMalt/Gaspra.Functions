using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.SqlGenerator.Extensions;
using Gaspra.SqlGenerator.Interfaces;

namespace Gaspra.SqlGenerator.Factories
{
    public class ScriptFactory : IScriptFactory
    {
        private readonly IReadOnlyCollection<IScriptSection> _scriptSections;

        public ScriptFactory(IEnumerable<IScriptSection> scriptSections)
        {
            _scriptSections = scriptSections
                .ToList()
                .OrderSections();
        }

        public async Task<string> ScriptFrom(IScriptVariableSet scriptVariableSet)
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
