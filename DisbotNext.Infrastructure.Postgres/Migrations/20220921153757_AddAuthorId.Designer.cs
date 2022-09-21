﻿// <auto-generated />
using System;
using DisbotNext.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DisbotNext.Infrastructure.Postgres.Migrations
{
    [DbContext(typeof(NpgsqlDbContext))]
    [Migration("20220921153757_AddAuthorId")]
    partial class AddAuthorId
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.10")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("DisbotNext.Infrastructures.Common.Models.ChatLog", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreateAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("MemberId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("MemberId");

                    b.ToTable("ChatLogs");
                });

            modelBuilder.Entity("DisbotNext.Infrastructures.Common.Models.ErrorLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Log")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Method")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal?>("TriggeredById")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("TriggeredById");

                    b.ToTable("ErrorLogs");
                });

            modelBuilder.Entity("DisbotNext.Infrastructures.Common.Models.Member", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("AutoMoveToChannel")
                        .HasColumnType("boolean");

                    b.Property<double>("Exp")
                        .HasColumnType("double precision");

                    b.Property<int>("Level")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Members");
                });

            modelBuilder.Entity("DisbotNext.Infrastructures.Common.Models.StockSubscription", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("DiscordMemberId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("StockSubscriptions");
                });

            modelBuilder.Entity("DisbotNext.Infrastructures.Common.Models.TempChannel", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("ChannelName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ChannelType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("ExpiredAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid>("GroupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.ToTable("TempChannels");
                });

            modelBuilder.Entity("DisbotNext.Infrastructures.Common.Models.ChatLog", b =>
                {
                    b.HasOne("DisbotNext.Infrastructures.Common.Models.Member", "Member")
                        .WithMany("ChatLogs")
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Member");
                });

            modelBuilder.Entity("DisbotNext.Infrastructures.Common.Models.ErrorLog", b =>
                {
                    b.HasOne("DisbotNext.Infrastructures.Common.Models.Member", "TriggeredBy")
                        .WithMany()
                        .HasForeignKey("TriggeredById");

                    b.Navigation("TriggeredBy");
                });

            modelBuilder.Entity("DisbotNext.Infrastructures.Common.Models.Member", b =>
                {
                    b.Navigation("ChatLogs");
                });
#pragma warning restore 612, 618
        }
    }
}
