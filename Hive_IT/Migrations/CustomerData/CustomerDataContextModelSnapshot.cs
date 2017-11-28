﻿// <auto-generated />
using Hive_IT.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace Hive_IT.Migrations.CustomerData
{
    [DbContext(typeof(CustomerDataContext))]
    partial class CustomerDataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Hive_IT.Data.Customer", b =>
                {
                    b.Property<long>("CustomerId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateCreated");

                    b.Property<string>("FirstName")
                        .IsRequired();

                    b.Property<string>("LastName")
                        .IsRequired();

                    b.HasKey("CustomerId");

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("Hive_IT.Data.CustomerAddress", b =>
                {
                    b.Property<long>("AddressId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("City");

                    b.Property<string>("Country");

                    b.Property<long>("CustomerId");

                    b.Property<string>("Postal")
                        .HasMaxLength(6);

                    b.Property<string>("ProvState");

                    b.Property<string>("StreetAddress");

                    b.HasKey("AddressId");

                    b.HasIndex("CustomerId");

                    b.ToTable("MailingAddresses");
                });

            modelBuilder.Entity("Hive_IT.Data.CustomerEmail", b =>
                {
                    b.Property<long>("EmailId")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("CustomerId");

                    b.Property<string>("Email");

                    b.HasKey("EmailId");

                    b.HasIndex("CustomerId");

                    b.ToTable("Emails");
                });

            modelBuilder.Entity("Hive_IT.Data.CustomerPhoneNumber", b =>
                {
                    b.Property<long>("PhoneId")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("CustomerId");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(13);

                    b.HasKey("PhoneId");

                    b.HasIndex("CustomerId");

                    b.ToTable("PhoneNumbers");
                });

            modelBuilder.Entity("Hive_IT.Data.Device", b =>
                {
                    b.Property<string>("DeviceId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BegunRepairAt");

                    b.Property<string>("CreatedAt");

                    b.Property<string>("DeclaredUnfixableAt");

                    b.Property<string>("DiagnosedAt");

                    b.Property<string>("Manufacturer")
                        .IsRequired();

                    b.Property<string>("Model")
                        .IsRequired();

                    b.Property<string>("Notes");

                    b.Property<string>("Password")
                        .IsRequired();

                    b.Property<string>("PickedUpAt");

                    b.Property<string>("Problem")
                        .IsRequired();

                    b.Property<string>("Provider")
                        .IsRequired();

                    b.Property<string>("RepairedAt");

                    b.Property<string>("Serial")
                        .IsRequired();

                    b.Property<string>("Status")
                        .IsRequired();

                    b.Property<string>("StatusLastUpdatedAt");

                    b.Property<string>("WorkOrderNumber");

                    b.HasKey("DeviceId");

                    b.HasIndex("WorkOrderNumber");

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("Hive_IT.Data.Manufacturer", b =>
                {
                    b.Property<int>("ManufacturerId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ManufacturerName");

                    b.HasKey("ManufacturerId");

                    b.ToTable("Manufacturers");
                });

            modelBuilder.Entity("Hive_IT.Data.ModelofDevice", b =>
                {
                    b.Property<int>("Identifier")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ManufacturerId");

                    b.Property<string>("Model");

                    b.HasKey("Identifier");

                    b.HasIndex("ManufacturerId");

                    b.ToTable("DeviceModels");
                });

            modelBuilder.Entity("Hive_IT.Data.WorkOrder", b =>
                {
                    b.Property<string>("WorkOrderNumber")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CompletionAt");

                    b.Property<long>("CustomerId");

                    b.Property<string>("PaidAt");

                    b.Property<string>("Status")
                        .IsRequired();

                    b.Property<string>("StatusLastUpdatedAt");

                    b.Property<string>("TimeCreated");

                    b.HasKey("WorkOrderNumber");

                    b.HasIndex("CustomerId");

                    b.ToTable("WorkOrders");
                });

            modelBuilder.Entity("Hive_IT.Data.CustomerAddress", b =>
                {
                    b.HasOne("Hive_IT.Data.Customer", "Customer")
                        .WithMany("CustomerAddress")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Hive_IT.Data.CustomerEmail", b =>
                {
                    b.HasOne("Hive_IT.Data.Customer", "Customer")
                        .WithMany("CustomerEmail")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Hive_IT.Data.CustomerPhoneNumber", b =>
                {
                    b.HasOne("Hive_IT.Data.Customer", "Customer")
                        .WithMany("CustomerPhoneNumber")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Hive_IT.Data.Device", b =>
                {
                    b.HasOne("Hive_IT.Data.WorkOrder", "WorkOrder")
                        .WithMany("Device")
                        .HasForeignKey("WorkOrderNumber");
                });

            modelBuilder.Entity("Hive_IT.Data.ModelofDevice", b =>
                {
                    b.HasOne("Hive_IT.Data.Manufacturer", "Manufacturer")
                        .WithMany("Models")
                        .HasForeignKey("ManufacturerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Hive_IT.Data.WorkOrder", b =>
                {
                    b.HasOne("Hive_IT.Data.Customer", "Customer")
                        .WithMany("WorkOrder")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
