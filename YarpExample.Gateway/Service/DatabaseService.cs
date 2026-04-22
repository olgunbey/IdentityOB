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

            var serviceChildPerm = servicePermissions.Where(y => y.PermissionId != 0).ToList();

            if (!serviceChildPerm.Any()) 
                return true;

            var userPermissionList = gatewayDbContext.Permissions.AsEnumerable().IntersectBy(userPermissionsName, x => x.Permission).ToList();

            if (!userPermissionList.Any())
                return false;

            foreach (var permission in userPermissionList)
            {
                var childUserPerm = gatewayDbContext.Permissions.Where(y => y.PermissionId == permission.Id).ToList();

                if (childUserPerm.Any())
                {
                    if (serviceChildPerm.IntersectBy(childUserPerm.Select(y => y.PermissionId), x => x.PermissionId).Any())
                        return true;

                    if (await SearchPermission(serviceChildPerm, childUserPerm.Select(y => y.Permission).ToList()))
                        return true;
                }
                else
                {
                    return servicePermissions.SingleOrDefault(x => x.PermissionId == permission.Id) != null;
                }


            }

            return false;
        }
    }
}
