using System.Data.Common;
using System.Text;
using CurrencyApp.Models;
using CurrencyApp.Services;
using CurrencyConversionAPI.Helpers;
using CurrencyConversionAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using static System.Net.WebRequestMethods;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddLogging();
DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", Microsoft.Data.SqlClient.SqlClientFactory.Instance);
builder.Services.AddAuthentication();

builder.Services.AddScoped<ICurrencyRateService, CurrencyRateService>();
//This tells Swagger to generate documentation for version v1 of our API.
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Currency Conversion API",
        Version = "v1",
        Description = "API for managing currency rates and performing conversions.",
    });
});
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddScoped<IDatabase>(provider => new Database(connectionString));
//builder.Services.AddScoped<Database>(provider => new Database(connectionString));

//builder.Services.AddScoped<CurrencyRateService>();
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
//this is for background service implementation
//builder.Services.AddHostedService<CurrencyRateScheduledService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true; // set to false if using HTTP for development
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero, // Prevents time-based issues
            ValidIssuer = builder.Configuration["Jwt:Issuer"], // Default value; can be your app's name or domain
            ValidAudience = builder.Configuration["Jwt:Audience"],  // Default value; can be your app's name or API
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))  // Secret key to sign tokens
        };
    });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSwagger();
//Swagger middleware
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Currency Conversion API v1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
});
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
//app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
