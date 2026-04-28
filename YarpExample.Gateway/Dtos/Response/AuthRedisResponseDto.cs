namespace YarpExample.Gateway.Dtos.Response
{
    public class AuthRedisResponseDto
    {
        public int UserId { get; set; }
        public List<string> Permissions { get; set; }
    }
}
