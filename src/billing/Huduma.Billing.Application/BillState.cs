using System;
using MassTransit;

namespace Huduma.Billing.Application
{
    public class BillState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public string Client { get;  set; }
        public double Amount { get; set; }

        public override string ToString()
        {
            return $"{Client}, Your Bill:{Amount:C} [{CurrentState}]";
        }
    }
}