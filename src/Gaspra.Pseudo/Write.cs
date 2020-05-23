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

        public Write(ILogger<Write> logger)
        {
            this.logger = logger;
        }

        public async Task Output(string text)
        {
            await Task.Run(() =>
            {
                logger.LogDebug(text);
                logger.LogInformation(text);
                logger.LogWarning(text);
                logger.LogError(text);
                logger.LogCritical(text);
            });
        }
    }
}
