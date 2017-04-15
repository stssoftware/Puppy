﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using TopCore.WebAPI.Data.EF;

namespace TopCore.WebAPI.Data.EF.Migrations
{
    [DbContext(typeof(DbContext))]
    [Migration("20170415022105_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("TopCore.WebAPI.Data.Entity.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("CreatedBy");

                    b.Property<DateTime>("CreatedOnUtc");

                    b.Property<int?>("DeletedBy");

                    b.Property<DateTime?>("DeletedOnUtc");

                    b.Property<bool>("IsDeleted");

                    b.Property<Guid>("Key");

                    b.Property<DateTime?>("LastUpdatedOnUtc");

                    b.Property<int?>("UpdatedBy");

                    b.Property<string>("UserName");

                    b.Property<byte[]>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("Id");

                    b.ToTable("User");
                });
        }
    }
}
