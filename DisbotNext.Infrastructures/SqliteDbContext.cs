using DisbotNext.Infrastructures.Sqlite.CustomDbSets;
using DisbotNext.Infrastructures.Sqlite.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace DisbotNext.Infrastructures.Sqlite
{
    public class SqliteDbContext : DbContext
    {
        private MemberDbSet _members;
        public DbSet<Member> Members
        {
            get => _members;
            set
            {
                _members = new MemberDbSet(value);
            }
        }

        public DbSet<ChatLog> ChatLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($@"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Local.db")}");
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Member>(builder =>
            {
                builder.HasKey(x => x.Id);
                builder.HasMany(x => x.ChatLogs);
            });

            modelBuilder.Entity<ChatLog>(builder =>
            {
                builder.HasKey(x => x.Id);
                builder.HasOne(x => x.Author);
            });
        }
    }
}
