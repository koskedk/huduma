using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Huduma.Contracts;
using MassTransit;
using MediatR;
using Serilog;

namespace Huduma.Billing.Application.Queries
{
    public class GetBillQuery:IRequest<Result<BillStatus>>
    {
        public Guid BillNo { get; }

        public GetBillQuery(Guid billNo)
        {
            BillNo = billNo;
        }
    }

    public class GetBillQueryHandler : IRequestHandler<GetBillQuery,Result<BillStatus>>
    {
        private readonly IRequestClient<CheckBill> _client;

        public GetBillQueryHandler(IRequestClient<CheckBill> client)
        {
            _client = client;
        }

        public async Task<Result<BillStatus>> Handle(GetBillQuery request, CancellationToken cancellationToken)
        {

            try
            {
                var (status,notFound) = await _client.GetResponse<BillStatus,BillNotFound>(new { BillNo = request.BillNo });
                if (status.IsCompletedSuccessfully)
                {
                    var respons = await status;
                    return Result.Success(respons.Message);
                }
                else
                {
                    var respons = await notFound;
                    throw new Exception($"Not Found {respons.Message.BillNo}");
                }
                
            }
            catch (Exception e)
            {
                Log.Error(e,"Bill Query Error");
                return Result.Failure<BillStatus>(e.Message);
            }
        }
    }
}