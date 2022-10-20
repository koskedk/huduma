using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

namespace Huduma.Billing.Domain
{
    public class Bill : Entity<Guid>
    {
        private readonly List<Payment> _payments = new();
        
        public string Client { get; private set; }
        public Money Charge { get; private set; }
        public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();
        public double TotalPaid => Payments.Sum(x => x.Amount.Value);
        public bool IsOpen => TotalPaid < Charge.Value;
        public bool IsClosed => !IsOpen;
        public Guid BillNo => Id;
        public double Amount => Charge.Value;

        private Bill()
        {
        }
        
        private Bill(string client, Money charge)
            : base(Guid.NewGuid())
        {
            Client = client;
            Charge = charge;
        }

        public static Bill Generate(string client, Money amount)
        {
            if (string.IsNullOrWhiteSpace(client))
                throw new ArgumentException("Missing Client");
            
            return new Bill(client, amount);
        }

        public void ReceivePayment(Money amountReceived)
        {
            if (AmountIsInExcess(amountReceived))
                throw new ArgumentException($"Excess payment of {ExcessAmount(amountReceived):C} of the Amount {Charge} Not Allowed !");
            
            _payments.Add(Payment.Generate(Id, amountReceived));
        }

        private bool AmountIsInExcess(Money amountReceived)
        {
            return amountReceived.Value > Charge.Value ||
                   amountReceived.Value > TotalPaid;
        }
        
        private double ExcessAmount(Money amountDue)
        {
            return amountDue.Value - Charge.Value;
        }
    }
}