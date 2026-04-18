namespace YarpExample.Gateway.Entity
{
    public class Permissions
    {
        public int Id { get; set; }
        public string Permission { get; set; }
        public int? PermissionId { get; set; }
        public ICollection<ServicesPermissions> ServicesPermissions { get; set; }
    }
}
