using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Huduma.Billing.Domain;
using MediatR;
using Serilog;

namespace Huduma.Billing.Application.Queries
{
    public class LoadBillsQuery:IRequest<Result<List<Bill>>>
    {
    }

    public class LoadBillsQueryHandler : IRequestHandler<LoadBillsQuery,Result<List<Bill>>>
    {
        private readonly IBillRepository _repository;

        public LoadBillsQueryHandler(IBillRepository repository)
        {
            _repository = repository;
        }


        public async Task<Result<List<Bill>>> Handle(LoadBillsQuery request, CancellationToken cancellationToken)
        {

            try
            {
                var bills = await _repository.Get();
                return Result.Success(bills);
            }
            catch (Exception e)
            {
                Log.Error(e,"Bill Load Error");
                return Result.Failure<List<Bill>>(e.Message);
            }
        }
    }
}