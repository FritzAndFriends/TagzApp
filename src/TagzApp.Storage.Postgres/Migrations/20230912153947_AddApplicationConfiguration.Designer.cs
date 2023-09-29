﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TagzApp.Storage.Postgres;

#nullable disable

namespace TagzApp.Storage.Postgres.Migrations
{
    [DbContext(typeof(TagzAppContext))]
    [Migration("20230912153947_AddApplicationConfiguration")]
    partial class AddApplicationConfiguration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("TagzApp.Common.Models.Settings", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("TagzApp.Storage.Postgres.PgContent", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Author")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Emotes")
                        .HasColumnType("text");

                    b.Property<string>("HashtagSought")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("PreviewCard")
                        .HasColumnType("text");

                    b.Property<string>("Provider")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<string>("ProviderId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("SourceUri")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasAlternateKey("Provider", "ProviderId");

                    b.ToTable("Content");
                });

            modelBuilder.Entity("TagzApp.Storage.Postgres.PgModerationAction", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("ContentId")
                        .HasColumnType("bigint");

                    b.Property<string>("Moderator")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Provider")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<string>("ProviderId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Reason")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<int>("State")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasAlternateKey("Provider", "ProviderId");

                    b.HasIndex("ContentId")
                        .IsUnique();

                    b.ToTable("ModerationActions");
                });

            modelBuilder.Entity("TagzApp.Storage.Postgres.Tag", b =>
                {
                    b.Property<string>("Text")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Text");

                    b.ToTable("TagsWatched");
                });

            modelBuilder.Entity("TagzApp.Storage.Postgres.PgModerationAction", b =>
                {
                    b.HasOne("TagzApp.Storage.Postgres.PgContent", "Content")
                        .WithOne("ModerationAction")
                        .HasForeignKey("TagzApp.Storage.Postgres.PgModerationAction", "ContentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Content");
                });

            modelBuilder.Entity("TagzApp.Storage.Postgres.PgContent", b =>
                {
                    b.Navigation("ModerationAction");
                });
#pragma warning restore 612, 618
        }
    }
}
