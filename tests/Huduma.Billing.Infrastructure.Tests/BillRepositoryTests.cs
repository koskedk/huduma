using System.Threading.Tasks;
using Huduma.Billing.Domain;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Huduma.Billing.Infrastructure.Tests
{
    [TestFixture]
    public class BillRepositoryTests
    {
        private IBillRepository _billRepository;

        [SetUp]
        public void SetUp()
        {
            _billRepository = TestInit.ServiceProvider.GetService<IBillRepository>();
        }

        [Test]
        public async Task should_Add()
        {
            var bill =Bill.Generate("Mary Jane", Money.FromKes(3300));

            await _billRepository.Add(bill);
            await _billRepository.Commit();
            Assert.Pass();
        }
    }
}