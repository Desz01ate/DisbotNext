using DisbotNext.Infrastructures.Common.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using DisbotNext.Infrastructure.Common;
using DisbotNext.Infrastructure.Common.Models;

namespace DisbotNext.Infrastructures.Sqlite
{
    public class SqliteDbContext : DisbotDbContext
    {
        public override DbSet<Member> Members { get; set; }

        public override DbSet<ChatLog> ChatLogs { get; set; }

        public override DbSet<TempChannel> TempChannels { get; set; }

        public override DbSet<ErrorLog> ErrorLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies().UseSqlite($@"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Local.db")}");
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Member>(builder =>
            {
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).ValueGeneratedOnAdd();
                builder.HasMany(x => x.ChatLogs).WithOne();
            });

            modelBuilder.Entity<ChatLog>(builder =>
            {
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).ValueGeneratedOnAdd();
                builder.Property(x => x.Content);
                builder.Property(x => x.CreateAt);
                builder.HasOne(x => x.Author).WithMany(x => x.ChatLogs);
            });

            modelBuilder.Entity<TempChannel>(builder =>
            {
                builder.HasKey(x => x.Id);
                builder.Property(x => x.ChannelName);
                builder.Property(x => x.CreatedAt);
                builder.Property(x => x.ExpiredAt);
            });

            modelBuilder.Entity<ErrorLog>(builder =>
            {
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).ValueGeneratedOnAdd();
                builder.HasOne(x => x.TriggeredBy);
            });
        }
    }
}
