using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Huduma.Billing.Domain;
using Huduma.Contracts;
using MassTransit;
using MediatR;
using Serilog;

namespace Huduma.Billing.Application.Commands
{
    public class AcceptPaymentCommand : IRequest<Result>
    {
        public Guid BillNo { get; }
        public double Amount { get; }

        public AcceptPaymentCommand(Guid billNo, double amount)
        {
            BillNo = billNo;
            Amount = amount;
        }
    }

    public class AcceptPaymentCommandHandler:IRequestHandler<AcceptPaymentCommand,Result>
    {
        private readonly IBillRepository _repository;
        private readonly IBus _bus;

        public AcceptPaymentCommandHandler(IBillRepository repository, IBus bus)
        {
            _repository = repository;
            _bus = bus;
        }

        public async Task<Result> Handle(AcceptPaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                //read
                var bill = await _repository.Get(request.BillNo);
                if (null == bill)
                    throw new NullReferenceException("Bill not Found!");
            
                if (bill.IsClosed)
                    throw new ArgumentException("Bill Already Paid!");
            
                //add Payment
            
                bill.ReceivePayment(Money.FromKes(request.Amount));
            
                await _repository.Update(bill);
                await _repository.Commit();
                
                //broadcast
                await _bus.Publish<PaymentReceived>(new
                {
                    request.BillNo,
                    request.Amount
                }, cancellationToken);

            
                //broadcast
                if (bill.IsClosed)
                    await _bus.Publish<BillCleared>(new
                    {
                        request.BillNo
                    }, cancellationToken);
           

                return Result.Success();
            }
            catch (Exception e)
            {
                Log.Error(e, "Billing Payment Error");
                return Result.Failure(e.Message);
            }
        }
    }
}