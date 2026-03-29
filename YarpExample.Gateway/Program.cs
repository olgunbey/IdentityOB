using Yarp.ReverseProxy.Transforms;
using YarpExample.Gateway.Dtos;
using YarpExample.Gateway.RedisService;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(context =>
    {
        context.AddRequestTransform(async tContext =>
        {
            using (ServiceProvider serviceProvider = builder.Services.BuildServiceProvider())
            {
                var proxyRequest = tContext.ProxyRequest;
                proxyRequest.Headers.TryGetValues("X-User-Key", out var userKeyValues);
                string? userKey = userKeyValues!.SingleOrDefault();
                if (string.IsNullOrEmpty(userKey))
                {
                    tContext.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await tContext.HttpContext.Response.WriteAsJsonAsync(new AuthRedisNotFoundResponseDto() { LoginUrl = "http://localhost:5000" });
                    return;
                }

                RedisService redisService = serviceProvider.GetService<RedisService>()!;

                redisService.ReadRedis(userKey, out AuthRedisResponseDto data);

                if (data == null || (data != null && data.LifeTime < DateTime.UtcNow)) //Bu kullanıcı rediste yok veya rediste var ama süresi dolmuş
                {
                    tContext.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await tContext.HttpContext.Response.WriteAsJsonAsync(new AuthRedisNotFoundResponseDto() { LoginUrl = "http://localhost:5000" });
                    return;
                }
            }
        });
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
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
