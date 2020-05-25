using Gaspra.Functions.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gaspra.Functions.Functions
{
    /*
     * IHelper solves circular dependencies
     */
    public interface IHelper
    {
        IEnumerable<string> BuildHelpMessage();
    }
    public class Helper : IHelper
    {
        private readonly IServiceProvider serviceProvider;

        public Helper(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IEnumerable<string> BuildHelpMessage()
        {
            var functions = serviceProvider.GetServices<IFunction>()
                .Where(f => !f.GetType().Equals(typeof(HelpFunction)));

            var helpInformation = new List<string>();

            foreach (var function in functions)
            {
                var aliases = string.Join($", ", function.FunctionAliases);

                helpInformation.Add($"[{function.GetType().Name}]");

                helpInformation.Add($"ALIASES: {aliases}");

                helpInformation.Add($"HELP: {function.FunctionHelp}");

                if (!function.Equals(functions.Last()))
                {
                    helpInformation.Add("");
                }
            }

            return helpInformation;
        }
    }

    public class HelpFunction : IFunction
    {
        private readonly ILogger logger;
        private readonly IHelper helper;

        public HelpFunction(
            ILogger<HelpFunction> logger,
            IHelper helper)
        {
            this.logger = logger;
            this.helper = helper;
        }

        public IEnumerable<string> FunctionAliases => new string[] { "help", "h" };

        public string FunctionHelp => string.Join($"{Environment.NewLine}", helper.BuildHelpMessage());

        public bool ValidateParameters() => true;

        public async Task Run(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                var helpInformation = helper.BuildHelpMessage();

                foreach (var help in helpInformation)
                {
                    logger.LogInformation(help);
                }
            });
        }


    }
}
