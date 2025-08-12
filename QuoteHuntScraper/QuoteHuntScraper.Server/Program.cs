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

var envPath = Path.Combine(
    Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName,
    ".env"
);
Env.Load(envPath);

builder.Services.Configure<ScraperSetting>(options =>
{
    options.Url = Environment.GetEnvironmentVariable("SCRAPER_URL") ?? string.Empty;
    options.Username = Environment.GetEnvironmentVariable("SCRAPER_USERNAME") ?? "user";
    options.Password = Environment.GetEnvironmentVariable("SCRAPER_PASSWORD") ?? "password";
});

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
