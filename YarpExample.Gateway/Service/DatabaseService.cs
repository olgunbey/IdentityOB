using Microsoft.EntityFrameworkCore;
using ServiceStack;
using YarpExample.Gateway.Database;
using YarpExample.Gateway.Entity;

namespace YarpExample.Gateway.Service
{
    public class DatabaseService(GatewayDbContext gatewayDbContext)
    {
        public async Task<bool> SearchPermission(List<ServicesPermissions> servicePermissions, List<string> userPermissionsName)
        {
            if (servicePermissions == null || !servicePermissions.Any() ||
                userPermissionsName == null || !userPermissionsName.Any())
            {
                return false;
            }

            if (await servicePermissions.AllAsync(async servicePermission => servicePermission.PermissionId == 0))
            {
                return servicePermissions.IntersectBy(userPermissionsName, sp => sp.Permission.Permission).Any();
            }

            var userPermissionList = gatewayDbContext.Permissions.AsEnumerable().IntersectBy(userPermissionsName, x => x.Permission).ToList();

            if (!userPermissionList.Any())
                return false;

            foreach (var permission in userPermissionList)
            {
                var childUserPerm = gatewayDbContext.Permissions.Where(y => y.PermissionId == permission.Id).ToList();

                if (childUserPerm.Any())
                {
                    if (servicePermissions.IntersectBy(childUserPerm.Select(y => y.Id), x => x.PermissionId).Any())
                        return true;
                }

                if (await SearchPermission(servicePermissions, childUserPerm.Select(y => y.Permission).ToList()))
                    return true;
            }
            return false;
        }
    }
}
