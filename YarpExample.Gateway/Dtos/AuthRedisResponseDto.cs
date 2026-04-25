namespace YarpExample.Gateway.Dtos
{
    public class AuthRedisResponseDto
    {
        public int UserId { get; set; }
        public List<string> Permissions { get; set; }
    }
}
