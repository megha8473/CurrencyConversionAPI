namespace CurrencyApp.Models
{
    public class CurrencyRates
    {
        public int Id { get; set; }
        public string CurrencyCode { get; set; }
        public decimal RateToDKK { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
