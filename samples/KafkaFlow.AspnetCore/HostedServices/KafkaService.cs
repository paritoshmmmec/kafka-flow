using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KafkaFlow.AspnetCore.HostedServices
{
    public class KafkaHostedService : IHostedService
    {
        private readonly ILogger<KafkaHostedService> logger;
        private readonly IKafkaBus bus;

        public KafkaHostedService(IServiceProvider provider,
            ILogger<KafkaHostedService> logger)
        {
            this.logger = logger;
            bus = provider.CreateKafkaBus();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting the bus");
            await bus.StartAsync(cancellationToken);
            logger.LogInformation("Bus Started");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping the bus");
            await bus.StopAsync();
            logger.LogInformation("Bus Stopped");
        }
    }
}
