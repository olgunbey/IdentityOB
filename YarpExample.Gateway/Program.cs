using Microsoft.EntityFrameworkCore;
using YarpExample.Gateway.Database;
using YarpExample.Gateway.RedisService;
using YarpExample.Gateway.RequestTransforms;
using YarpExample.Gateway.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<RedisService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddDbContext<GatewayDbContext>(y => y.UseNpgsql("Host=localhost;Port=5432;Username=olgunbey;Password=sahinbey;Database=GatewayDbContext"));
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(context =>
    {
        var scope = context.Services.CreateScope();
        var redisService = scope.ServiceProvider.GetRequiredService<RedisService>();
        var gatewayDbContext = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();
        var permissionService = scope.ServiceProvider.GetRequiredService<PermissionService>();
        context.RequestTransforms.Add(new AuthorizationRequestTransform(redisService, gatewayDbContext, permissionService));
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
