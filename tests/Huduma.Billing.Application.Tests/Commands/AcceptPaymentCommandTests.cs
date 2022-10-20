using System;
using System.Linq;
using System.Threading.Tasks;
using Huduma.Billing.Application.Commands;
using Huduma.Billing.Infrastructure;
using Huduma.Contracts;
using MassTransit.Testing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Serilog;

namespace Huduma.Billing.Application.Tests.Commands;

[TestFixture]
public class AcceptPaymentCommandTests
{
    private IMediator _mediator;
    private ITestHarness _testHarness;
    private ISagaStateMachineTestHarness<BillStateMachine, BillState> _sagaHarness;

    [SetUp]
    public void SetUp()
    {
        _testHarness = TestInit.TestHarness;
        _sagaHarness = _testHarness.GetSagaStateMachineHarness<BillStateMachine, BillState>();
        _mediator = TestInit.ServiceProvider.GetService<IMediator>();
    }

    [Test]
    public  async Task should_Execute_Partial()
    {
        var billNo = GetBillNo("Mama MIa",500);

        var result = await _mediator.Send(new AcceptPaymentCommand(billNo,490));
        
        Assert.That(result.IsSuccess);
        
        Assert.That(await _testHarness.Consumed.Any<PaymentReceived>());
        Assert.That(await _sagaHarness.Consumed.Any<PaymentReceived>());
            
        var instance = _sagaHarness.Created.ContainsInState(billNo, _sagaHarness.StateMachine, _sagaHarness.StateMachine.Paid);
        Assert.IsNotNull(instance, "Saga instance not found");
        Assert.That(instance.CorrelationId, Is.EqualTo(billNo));

        Assert.IsTrue(await _testHarness.Published.Any<PaymentReceived>());
        Log.Debug($"{instance.ToString()}");
    }

    
    [Test]
    public  async Task should_Execute_Full_Payment()
    {
        var billNo = GetBillNo("Baba Kia",500);

        var result = await _mediator.Send(new AcceptPaymentCommand(billNo,500));
        
        Assert.That(result.IsSuccess);
        
        Assert.That(await _testHarness.Consumed.Any<BillCleared>());
        Assert.That(await _sagaHarness.Consumed.Any<BillCleared>());
            
        var instance = _sagaHarness.Created.ContainsInState(billNo, _sagaHarness.StateMachine, _sagaHarness.StateMachine.Closed);
        Assert.IsNotNull(instance, "Saga instance not found");
        Assert.That(instance.CorrelationId, Is.EqualTo(billNo));

        Assert.IsTrue(await _testHarness.Published.Any<BillCleared>());
        Log.Debug($"{instance.ToString()}");
    }
    private Guid GetBillNo(string name,double amount)
    {
        var client = $"{name} {DateTime.Now.Ticks}";
        var result=  _mediator.Send(new ChargeClientCommand(client, amount)).Result;
        var context = TestInit.ServiceProvider.GetService<BillingDbContext>();
        var savedBill = context.Bills.FirstOrDefault(x => x.Client == client);
        return savedBill.BillNo;
    }
}