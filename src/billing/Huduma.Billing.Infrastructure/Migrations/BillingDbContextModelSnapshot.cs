﻿// <auto-generated />
using System;
using Huduma.Billing.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Huduma.Billing.Infrastructure.Migrations
{
    [DbContext(typeof(BillingDbContext))]
    partial class BillingDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Huduma.Billing.Domain.Bill", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Client")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("Client");

                    b.ToTable("Bills");
                });

            modelBuilder.Entity("Huduma.Billing.Domain.Payment", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<Guid>("BillId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("BillId");

                    b.ToTable("Payments");
                });

            modelBuilder.Entity("Huduma.Billing.Domain.Bill", b =>
                {
                    b.OwnsOne("Huduma.Billing.Domain.Money", "Charge", b1 =>
                        {
                            b1.Property<Guid>("BillId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<string>("Currency")
                                .HasMaxLength(10)
                                .HasColumnType("nvarchar(10)");

                            b1.Property<double>("Value")
                                .HasColumnType("float");

                            b1.HasKey("BillId");

                            b1.ToTable("Bills");

                            b1.WithOwner()
                                .HasForeignKey("BillId");
                        });

                    b.Navigation("Charge");
                });

            modelBuilder.Entity("Huduma.Billing.Domain.Payment", b =>
                {
                    b.HasOne("Huduma.Billing.Domain.Bill", null)
                        .WithMany("Payments")
                        .HasForeignKey("BillId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("Huduma.Billing.Domain.Money", "Amount", b1 =>
                        {
                            b1.Property<string>("PaymentId")
                                .HasColumnType("nvarchar(50)");

                            b1.Property<string>("Currency")
                                .HasMaxLength(10)
                                .HasColumnType("nvarchar(10)");

                            b1.Property<double>("Value")
                                .HasColumnType("float");

                            b1.HasKey("PaymentId");

                            b1.ToTable("Payments");

                            b1.WithOwner()
                                .HasForeignKey("PaymentId");
                        });

                    b.Navigation("Amount");
                });

            modelBuilder.Entity("Huduma.Billing.Domain.Bill", b =>
                {
                    b.Navigation("Payments");
                });
#pragma warning restore 612, 618
        }
    }
}
