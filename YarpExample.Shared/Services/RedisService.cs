using YarpExample.Shared.Dtos;

namespace YarpExample.Shared.Services
{
    internal class RedisService
    {
        public void ReadRedis(string userKey, out AuthRedisResponseDto data)
        {
            // Redis'ten veriyi okuyun ve data'ya atayın
            // Örnek olarak, Redis'ten okunan veriyi simüle edelim
            data = new AuthRedisResponseDto();
        }
    }
}
