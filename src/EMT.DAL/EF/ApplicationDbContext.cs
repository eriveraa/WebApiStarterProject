using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Query;
using System.Reflection;
using EMT.Common.Entities;

namespace EMT.DAL.EF
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            //Debug.WriteLine($"* Constructor of {this.GetType()} at {DateTime.Now}");
        }

        // Dbsets correspondiente a cada tabla física / entidad
        public DbSet<MyNote> MyNote { get; set; }
        public DbSet<MyTask> MyTask { get; set; }
        public DbSet<AppUser> AppUser { get; set; }
        public DbSet<AppUserRole> AppUserRole { get; set; }
        public DbSet<AppParameter> AppParameter { get; set; }
    }
}
