using MassTransit;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace SagaWhenEnterDispatchIssue
{
    public class BusHostedService : IHostedService
    {
        private readonly IBusControl _bus;
        public BusHostedService(IBusControl bus) => _bus = bus;

        public async Task StartAsync(CancellationToken cancellationToken) => await _bus.StartAsync(cancellationToken).ConfigureAwait(false);
        public async Task StopAsync(CancellationToken cancellationToken) => await _bus.StopAsync(cancellationToken).ConfigureAwait(false);
    }
}
