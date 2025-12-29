using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Payments.Application.Services;

namespace Payments.Worker.Services;

public class ExpirationWorkerService : BackgroundService
{
    private readonly ILogger<ExpirationWorkerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

    public ExpirationWorkerService(
        ILogger<ExpirationWorkerService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Expiration Worker Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);

                // Crear un scope para resolver servicios scoped
                using (var scope = _serviceProvider.CreateScope())
                {
                    var paymentIntentService = scope.ServiceProvider
                        .GetRequiredService<IPaymentIntentService>();

                    var expiredCount = await paymentIntentService.ExpirePendingAsync();

                    if (expiredCount > 0)
                    {
                        _logger.LogInformation("Expired {Count} payment intents", expiredCount);
                    }
                }

                await Task.Delay(_interval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing worker");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("Expiration Worker Service stopped");
    }
}
