using MassTransit;
using Huduma.Billing.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Huduma.Billing.Infrastructure
{
    public class BillStateConfiguration:SagaClassMap<BillState>
    {
        protected override void Configure(EntityTypeBuilder<BillState> entity, ModelBuilder model)
        {
            entity.Property(x => x.CurrentState).HasMaxLength(64);
            entity.Property(x => x.Client);
            entity.Property(x => x.Amount);
        }
    }
}
