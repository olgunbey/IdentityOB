using Microsoft.EntityFrameworkCore;
using PermiCore.Auth.Database;
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
        public async Task<Users?> LoginAsync(string email, string password)
        {
            return await authDbContext.Users.FirstOrDefaultAsync(y => y.Email == email && y.Password == password);
        }
    }
}
