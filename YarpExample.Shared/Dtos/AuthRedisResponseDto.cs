namespace YarpExample.Shared.Dtos
{
    internal class AuthRedisResponseDto
    {
        public string UserId { get; set; }
        public string UserKey { get; set; }
        public List<string> Permissions { get; set; }
        public DateTime LifeTime { get; set; }
    }
}
