# CurrencyConversionAPI
Create a new ASP.net we app using .net 8.0 
# API Testing
To fetch the all the currency along with rates:
GET https://localhost:44323/api/currency/rates
[image](https://github.com/user-attachments/assets/c7bf4070-23a9-4f55-9850-0ea585e6cb50)

# To fetch and store all the data to table 
POST https://localhost:44323/api/currency/fetch-rates (along  with auth token which we will get from login endpoint)
[image](https://github.com/user-attachments/assets/8b8a849b-0574-4469-b96e-748ddb481b12)

# To convert amount fromcurrent to Tocurrency
https://localhost:44323/api/currency/convert (along  with auth token which we will get from login endpoint)
BODY:
{
  "FromCurrency": "AUD",
  "ToCurrency": "DKK",
  "Amount": 100
}
[image](https://github.com/user-attachments/assets/be53015e-b00d-457e-bab0-696fb6cfb511)

# TO get auth token:
POST https://localhost:44323/api/auth/login
BODY:
{
  "Username": "admin",
  "Password": "password123"
}
[image](https://github.com/user-attachments/assets/191b3e40-5910-42cf-938c-17c636225199)
