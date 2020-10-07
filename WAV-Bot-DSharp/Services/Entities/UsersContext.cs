using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;

namespace WAV_Bot_DSharp.Services.Entities
{
    public class UsersContext : DbContext
    {
        public DbSet<UserInfo> Users { get; set; }

        public UsersContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=users.db")
                      .EnableDetailedErrors();

    }
}
