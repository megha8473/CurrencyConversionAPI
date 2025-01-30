using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using CurrencyApp.Models;
using CurrencyConversionAPI.Controllers;
using CurrencyConversionAPI.Helpers;
using CurrencyConversionAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using static CurrencyConversionAPI.Controllers.CurrencyController;

namespace CurrencyConversionAppTests.Controllers
{
    public class CurrencyControllerTests
    {
        private readonly Mock<IDatabase> _mockDatabase;
        private readonly Mock<ICurrencyRateService> _mockCurrencyRateService;
        private readonly Mock<ILogger<CurrencyController>> _logger;
        private readonly CurrencyController _controller;

        public CurrencyControllerTests()
        {
            _mockDatabase = new Mock<IDatabase>();
            _mockCurrencyRateService = new Mock<ICurrencyRateService>();
            _controller = new CurrencyController(_mockDatabase.Object, _mockCurrencyRateService.Object, _logger.);
        }
        [Fact]
        public async Task GetRates_ReturnsOkResult()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("CurrencyCode");
            dataTable.Columns.Add("RateToDKK");
            dataTable.Rows.Add("USD", 7.5);
            dataTable.Rows.Add("EUR", 7.4);
            _mockDatabase.Setup(db => db.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<DbParameter[]>())).ReturnsAsync(dataTable);
            var result =await  _controller.GetRates();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<DataTable>(okResult.Value);
            Assert.Equal(2, returnValue.Rows.Count);


        }

        [Fact]
        public async Task ConvertCurrency_ReturnsZeroResult()
        {
            var conversion = new Conversion
            {
                FromCurrency = "abc",
                ToCurrency = "DKK",
                Amount = 100
            };
            var dataTable = new DataTable();
           // dataTable.Columns.Add("RateToDKK");
            //dataTable.Rows.Add(7.5);
            _mockDatabase.Setup(db => db.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<DbParameter[]>())).ReturnsAsync(dataTable);
            var result = await _controller.ConvertCurrency(conversion);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Currency not found", notFoundResult.Value);
        }

        [Fact]
        public async Task ConvertCurrency_ReturnsOKResult()
        {
            var conversion = new Conversion
            {
                FromCurrency = "USD",
                ToCurrency = "DKK",
                Amount = 100
            };
            var dataTable = new DataTable();
             dataTable.Columns.Add("RateToDKK");
            dataTable.Rows.Add(7.5);
            _mockDatabase.Setup(db => db.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<DbParameter[]>())).ReturnsAsync(dataTable);
            var result = await _controller.ConvertCurrency(conversion);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ConversionResult>(okResult.Value);
            Assert.Equal(750, returnValue.ConvertedAmount);
        }

        [Fact]
        public async Task FetchAndStoreRates_ReturnsOKResult()
        {
            var conversion = new Conversion
            {
                FromCurrency = "USD",
                ToCurrency = "DKK",
                Amount = 100
            };
            var dataTable = new DataTable();
            dataTable.Columns.Add("RateToDKK");
            dataTable.Rows.Add(7.5);
            _mockCurrencyRateService.Setup(db => db.FetchAndStoreRatesAsync()).Returns(Task.CompletedTask);
            var result = await _controller.FetchAndStoreRates();
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Currency rates fetched and stored successfully.", okResult.Value);
        }

    }
}
