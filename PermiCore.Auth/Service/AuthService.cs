using Microsoft.EntityFrameworkCore;
using PermiCore.Auth.Database;
using PermiCore.Auth.Dtos.Response;
using PermiCore.Auth.Entity;

namespace PermiCore.Auth.Service
{
    public class AuthService(AuthDbContext authDbContext)
    {
        public async Task<Users?> GetUserById(string email, string password)
        {
            return await authDbContext.Users.AsNoTrackingWithIdentityResolution()
                   .Include(y => y.UserPermissions)
                   .ThenInclude(y => y.Permission)
                   .FirstOrDefaultAsync(x => x.Email == email && x.Password == password);
        }
        public async Task LoginAsync(int userID, string permissionJson, Guid authUserKey)
        {
            authDbContext.LoginUser.Add(new LoginUser() { UserId = userID, PermissionJson = permissionJson, AuthUserKey = authUserKey });
            await authDbContext.SaveChangesAsync();
        }
        public async Task<List<GetAllUserPermissionResponseDto>> GetAllUserPermissions()
        {
            return await authDbContext.Users.AsNoTrackingWithIdentityResolution()
                 .Include(y => y.UserPermissions)
                 .ThenInclude(y => y.Permission)
                 .Select(y=> new GetAllUserPermissionResponseDto()
                 {
                     UserId=y.Id,
                     Permissions = y.UserPermissions.Select(y=>y.Permission.Permission).ToList()
                 })
                 .ToListAsync();
        }
    }
}
