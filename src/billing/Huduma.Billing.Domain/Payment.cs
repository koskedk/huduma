using System;
using CSharpFunctionalExtensions;

namespace Huduma.Billing.Domain
{
    public class Payment:Entity<string>
    {
        public Guid BillId { get;private set; }
        public Money Amount { get;private set; }
        
        private Payment(){}
        private Payment(string id, Guid billId,Money amount)
            : base(id)
        {
            BillId = billId;
            Amount = amount;
        }

        public static Payment Generate(Guid billNo, Money amount)
        {
            var receipt = $"RT-MPESA-{DateTime.UtcNow.Ticks}";
            return new Payment(receipt, billNo, amount);
        }
    }
}