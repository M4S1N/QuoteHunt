using QuoteHuntWebAPI.DTO;
using QuoteHuntWebAPI.Services;
using QuoteHuntWebAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ScraperSetting>(
    builder.Configuration.GetSection("ScraperSettings")
);

#region Services
builder.Services.AddHttpClient<HttpScraperClient>();
builder.Services.AddScoped<IScraperClient, HttpScraperClient>();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
