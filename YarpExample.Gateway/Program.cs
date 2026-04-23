using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Transforms;
using YarpExample.Gateway.Database;
using YarpExample.Gateway.RedisService;
using YarpExample.Gateway.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<RedisService>();
builder.Services.AddScoped<DatabaseService>();
builder.Services.AddDbContext<GatewayDbContext>(y => y.UseNpgsql("Host=localhost;Port=5432;Username=olgunbey;Password=sahinbey;Database=GatewayDbContext"));
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(context =>
    {
        context.AddRequestTransform(async tContext =>
        {
            using (ServiceProvider serviceProvider = builder.Services.BuildServiceProvider())
            {
                var httpContext = tContext.HttpContext;
                httpContext.Request.Headers.TryGetValue("X-User-Key", out StringValues val);
                string value = val.ToString();
                if (string.IsNullOrWhiteSpace(val))
                {
                    httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                RedisService redisService = serviceProvider.GetService<RedisService>()!;
                var hasAuthUser = await redisService.ReadRedis(value);

                if (hasAuthUser == null) //Bu kullanıcı rediste yok
                {
                    httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
                if (hasAuthUser != null && hasAuthUser.LifeTime < DateTime.UtcNow) //Bu kullanıcı rediste var ama süresi dolmuş
                {
                    await redisService.UpdateUserRedis(value); //Burada kullanıcının süresini güncelliyoruz
                }
                GatewayDbContext gatewayDbContext = serviceProvider.GetService<GatewayDbContext>()!;
                DatabaseService databaseService = serviceProvider.GetService<DatabaseService>()!;

                var path = tContext.HttpContext.Request.Path;


                try
                {
                    var servicePermissions =await gatewayDbContext.ServicesPermissions.
                             AsNoTrackingWithIdentityResolution().
                             Include(y => y.Service).
                             Include(y => y.Permission).
                             Where(x => x.Service.RequestPath == path.ToString().ToLower()).ToListAsync();
                    if (!await databaseService.SearchPermission(servicePermissions, hasAuthUser!.Permissions))
                    {
                        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return;
                    }
                }
                catch (Exception)
                {

                    throw;
                }
              
            }

        });
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseForwardedHeaders();
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();
app.MapControllers();
app.Run();
