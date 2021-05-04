using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Query;
using System.Reflection;
using EMT.Common.Entities;
using System.Threading.Tasks;
using System.Threading;

namespace EMT.DAL.EF
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            //Debug.WriteLine($"* Constructor of {this.GetType()} at {DateTime.Now}");
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // This will automatically put the timestamps in any Entity derived from AuditableEntityBase
            var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var entries = ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
            //.Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added  || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                if (entityEntry.Entity is AuditableEntityBase auditableEntity)
                {
                    // Implementation may change based on the useage scenario, this
                    // sample is for forma authentication.
                    //string currentUser = HttpContext.Current.User.Identity.Name;
                    //var currentUsername = !string.IsNullOrEmpty(System.Web.HttpContext.Current?.User?.Identity?.Name) ? HttpContext.Current.User.Identity.Name : "Anonymous";

                    auditableEntity.UpdatedAt = timeStamp;
                    if (entityEntry.State == EntityState.Modified)
                    {
                        // mark property as "don't touch" as we don't want to update on a Modify operation
                        entityEntry.Property("CreatedAt").IsModified = false;
                    }
                    else if (entityEntry.State == EntityState.Added)
                    {
                        auditableEntity.IsDeleted = false;
                        auditableEntity.CreatedAt = timeStamp;
                    }
                }
                //if (entityEntry.State == EntityState.Modified)
                //{
                //    entityEntry.Property("UpdatedDate").CurrentValue = DateTime.Now;
                //}
                //else if (entityEntry.State == EntityState.Added)
                //{
                //    entityEntry.Property("CreatedDate").CurrentValue = DateTime.Now;
                //    entityEntry.Property("UpdatedDate").CurrentValue = DateTime.Now;
                //}
            }

            // For other entities (not derived from AuditableEntityBase, it will work as always/default)
            return await base.SaveChangesAsync(cancellationToken);
        }

        // Dbsets correspondiente a cada tabla física / entidad
        public DbSet<MyNote> MyNote { get; set; }
        public DbSet<MyTask> MyTask { get; set; }
        public DbSet<AppUser> AppUser { get; set; }
        public DbSet<AppUserRole> AppUserRole { get; set; }
        public DbSet<AppParameter> AppParameter { get; set; }


    }
}
