namespace PermiCore.Auth.Entity
{
    public class UserPermissions
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PermissionId { get; set; }
        public Users User { get; set; }
        public Permissions Permission { get; set; }
    }
}
