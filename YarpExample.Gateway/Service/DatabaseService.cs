using Microsoft.EntityFrameworkCore;
using ServiceStack;
using YarpExample.Gateway.Database;
using YarpExample.Gateway.Entity;

namespace YarpExample.Gateway.Service
{
    public class DatabaseService(GatewayDbContext gatewayDbContext)
    {
        public async Task<bool> SearchPermission(IEnumerable<ServicesPermissions> servicePermissions, List<string> userPermissionsName)
        {
            if (servicePermissions == null || !servicePermissions.Any() ||
                userPermissionsName == null || !userPermissionsName.Any())
                return false;

            if (await servicePermissions.AllAsync(async servicePermission => servicePermission.PermissionId == 0)) //Servisin bütün permissionları parent
                return servicePermissions.IntersectBy(userPermissionsName, sp => sp.Permission.Permission).Any();

            var childUserPerm = gatewayDbContext.Permissions
                .Where(y => y.PermissionId.HasValue &&
                            gatewayDbContext.Permissions
                                .Where(x => userPermissionsName.Contains(x.Permission))
                                .Select(x => x.Id)
                                .Contains(y.PermissionId.Value));

            if (!childUserPerm.Any())
                return servicePermissions.IntersectBy(userPermissionsName, sp => sp.Permission.Permission).Any();

            if (servicePermissions.IntersectBy(childUserPerm.Select(y => y.Id), x => x.PermissionId).Any())
                return true;

            if (await SearchPermission(servicePermissions, childUserPerm.Select(y => y.Permission).ToList()))
                return true;

            return false;
        }
    }
}
