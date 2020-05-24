using Gaspra.Functions.Correlation.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Gaspra.Pseudo
{
    public interface IWrite
    {
        public Task Output(string text);
    }

    public class Write : IWrite
    {
        private readonly ILogger logger;
        private readonly ICorrelationContext cxt;

        public Write(
            ILogger<Write> logger,
            ICorrelationContext cxt)
        {
            this.logger = logger;
            this.cxt = cxt;
        }

        public async Task Output(string text)
        {
            await Task.Run(() =>
            {
                logger.LogDebug("{cxt} - {text}", cxt.CorrelationId, text);
                logger.LogInformation("{cxt} - {text}", cxt.CorrelationId, text);
                logger.LogWarning("{cxt} - {text}", cxt.CorrelationId, text);
                logger.LogError("{cxt} - {text}", cxt.CorrelationId, text);
                logger.LogCritical("{cxt} - {text}", cxt.CorrelationId, text);
            });
        }
    }
}
