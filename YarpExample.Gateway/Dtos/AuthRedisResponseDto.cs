namespace YarpExample.Gateway.Dtos
{
    public class AuthRedisResponseDto
    {
        public string UserKey { get; set; }
        public List<string> Permissions { get; set; }
        public DateTime LifeTime { get; set; }
    }
}
