namespace PermiCore.Auth.Dtos.Response
{
    public class GetAllUserPermissionResponseDto
    {
        public int UserId { get; set; }
        public List<string> Permissions { get; set; }
    }
}
