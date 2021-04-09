using DisbotNext.Infrastructures.Sqlite.CustomDbSets;
using DisbotNext.Infrastructures.Common.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using Microsoft.EntityFrameworkCore.Proxies;
using DisbotNext.Infrastructure.Common;

namespace DisbotNext.Infrastructures.Sqlite
{
    public class SqliteDbContext : DisbotDbContext
    {
        private MemberDbSet _members;

        public override DbSet<Member> Members
        {
            get => _members;
            set
            {
                _members = new MemberDbSet(value);
            }
        }

        public override DbSet<ChatLog> ChatLogs { get; set; }

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
        }
    }
}
