using QuoteHuntWebAPI.DTO;
using QuoteHuntWebAPI.Services;
using QuoteHuntWebAPI.Services.Interfaces;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ScraperSetting>(builder.Configuration.GetSection("ScraperSettings"));
builder.Services.Configure<RedisSetting>(builder.Configuration.GetSection("Redis"));

if (builder.Configuration.GetValue<bool>("Redis:UseRedis"))
{
    var redisHost = builder.Configuration.GetValue<string>("Redis:Host");
    var redisPort = builder.Configuration.GetValue<int>("Redis:Port");
    var redisConnection = $"{redisHost}:{redisPort}";
    var redis = ConnectionMultiplexer.Connect(redisConnection);
    builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
}

#region Services
builder.Services.AddHttpClient<ScraperClient>();
builder.Services.AddScoped<IScraperClient, ScraperClient>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
