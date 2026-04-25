
using Microsoft.EntityFrameworkCore;
using PermiCore.Auth.Database;
using PermiCore.Auth.Service;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddDbContext<AuthDbContext>(y => y.UseNpgsql("Host=localhost;Port=5432;Username=olgunbey;Password=sahinbey;Database=AuthDbContext"));
builder.Services.AddScoped<AuthService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "127.0.0.1:6379";
});
builder.Services.AddHybridCache();
builder.Services.AddSwaggerGen();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
