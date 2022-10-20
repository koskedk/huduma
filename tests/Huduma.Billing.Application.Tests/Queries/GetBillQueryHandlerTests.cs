using System;
using System.Linq;
using System.Threading.Tasks;
using Huduma.Billing.Application.Commands;
using Huduma.Billing.Application.Queries;
using Huduma.Billing.Infrastructure;
using Huduma.Contracts;
using MassTransit.Testing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Serilog;

namespace Huduma.Billing.Application.Tests.Queries;

[TestFixture]
public class GetBillQueryTests
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
    public  async Task should_Query_Open()
    {
        var billNo = GetBillNo("Mama xMIa",500);

        var result = await _mediator.Send(new GetBillQuery(billNo));
        
        Assert.That(result.IsSuccess);
        Assert.IsNotNull(result.Value);
        
        Assert.That(await _sagaHarness.Consumed.Any<CheckBill>());
            
        var instance = _sagaHarness.Created.ContainsInState(billNo, _sagaHarness.StateMachine, _sagaHarness.StateMachine.Open);
        Assert.IsNotNull(instance, "Saga instance not found");
        
        Assert.That(instance.CorrelationId, Is.EqualTo(result.Value.BillNo));
        Assert.That(instance.Client, Is.EqualTo(result.Value.Client));
        Assert.That(instance.Amount, Is.EqualTo(result.Value.Amount));

        Assert.IsTrue(await _testHarness.Published.Any<CheckBill>());
        Log.Debug($"{result.Value.Client}, Your Bill:{result.Value.Amount:C} [{instance.CurrentState}]");
    }

    
    [Test]
    public  async Task should_Query_NotFound()
    {
        var billNo = Guid.NewGuid();

        var result = await _mediator.Send(new GetBillQuery(billNo));
        
        Assert.That(result.IsFailure);
        
        Assert.That(await _testHarness.Consumed.Any<BillNotFound>());
            
        var instance = _sagaHarness.Created.ContainsInState(billNo, _sagaHarness.StateMachine, _sagaHarness.StateMachine.Open);
        Assert.IsNull(instance, "Saga instance exists found");
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