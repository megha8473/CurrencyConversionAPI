using CurrencyApp.Models;
using CurrencyConversionAPI.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using CurrencyConversionAPI.Services;
using Microsoft.AspNetCore.Authorization;
using System.Data.Common;

namespace CurrencyConversionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private readonly IDatabase _database;
        private readonly ICurrencyRateService _currencyRateService;
        private readonly ILogger<CurrencyController> _logger;
        public CurrencyController(IDatabase database, ICurrencyRateService currencyRateService,ILogger<CurrencyController> logger)
        {
            _database = database;
            _currencyRateService = currencyRateService; 
            _logger = logger;
        }

        [HttpGet("rates")]
        [Authorize]
        public async Task<IActionResult> GetRates()
        {
            _logger.LogInformation("Fetching data ...");
            _logger.LogDebug("Fetching data ...");
            var data = await _database.ExecuteQueryAsync("SELECT CurrencyCode, RateToDKK FROM CurrencyRates");
            return Ok(data);
          /* var result = new List<Dictionary<string, object>>();
            foreach (DataRow row in data.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in data.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                result.Add(dict);
            }

            return Ok(result);*/
        }
        

        [HttpPost("convert")]
        [Authorize]
        public async Task<IActionResult> ConvertCurrency([FromBody] Conversion request)
        {
            var rateData = await _database.ExecuteQueryAsync(
                "SELECT RateToDKK FROM CurrencyRates WHERE CurrencyCode = @CurrencyCode",
                new DbParameter[] { new SqlParameter("@CurrencyCode", request.FromCurrency) });

            if (rateData.Rows.Count == 0) return NotFound("Currency not found");

            var rate = Convert.ToDecimal(rateData.Rows[0]["RateToDKK"]);
            var convertedAmount = request.Amount * rate;
            request.ConversionDate = DateTime.UtcNow;
            await _database.ExecuteNonQueryAsync(
                "INSERT INTO Conversions (FromCurrency, ToCurrency, Amount, ConvertedAmount, ConversionDate) VALUES (@FromCurrency, @ToCurrency, @Amount, @ConvertedAmount, @ConversionDate)",
                new DbParameter[]
                {
                    new SqlParameter("@FromCurrency", request.FromCurrency),
                    new SqlParameter("@ToCurrency", request.ToCurrency),
                    new SqlParameter("@Amount", request.Amount),
                    new SqlParameter("@ConvertedAmount", convertedAmount),
                    new SqlParameter("@ConversionDate", request.ConversionDate)
                });

            return Ok(new ConversionResult { ConvertedAmount = convertedAmount });
        }
       
        [HttpPost("fetch-rates")]
        [Authorize]
        public async Task<IActionResult> FetchAndStoreRates()
        {
            try
            {
                await _currencyRateService.FetchAndStoreRatesAsync();
                return Ok("Currency rates fetched and stored successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        public class ConversionResult
        {
            public decimal ConvertedAmount { get; set; }
        }

    }

   
}
