using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using YarpExample.Gateway.Database;
using YarpExample.Gateway.Dtos.Response;
using YarpExample.Gateway.Entity;

namespace YarpExample.Gateway.Service
{
    public class GatewayService(GatewayDbContext gatewayDbContext)
    {
        public async Task<bool> SearchPermission(List<string> servicePermissions, List<string> userPermissionsName)
        {
            if (servicePermissions == null || !servicePermissions.Any())
                return true;

            if (userPermissionsName == null || !userPermissionsName.Any())
                return false;

            var childUserPerm = gatewayDbContext.Permissions
                .Where(y => y.PermissionId.HasValue &&
                            gatewayDbContext.Permissions
                                .Where(x => userPermissionsName.Contains(x.Permission))
                                .Select(x => x.Id)
                                .Contains(y.PermissionId.Value));

            if (!childUserPerm.Any())
                return servicePermissions.IntersectBy(userPermissionsName, sp => sp).Any();

            if (servicePermissions.IntersectBy(userPermissionsName, sp => sp).Any())
                return true;

            if (await SearchPermission(servicePermissions, childUserPerm.Select(y => y.Permission).ToList()))
                return true;

            return false;
        }
    }
}
