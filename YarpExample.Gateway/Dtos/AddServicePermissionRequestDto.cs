namespace YarpExample.Gateway.Dtos
{
    public class AddServicePermissionRequestDto
    {
        public int RequestPathId { get; set; }
        public List<int> PermissionsId { get; set; }
    }
}
