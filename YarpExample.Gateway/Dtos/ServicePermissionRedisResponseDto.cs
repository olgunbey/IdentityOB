namespace YarpExample.Gateway.Dtos
{
    public class ServicePermissionRedisResponseDto
    {
        public string RequestPath { get; set; }
        public List<int> PermissionsId { get; set; }
    }
}
