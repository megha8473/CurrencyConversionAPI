## Database Setup

### Prerequisites
- SQL Server management studio installed  and also installed sql server express to run locally.
- Connection string configured in `appsettings.json`.
 "ConnectionStrings": {
        "DefaultConnection": "Server=localhost;Database=CurrencyDb;Encrypt=True;TrustServerCertificate=True;Trusted_Connection=True;"
    },

### Steps to Set Up the Database
1.Run below queries to set up database and tables:
   CREATE DATABASE CurrencyDB;

   CREATE TABLE CurrencyRates (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CurrencyCode NVARCHAR(10) NOT NULL,
    RateToDKK DECIMAL(6, 5) NOT NULL,
    LastUpdated DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Conversions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FromCurrency NVARCHAR(10) NOT NULL,
    ToCurrency NVARCHAR(10) NOT NULL,
    Amount DECIMAL(10, 6) NOT NULL,
    ConvertedAmount DECIMAL(18, 6) NOT NULL,
    ConversionDate DATETIME NOT NULL DEFAULT GETDATE()
);

### created index
CREATE INDEX idx_currency_code ON CurrencyRates (CurrencyCode);
