using Gaspra.Functions.Correlation;
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
                var parameters = function.Parameters.Select(p => $"-{p.Key} (Optional: {p.Optional}) {p.About}");

                helpInformation.Add($"FUNCTION:");

                helpInformation.Add($"    {function.GetType().Name}");

                helpInformation.Add($"    {function.About}");

                helpInformation.Add($"ALIASES:");

                helpInformation.AddRange(function.Aliases.Select(a => $"    {a}"));

                helpInformation.Add($"PARAMETERS:");

                helpInformation.AddRange(parameters.Select(p => $"    {p}"));

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

        public IReadOnlyCollection<string> Aliases => new string[] { "help", "h" };

        public IReadOnlyCollection<IFunctionParameter> Parameters => new List<FunctionParameter>();

        public string About => string.Join($"{Environment.NewLine}", helper.BuildHelpMessage());

        public bool ValidateParameters(IReadOnlyCollection<IFunctionParameter> parameters) => true;

        public async Task Run(CancellationToken cancellationToken, IReadOnlyCollection<IFunctionParameter> parameters)
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
