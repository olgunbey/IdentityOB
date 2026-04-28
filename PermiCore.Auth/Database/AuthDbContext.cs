using Microsoft.EntityFrameworkCore;
using PermiCore.Auth.Entity;

namespace PermiCore.Auth.Database
{
    public class AuthDbContext(DbContextOptions<AuthDbContext> contextOptions) : DbContext(contextOptions)
    {
        public DbSet<Permissions> Permissions { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<UserPermissions> UserPermissions { get; set; }
        public DbSet<LoginUser> LoginUser { get; set; }
        public DbSet<Outbox> Outbox { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permissions>().Property(y => y.PermissionId).IsRequired(false);

            modelBuilder.Entity<UserPermissions>().
                HasOne(y => y.Permission).
                WithMany(x => x.UserPermissions).HasForeignKey(y => y.PermissionId);

            modelBuilder.Entity<UserPermissions>().
                HasOne(y => y.User).
                WithMany(x => x.UserPermissions).HasForeignKey(y => y.UserId);
        }
    }
}
