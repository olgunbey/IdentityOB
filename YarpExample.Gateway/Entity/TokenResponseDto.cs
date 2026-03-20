namespace YarpExample.Gateway.Entity
{
    public class TokenResponseDto
    {
        public required string AccessToken { get; set; }
        public required DateTime TokenLifeTime { get; set; }
    }
}
