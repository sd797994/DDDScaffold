using Infrastructure.DataBase.PO;
using InfrastructureBase;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Infrastructure.DataBase
{
    public class MySqlEfContext : DbContext
    {
        public MySqlEfContext(DbContextOptions<MySqlEfContext> options) : base(options)
        {

        }
        public MySqlEfContext()
        {

        }
        //dbset put here
        public DbSet<User> User { get; set; }
        public DbSet<Permission> Permission { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<UserRole> UserRole { get; set; }
        public DbSet<RolePermission> RolePermission { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("Server=localhost;Database=douyin;User=root;Password=3wdianFybcn#rds", new MySqlServerVersion(new Version(5, 7, 0)));
            }
        }
        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbUpdateException dbEx)
            {
                HandleMaxLengthError(dbEx);
                throw; 
            }
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException dbEx)
            {
                HandleMaxLengthError(dbEx);
                throw;
            }
        }

        private void HandleMaxLengthError(DbUpdateException dbEx)
        {
            if (dbEx.InnerException is SqlException sqlEx)
            {
                var errorMessage = sqlEx.Message;
                if (errorMessage.Contains("string or binary data would be truncated"))
                {
                    throw new InfrastructureException("一个或多个字段的值超过了数据库列的最大长度，请检查输入的数据长度。");
                }
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>().HasIndex(e => e.UserId);
            modelBuilder.Entity<UserRole>().HasIndex(e => e.RoleId); 
            modelBuilder.Entity<RolePermission>().HasIndex(e => e.RoleId);
            modelBuilder.Entity<RolePermission>().HasIndex(e => e.PermissionId);

        }
    }
}
