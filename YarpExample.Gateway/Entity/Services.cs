namespace YarpExample.Gateway.Entity
{
    public class Services
    {
        public int Id { get; set; }
        public string RequestPath { get; set; }
        public ICollection<ServicesPermissions> ServicesPermissions { get; set; }

    }
}
