using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using System.Xml.Linq;
using CurrencyApp.Models;
using CurrencyConversionAPI.Helpers;
using Newtonsoft.Json;

namespace CurrencyConversionAPI.Services
{
    public class CurrencyRateService: ICurrencyRateService
    {
        private readonly IDatabase _database;

        public CurrencyRateService(IDatabase database)
        {
            _database = database;
        }

        public async Task FetchAndStoreRatesAsync()
        {
            using var client = new HttpClient();
            var response = await client.GetAsync("https://www.nationalbanken.dk/api/currencyratesxml?lang=da"); 
            response.EnsureSuccessStatusCode();
           

            var xmlContent = await response.Content.ReadAsStringAsync();

            // Parse the XML
            var xDoc = XDocument.Parse(xmlContent);
            var rates = new List<CurrencyRates>();

          
            foreach (var currencyElement in xDoc.Descendants("currency"))
            {
                var currencyCode = currencyElement.Attribute("code")?.Value;
                var rateToDKK = currencyElement.Attribute("rate")?.Value;

                if (!string.IsNullOrEmpty(currencyCode) && !string.IsNullOrEmpty(rateToDKK))
                {
                    rateToDKK = rateToDKK.Replace(",", ".");
                    if (decimal.TryParse(rateToDKK, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var rate))
                    {
                        rates.Add(new CurrencyRates
                        {
                            CurrencyCode = currencyCode,
                            RateToDKK = rate
                        });
                    }
                }
            }

            // Store in database
            foreach (var rate in rates)
            {
                string query = @"
                    IF EXISTS (SELECT 1 FROM CurrencyRates WHERE CurrencyCode = @CurrencyCode)
                        UPDATE CurrencyRates 
                        SET RateToDKK = @RateToDKK, LastUpdated = GETDATE() 
                        WHERE CurrencyCode = @CurrencyCode
                    ELSE
                        INSERT INTO CurrencyRates (CurrencyCode, RateToDKK, LastUpdated) 
                        VALUES (@CurrencyCode, @RateToDKK, GETDATE())";

                var parameters = new DbParameter[]
                {
                    CreateParameter("@CurrencyCode", rate.CurrencyCode),
                    CreateParameter("@RateToDKK", rate.RateToDKK)
                };

                await _database.ExecuteNonQueryAsync(query, parameters);
            }
        }

        private DbParameter CreateParameter(string name, object value)
        {
            var parameter = _database.DbProviderFactory.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            return parameter;
        }
    }

   
}
