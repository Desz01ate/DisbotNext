﻿using DisbotNext.Infrastructures.Common.Models;
using Microsoft.EntityFrameworkCore;
using DisbotNext.Infrastructures.Common;

namespace DisbotNext.Infrastructures.Sqlite
{
    public class SqliteDbContext : DisbotDbContext
    {
        public override DbSet<Member> Members { get; set; }

        public override DbSet<ChatLog> ChatLogs { get; set; }

        public override DbSet<TempChannel> TempChannels { get; set; }

        public override DbSet<ErrorLog> ErrorLogs { get; set; }

        public override DbSet<StockSubscription> StockSubscriptions { get; set; }

        public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Member>(builder =>
            {
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id);
                builder.HasMany(x => x.ChatLogs).WithOne();
            });

            modelBuilder.Entity<ChatLog>(builder =>
            {
                builder.HasKey(x => x.Id);
                builder.Property(x => x.MemberId)
                       .IsRequired();
                builder.Property(x => x.Content)
                       .IsRequired();
                builder.Property(x => x.CreateAt)
                       .IsRequired();

                builder.HasOne(x => x.Member)
                       .WithMany(x => x.ChatLogs)
                       .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TempChannel>(builder =>
            {
                builder.HasKey(x => x.Id);
                builder.Property(x => x.ChannelName);
                builder.Property(x => x.CreatedAt);
                builder.Property(x => x.ExpiredAt);
                builder.Property(x => x.ChannelType).HasConversion<string>().IsRequired();
                builder.Property(x => x.GroupId).IsRequired().ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<ErrorLog>(builder =>
            {
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).ValueGeneratedOnAdd();
                builder.HasOne(x => x.TriggeredBy);
            });

            modelBuilder.Entity<StockSubscription>(builder =>
            {
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).ValueGeneratedOnAdd();
                builder.Property(x => x.Symbol).IsRequired();
            });
        }
    }
}
