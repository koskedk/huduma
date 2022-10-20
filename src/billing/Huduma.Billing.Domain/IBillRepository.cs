using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Huduma.Billing.Domain
{
    public interface IBillRepository : IDisposable
    {
        Task<List<Bill>> Get();
        Task<Bill> Get(Guid id);
        Task Add(Bill bill);
        Task Update(Bill bill);
        Task Delete(Guid id);
        Task Commit();
    }
}