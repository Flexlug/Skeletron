using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;

using WAV_Bot_DSharp.Services.Structures;

namespace WAV_Bot_DSharp.Services.Entities
{
    public class TrackedUserContext : DbContext
    {
        public DbSet<TrackedUser> Users { get; set; }

        public TrackedUserContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=trackedUsers.db")
                      .EnableDetailedErrors();
    }
}
