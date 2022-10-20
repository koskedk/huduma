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
    public class ChargeClientCommand : IRequest<Result>
    {
        public string Client { get; }
        public double Amount { get; }

        public ChargeClientCommand(string client, double amount)
        {
            Client = client;
            Amount = amount;
        }
    }

    public class ChargeClientCommandHandler : IRequestHandler<ChargeClientCommand, Result>
    {
        private readonly IBillRepository _billRepository;
        private readonly IBus _bus;

        public ChargeClientCommandHandler(IBillRepository billRepository, IBus bus)
        {
            _bus = bus;
            _billRepository = billRepository;
        }

        public async Task<Result> Handle(ChargeClientCommand request, CancellationToken cancellationToken)
        {
            var bill = Bill.Generate(request.Client, Money.FromKes(request.Amount));

            try
            {
                //save
                await _billRepository.Add(bill);
                await _billRepository.Commit();
                
                //broadcast
                await _bus.Publish<ClientBilled>(new
                {
                    bill.BillNo,
                    bill.Client,
                    bill.Amount
                }, cancellationToken);

                return Result.Success();
            }
            catch (Exception e)
            {
                Log.Error(e, "Billing Error");
                return Result.Failure(e.Message);
            }
        }
    }
}