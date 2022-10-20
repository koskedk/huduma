using Huduma.Billing.Domain;
using Microsoft.EntityFrameworkCore;

namespace Huduma.Billing.Infrastructure
{
    public class BillingDbContext:DbContext
    {
        public DbSet<Bill> Bills { get; set; }
        public DbSet<Payment> Payments { get; set; }
    
        public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bill>().OwnsOne(x => x.Charge);
            modelBuilder.Entity<Payment>().OwnsOne(x => x.Amount);
            base.OnModelCreating(modelBuilder);
        }
    }
}