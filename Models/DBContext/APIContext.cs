using Microsoft.EntityFrameworkCore;
using THMY_API.Models;
namespace THMY_API.Models.DBContext;

public class APIContext : DbContext
{
    public APIContext() { }
    public APIContext(DbContextOptions<APIContext> options) : base(options) { }
    public DbSet<APIStorage> APIStroageKey { get; set; }
    public DbSet<Role> Role { get; set; }
    public DbSet<EmpRole> EmpRole { get; set; }
    public DbSet<RolePermission> RolePermission { get; set; }
    public DbSet<Permission> Permission { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure composite primary keys for associative entities
        modelBuilder.Entity<EmpRole>()
            .HasKey(e => new { e.empId, e.roleId, e.systemId });

        modelBuilder.Entity<RolePermission>()
            .HasKey(rp => new { rp.roleId, rp.permissionId, rp.systemId });

        // Configure single primary keys for main entities
        modelBuilder.Entity<Role>()
            .HasKey(r => r.roleId);

        modelBuilder.Entity<Permission>()
            .HasKey(p => p.permissionId);
    }
}
