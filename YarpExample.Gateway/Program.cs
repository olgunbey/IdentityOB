using Yarp.ReverseProxy.Transforms;
using YarpExample.Gateway.Dtos;
using YarpExample.Gateway.RedisService;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<RedisService>();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(context =>
    {
        context.AddRequestTransform(async tContext =>
        {
            using (ServiceProvider serviceProvider = builder.Services.BuildServiceProvider())
            {
                var httpContext = tContext.HttpContext;
                var headerDictionary = httpContext.Request.Headers;
                string? userKeyValue = tContext.HttpContext.Request.Cookies["X-User-Key"];
                if (string.IsNullOrEmpty(userKeyValue))
                {
                    httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await httpContext.Response.WriteAsJsonAsync(new AuthRedisNotFoundResponseDto() { ReturnLoginUrl = "http://localhost:5000" });
                    return;
                }

                RedisService redisService = serviceProvider.GetService<RedisService>()!;

                var hasAuthUser = await redisService.ReadRedis(userKeyValue);

                if (hasAuthUser == null || (hasAuthUser != null && hasAuthUser.LifeTime < DateTime.UtcNow)) //Bu kullanıcı rediste yok veya rediste var ama süresi dolmuş
                {
                    httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await httpContext.Response.WriteAsJsonAsync(new AuthRedisNotFoundResponseDto() { ReturnLoginUrl = "http://localhost:5000" });
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
