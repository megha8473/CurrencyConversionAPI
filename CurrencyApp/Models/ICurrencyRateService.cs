namespace CurrencyApp.Models
{
    public interface ICurrencyRateService
        {
            Task FetchAndStoreRatesAsync();
        }
    
}
