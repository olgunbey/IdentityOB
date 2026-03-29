namespace YarpExample.Gateway.RedisService
{
    public class RedisService
    {
        public void ReadRedis<T>(string key, out T data)
        {
            // Simulate reading from Redis
            // In a real implementation, you would use a Redis client library to fetch the value
            // For demonstration purposes, we will return a default value
            data = default(T);
        }
    }
}
