using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Huduma.Billing.Domain;
using Huduma.Contracts;
using MassTransit;
using MediatR;
using Serilog;

namespace Huduma.Billing.Application.Queries
{
    public class GetBillQuery:IRequest<Result<BillState>>
    {
        public Guid BillNo { get; }

        public GetBillQuery(Guid billNo)
        {
            BillNo = billNo;
        }
    }

    public class GetBillQueryHandler : IRequestHandler<GetBillQuery,Result<BillState>>
    {
        private readonly IBillRepository _repository;
        private readonly IBus _bus;

        public GetBillQueryHandler(IBillRepository repository, IBus bus)
        {
            _repository = repository;
            _bus = bus;
        }

        public async Task<Result<BillState>> Handle(GetBillQuery request, CancellationToken cancellationToken)
        {

            try
            {
                var client = _bus.CreateRequestClient<CheckBillRequested>();
                var response = await client.GetResponse<BillState>(new { BillNo = request.BillNo });
                return Result.Success(response.Message);
            }
            catch (Exception e)
            {
                Log.Error(e,"Bill Query Error");
                return Result.Failure<BillState>(e.Message);
            }
        }
    }
}