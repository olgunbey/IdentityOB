using Microsoft.EntityFrameworkCore;
using YarpExample.Gateway.Entity;

namespace YarpExample.Gateway.Database
{
    public class GatewayDbContext(DbContextOptions<GatewayDbContext> dbContextOptions) : DbContext(dbContextOptions)
    {
        public DbSet<Services> Services { get; set; }
        public DbSet<ServicesPermissions> ServicesPermissions { get; set; }
        public DbSet<Permissions> Permissions { get; set; }

        public DbSet<Outbox> Outbox { get; set; }
        public DbSet<Inbox> Inbox { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permissions>().Property(y => y.PermissionId).IsRequired(false).HasDefaultValue(0);

            modelBuilder.Entity<ServicesPermissions>()
                .HasOne(sp => sp.Service)
                .WithMany(s => s.ServicesPermissions)
                .HasForeignKey(sp => sp.ServiceId);

            modelBuilder.Entity<ServicesPermissions>()
                .HasOne(sp => sp.Permission)
                .WithMany(p => p.ServicesPermissions)
                .HasForeignKey(sp => sp.PermissionId);
            base.OnModelCreating(modelBuilder);
        }
    }
}
