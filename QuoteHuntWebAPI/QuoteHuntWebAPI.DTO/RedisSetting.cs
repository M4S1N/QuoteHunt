namespace QuoteHuntWebAPI.DTO
{
    public class RedisSetting
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 6379;
        public bool UseRedis { get; set; } = false;
        public int CacheTTLSeconds { get; set; } = 300;
    }
}
