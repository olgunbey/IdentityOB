namespace YarpExample.Gateway.Entity
{
    public class ConnectTokenRequestDto
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string GrantType { get; set; }
    }
}
