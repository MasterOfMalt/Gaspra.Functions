using Gaspra.MergeSprocs.DataAccess.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Gaspra.MergeSprocs
{
    public class MergeSprocsService : BackgroundService
    {
        private readonly ILogger logger;
        private readonly IDataAccess dataAccess;

        public MergeSprocsService(
            IDataAccess dataAccess,
            ILogger<MergeSprocsService> logger)
        {
            this.dataAccess = dataAccess;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() =>
            {
                logger.LogInformation($"{nameof(MergeSprocsService)} cancellation token called");
            });

            while (!cancellationToken.IsCancellationRequested)
            {
                var tableInfo = await dataAccess.GetColumnInformation();

                logger.LogInformation("{tableinfo}", tableInfo);

                return;
            }


        }
    }
}