namespace PermiCore.Auth.Entity
{
    public class LoginUser
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public Guid AuthUserKey { get; set; }
        public string PermissionJson { get; set; }
    }
}
