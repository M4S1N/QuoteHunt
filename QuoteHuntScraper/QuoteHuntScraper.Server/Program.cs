using Microsoft.AspNetCore.Authentication;
using QuoteHuntScraper.Services;
using QuoteHuntScraper.Services.Interfaces;
using DotNetEnv;
using QuoteHuntScraper.DTO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ScraperSetting>(options =>
{
    options.Url = builder.Configuration["SCRAPER_URL"] ?? string.Empty;
    options.Username = builder.Configuration["SCRAPER_USERNAME"] ?? "user";
    options.Password = builder.Configuration["SCRAPER_PASSWORD"] ?? "password";
});

builder.Configuration.AddEnvironmentVariables();

#region Services
builder.Services.AddScoped<IScraperService, ScraperService>();
builder.Services.AddScoped<IWebDriverFactory, WebDriverFactory>();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
