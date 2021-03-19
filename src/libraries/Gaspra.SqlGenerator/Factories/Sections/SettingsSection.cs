using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections
{
    public class SettingsSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new ScriptOrder(new[] { 0 });

        public SettingsSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IScriptVariableSet variables)
        {
            return Task.FromResult(true);
        }

        public async Task<string> Value(IScriptVariableSet variables)
        {
            var scriptLines = await _scriptLineFactory.LinesFrom(
                0,
                "SET NOCOUNT ON",
                "GO",
                "SET ANSI_NULLS ON",
                "GO",
                "SET QUOTED_IDENTIFIER ON",
                "GO");

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
