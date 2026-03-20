namespace YarpExample.Gateway.RedisService
{
    public class Service
    {
        public T ReadRedis<T>(string key)
        {
            // Simulate reading from Redis
            // In a real implementation, you would use a Redis client library to fetch the value
            // For demonstration purposes, we will return a default value
            return default(T);
        }
    }
}
