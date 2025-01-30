
using CurrencyApp.Models;

namespace CurrencyApp.Services
{
    public class CurrencyRateScheduledService: BackgroundService
    {
        //private readonly ICurrencyRateService _currencyRateService;
        private readonly ILogger<CurrencyRateScheduledService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public CurrencyRateScheduledService(ILogger<CurrencyRateScheduledService> logger, IServiceScopeFactory serviceScopeFactory)
        {
           // _currencyRateService = currencyRateService;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Currency Rate Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //Creates a scoped service as you cannot directly inject a scoped service into singleton.
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        _logger.LogInformation("Fetching and updating currency rates...");
                        var currencyRateService = scope.ServiceProvider.GetRequiredService<ICurrencyRateService>();
                        await currencyRateService.FetchAndStoreRatesAsync();
                        _logger.LogInformation("Currency rates updated successfully.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error fetching currency rates: {ex.Message}");
                }

                // Wait for 60 minutes before the next update
                await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken);
            }
        }
    }
}
