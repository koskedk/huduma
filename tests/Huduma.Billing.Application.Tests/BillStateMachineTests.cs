using System;
using System.Threading.Tasks;
using Huduma.Contracts;
using MassTransit.Testing;
using NUnit.Framework;
using Serilog;

namespace Huduma.Billing.Application.Tests
{
    [TestFixture]
    public class BillStateMachineTests
    {
        private ITestHarness _testHarness;
        private ISagaStateMachineTestHarness<BillStateMachine, BillState> _sagaHarness;

        [SetUp]
        public async Task Setup()
        {

            _testHarness = TestInit.TestHarness;
            _sagaHarness = _testHarness.GetSagaStateMachineHarness<BillStateMachine, BillState>();
        }

        [Test]
        public async Task should_BillClient()
        {
            var billNo = Guid.NewGuid();
            var client = "Joe Donde";
            var amount = 1000.80;
            
            await _testHarness.Bus.Publish<ClientBilled>(new
            {
                BillNo = billNo,
                Client = client,
                Amount = amount
            });

            Assert.That(await _testHarness.Consumed.Any<ClientBilled>());
            Assert.That(await _sagaHarness.Consumed.Any<ClientBilled>());
            Assert.That(await _sagaHarness.Created.Any(x => x.CorrelationId == billNo));
            
            var instance = _sagaHarness.Created.ContainsInState(billNo, _sagaHarness.StateMachine, _sagaHarness.StateMachine.Open);
            Assert.IsNotNull(instance, "Saga instance not found");
            Assert.That(instance.CorrelationId, Is.EqualTo(billNo));

            Assert.IsTrue(await _testHarness.Published.Any<ClientBilled>());
            Log.Debug($"{instance.ToString()}");
        }
        
        [Test]
        public async Task should_ReceivePayment()
        {
            var billNo = Guid.NewGuid();
            var client = "Joe Donde";
            var amount = 1000.80;
            
            await _testHarness.Bus.Publish<ClientBilled>(new
            {
                BillNo = billNo,
                Client = client,
                Amount = amount
            });
            
            await _testHarness.Bus.Publish<PaymentReceived>(new
            {
                BillNo = billNo,
                Amount = amount
            });

            Assert.That(await _testHarness.Consumed.Any<PaymentReceived>());
            Assert.That(await _sagaHarness.Consumed.Any<PaymentReceived>());
            Assert.That(await _sagaHarness.Created.Any(x => x.CorrelationId == billNo));
            
            var instance = _sagaHarness.Created.ContainsInState(billNo, _sagaHarness.StateMachine, _sagaHarness.StateMachine.Paid);
            Assert.IsNotNull(instance, "Saga instance not found");
            Assert.That(instance.CorrelationId, Is.EqualTo(billNo));

            Assert.IsTrue(await _testHarness.Published.Any<PaymentReceived>());
            Log.Debug($"{instance.ToString()}");
        }
        
        [Test]
        public async Task should_ClearBill()
        {
            var billNo = Guid.NewGuid();
            var client = "Joe Donde";
            var amount = 1000.80;
            
            await _testHarness.Bus.Publish<ClientBilled>(new
            {
                BillNo = billNo,
                Client = client,
                Amount = amount
            });
            
            await _testHarness.Bus.Publish<PaymentReceived>(new
            {
                BillNo = billNo,
                Amount = amount
            });
            
            await _testHarness.Bus.Publish<BillCleared>(new
            {
                BillNo = billNo
            });

            Assert.That(await _testHarness.Consumed.Any<BillCleared>());
            Assert.That(await _sagaHarness.Consumed.Any<BillCleared>());
            
            Assert.That(await _testHarness.Consumed.Any<BillCleared>());
            Assert.That(await _sagaHarness.Consumed.Any<BillCleared>());
            Assert.That(await _sagaHarness.Created.Any(x => x.CorrelationId == billNo));
            
            var instance = _sagaHarness.Created.ContainsInState(billNo, _sagaHarness.StateMachine, _sagaHarness.StateMachine.Closed);
            Assert.IsNotNull(instance, "Saga instance not found");
            Assert.That(instance.CorrelationId, Is.EqualTo(billNo));

            Assert.IsTrue(await _testHarness.Published.Any<BillCleared>());
            Log.Debug($"{instance.ToString()}");
        }
    }
}