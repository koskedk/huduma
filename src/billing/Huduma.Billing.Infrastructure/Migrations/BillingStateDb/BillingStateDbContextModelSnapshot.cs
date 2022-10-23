﻿// <auto-generated />
using System;
using Huduma.Billing.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Huduma.Billing.Infrastructure.Migrations.BillingStateDb
{
    [DbContext(typeof(BillingStateDbContext))]
    partial class BillingStateDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Huduma.Billing.Application.BillState", b =>
                {
                    b.Property<Guid>("CorrelationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("Amount")
                        .HasColumnType("float");

                    b.Property<string>("Client")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CurrentState")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("CorrelationId");

                    b.ToTable("BillState");
                });
#pragma warning restore 612, 618
        }
    }
}
