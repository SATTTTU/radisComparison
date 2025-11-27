using StackExchange.Redis;
using System.Text.Json;

namespace ConsoleApplication
{
    public static class RedisCache
    {
        private static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379");

        private static readonly IDatabase cache = redis.GetDatabase();

        public static async Task SetAsync(string key, object value)
        {
            string json = JsonSerializer.Serialize(value);
            await cache.StringSetAsync(key, json);
        }

        public static async Task<T?> GetAsync<T>(string key)
        {
            string data = await cache.StringGetAsync(key);

            if (string.IsNullOrEmpty(data))
                return default;

            return JsonSerializer.Deserialize<T>(data);
        }
    }
}
