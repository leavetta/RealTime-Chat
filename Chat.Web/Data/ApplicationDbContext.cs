using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Chat.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Chat.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
             //Database.EnsureDeleted();
             Database.EnsureCreated();
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ApplicationUser> AppUsers { get; set; }
        public DbSet<ChatMember> ChatMembers { get; set; }

        
        public Task<List<ApplicationUser>> GetAppUsers()
        {
            return AppUsers.ToListAsync();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
           /* builder.Entity<ApplicationUser>()
        .ToTable("AspNetUsers", t => t.ExcludeFromMigrations());
            builder.Entity<IdentityRole>()
        .ToTable("AspNetRoles", t => t.ExcludeFromMigrations());*/
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
