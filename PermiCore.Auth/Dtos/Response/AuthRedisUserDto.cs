namespace PermiCore.Auth.Dtos.Response
{
    public class AuthRedisUserDto
    {
        public int UserId { get; set; }
        public List<string> Permissions { get; set; }
    }
}
