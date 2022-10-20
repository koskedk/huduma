using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Huduma.Billing.Application.Commands;
using Huduma.Billing.Application.Queries;
using Huduma.Billing.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Huduma.Billing.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CashierController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CashierController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Query(Guid billNo)
        {
            Log.Debug("Finding Bills");
            var result = await _mediator.Send(new GetBillQuery(billNo));
            if (result.IsSuccess)
                return Ok(result.Value);

            return NotFound();
        }
        
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            Log.Debug("Loading Bills");
            var result = await _mediator.Send(new LoadBillsQuery());
            if (result.IsSuccess)
                return Ok(result.Value);

            return Ok(new List<Bill>());
        }
        
        [HttpPost]
        public async Task<IActionResult> ChargeClient(BillDto billDto)
        {
            Log.Debug("Charging Bills");
            var result = await _mediator.Send(new ChargeClientCommand(billDto.Client,billDto.Amount));
            if (result.IsSuccess)
                return Ok();

            return BadRequest();
        }
        
        [HttpPut]
        public async Task<IActionResult> AcceptPayment(PayDto payDto)
        {
            Log.Debug("Paying Bills");
            var result = await _mediator.Send(new AcceptPaymentCommand(payDto.BillNo,payDto.Amount));
            if (result.IsSuccess)
                return Ok();

            return BadRequest();
        }
    }
}