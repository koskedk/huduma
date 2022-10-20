using Huduma.Billing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Huduma.Billing.Infrastructure.Configurations;

public class BillConfigurations:IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> builder)
    {
        builder.Property(x => x.Client);
        builder.OwnsOne(x => x.Charge, p =>
        {
            p.Property(a => a.Currency).HasMaxLength(10);
        });
        builder.HasIndex(x => x.Client);
    }
}