using System;
using System.Linq;
using System.Threading.Tasks;
using Huduma.Billing.Application.Commands;
using Huduma.Billing.Infrastructure;
using Huduma.Contracts;
using MassTransit;
using MassTransit.Testing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Serilog;

namespace Huduma.Billing.Application.Tests.Commands;

[TestFixture]
public class ChargeClientCommandTests
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
    public  async Task should_Exectute()
    {
        var client = $"Mary Donde {DateTime.Now.Ticks}";
        var result =await _mediator.Send(new ChargeClientCommand(client, 500));
        
        Assert.That(result.IsSuccess,"Not successful!");

        var context = TestInit.ServiceProvider.GetService<BillingDbContext>();
        var savedBill = context.Bills.FirstOrDefault(x => x.Client == client);
        
        Assert.NotNull(savedBill,"Bill not saved");
        Assert.That(await _testHarness.Consumed.Any<ClientBilled>());
        Assert.That(await _sagaHarness.Consumed.Any<ClientBilled>());
            
        var instance = _sagaHarness.Created.ContainsInState(savedBill.BillNo, _sagaHarness.StateMachine, _sagaHarness.StateMachine.Open);
        Assert.IsNotNull(instance, "Saga instance not found");
        Assert.That(instance.CorrelationId, Is.EqualTo(savedBill.BillNo));

        Assert.IsTrue(await _testHarness.Published.Any<ClientBilled>());
        Log.Debug($"{instance.ToString()}");
    }
}