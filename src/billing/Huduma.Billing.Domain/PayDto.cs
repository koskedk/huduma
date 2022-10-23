using System;

namespace Huduma.Billing.Domain
{
    public class PayDto
    {
        public Guid BillNo { get; set; }
        public double Amount { get; set; }
    }
}