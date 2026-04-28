using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using YarpExample.Gateway;
using YarpExample.Gateway.Database;
using YarpExample.Gateway.RequestTransforms;
using YarpExample.Gateway.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHangfire(config => config.UseMemoryStorage());
builder.Services.AddHangfireServer();

builder.Services.AddScoped<GatewayService>();
builder.Services.AddStackExchangeRedisCache(action =>
{
    action.Configuration = "127.0.0.1:6379";
});
builder.Services.AddHybridCache();
builder.Services.AddDbContext<GatewayDbContext>(y => y.UseNpgsql("Host=localhost;Port=5432;Username=olgunbey;Password=sahinbey;Database=GatewayDbContext"));
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(context =>
    {
        var scope = context.Services.CreateScope();
        var hybridCache = scope.ServiceProvider.GetRequiredService<HybridCache>();
        var gatewayDbContext = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();
        var gatewayService = scope.ServiceProvider.GetRequiredService<GatewayService>();
        var httpClient = scope.ServiceProvider.GetService<HttpClient>();
        httpClient.BaseAddress = new Uri("http://localhost:5127");
        context.RequestTransforms.Add(new AuthorizationRequestTransform(hybridCache, gatewayDbContext, gatewayService, httpClient));
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseForwardedHeaders();
app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard();
app.MapReverseProxy();
app.MapControllers();
app.Run();
