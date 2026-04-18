namespace YarpExample.Gateway.Entity
{
    public class ServicesPermissions
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public int PermissionId { get; set; }
        public Services Service { get; set; }
        public Permissions Permission { get; set; }
    }
}
