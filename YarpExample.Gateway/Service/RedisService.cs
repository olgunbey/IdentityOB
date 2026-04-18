using ServiceStack.Redis;
using YarpExample.Gateway.Dtos;

namespace YarpExample.Gateway.RedisService
{
    public class RedisService
    {
        private readonly IRedisClientAsync redisClientAsync;
        public RedisService()
        {
            redisClientAsync = new RedisManagerPool("127.0.0.1:6379").GetClientAsync().Result; // Redis sunucusunun adresi ve portu
        }
        public async Task<AuthRedisResponseDto?> ReadRedis(string key)
        {
            var data =await redisClientAsync.GetAsync<List<AuthRedisResponseDto>>("AuthServer");

           return data.SingleOrDefault(y => y.UserKey == key);
        }
        public async Task UpdateUserRedis(string key)
        {
            var data = await redisClientAsync.GetAsync<List<AuthRedisResponseDto>>("AuthServer");

            if(data.Count() == 0)
                return;

            data.First(y => y.UserKey == key).LifeTime = DateTime.UtcNow.AddMinutes(30);

            await redisClientAsync.SetAsync("AuthServer", data);
        }
    }
}
