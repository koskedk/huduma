using Huduma.Billing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Huduma.Billing.Infrastructure.Configurations;

public class PaymentConfigurations:IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.OwnsOne(x => x.Amount, p =>
        {
            p.Property(a => a.Currency).HasMaxLength(10);
        });
    }
}