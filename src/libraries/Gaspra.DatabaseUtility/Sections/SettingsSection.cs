using Gaspra.DatabaseUtility.Interfaces;
using Gaspra.DatabaseUtility.Models.Script;
using System.Threading.Tasks;

namespace Gaspra.DatabaseUtility.Sections
{
    public class SettingsSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new ScriptOrder(new[] { 0, 0 });

        public SettingsSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IScriptVariables variables)
        {
            return Task.FromResult(true);
        }

        public async Task<string> Value(IScriptVariables variables)
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
