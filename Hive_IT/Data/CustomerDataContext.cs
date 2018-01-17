using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Data
{
    public class CustomerDataContext: DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerPhoneNumber> PhoneNumbers { get; set; }
        public DbSet<CustomerEmail> Emails { get; set; }
        public DbSet<CustomerAddress> MailingAddresses { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<ModelofDevice> DeviceModels { get; set; }
        public DbSet<WorkOrderHistory> Histories { get; set; }
        public DbSet<Service> Services { get; set; }

        
        public CustomerDataContext(DbContextOptions<CustomerDataContext> options): base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WorkOrderService>()
                .HasKey(x => new { x.WorkOrderNumber, x.ServiceId });
                       
        }
    }
}
