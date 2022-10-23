using Huduma.Contracts;
using MassTransit;

namespace Huduma.Billing.Application
{
    public class BillStateMachine : MassTransitStateMachine<BillState>
    {
        public BillStateMachine()
        {
            Event(() =>
                    BillClient, x => x.CorrelateById(context => context.Message.BillNo)
            );
            Event(() =>
                    ReceivePayment, x => x.CorrelateById(context => context.Message.BillNo)
            );
            Event(() =>
                    ClearBill, x => x.CorrelateById(context => context.Message.BillNo)
            );

            Event(() =>
                BillCheck, x =>
            {
                x.CorrelateById(context => context.Message.BillNo);
                x.OnMissingInstance(m => m.ExecuteAsync(async co =>
                {
                    if (co.RequestId.HasValue)
                    {
                        await co.RespondAsync<BillNotFound>(new { co.Message.BillNo });
                    }
                }));

            });


            InstanceState(x => x.CurrentState);

            Initially(
                When(BillClient)
                    .Then(x =>
                    {
                        x.Saga.CorrelationId = x.Message.BillNo;
                        x.Saga.Client = x.Message.Client;
                        x.Saga.Amount = x.Message.Amount;

                    })
                    .TransitionTo(Open)
            );

            During(Open,
                When(ReceivePayment)
                    .Then(x =>
                    {
                        x.Saga.CorrelationId = x.Message.BillNo;
                        x.Saga.Amount = x.Message.Amount;
                    })
                    .PublishAsync(context => context.Init<PaymentReceived>(new
                        { BillNo = context.Saga.CorrelationId, Amount = context.Saga.Amount }))
                    .TransitionTo(Paid)
            );

            During(Paid,
                When(ReceivePayment)
                    .Then(x =>
                    {
                        x.Saga.CorrelationId = x.Message.BillNo;
                        x.Saga.Amount = x.Message.Amount;
                    })
                    .PublishAsync(context => context.Init<PaymentReceived>(new
                        { BillNo = context.Saga.CorrelationId, Amount = context.Saga.Amount }))
                    .TransitionTo(Paid)
            );


            During(Paid,
                When(ClearBill)
                    .Then(x => { x.Saga.CorrelationId = x.Message.BillNo; })
                    .TransitionTo(Closed)
            );

            DuringAny(
                When(BillCheck)
                    .RespondAsync(context => context.Init<BillStatus>(new
                    {
                        BillNo = context.CorrelationId,
                        Client = context.Saga.Client,
                        Amount = context.Saga.Amount,
                        Status = context.Saga.CurrentState
                    }))
            );
        }

        public Event<ClientBilled> BillClient { get; private set; }
        public Event<PaymentReceived> ReceivePayment { get; private set; }
        public Event<BillCleared> ClearBill { get; private set; }
        public Event<CheckBill> BillCheck { get; private set; }

        public State Open { get; private set; }
        public State Paid { get; private set; }
        public State Closed { get; private set; }
    }
}
