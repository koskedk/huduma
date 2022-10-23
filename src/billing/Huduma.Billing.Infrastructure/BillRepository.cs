using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Huduma.Billing.Domain;
using Microsoft.EntityFrameworkCore;

namespace Huduma.Billing.Infrastructure
{
    public class BillRepository:IBillRepository
    {
        private readonly BillingDbContext _context;

        public BillRepository(BillingDbContext context)
        {
            _context = context;
        }

        public Task<List<Bill>> Get()
        {
            return _context.Bills
                .Include(x => x.Payments)
                .ToListAsync();
        }

        public Task<Bill> Get(Guid id)
        {
            return _context.Bills
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task Add(Bill bill)
        {
            await _context.AddAsync(bill);
        }

        public Task Update(Bill bill)
        {
            _context.Update(bill);
            return Task.CompletedTask;
        }

        public Task Delete(Guid id)
        {
            var ent = Get(id);
            _context.Remove(ent);
            return Task.CompletedTask;
        }

        public async Task Commit()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}