using GudumholmIF.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GudumholmIF.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class FeesController : ControllerBase
    {
        private readonly IFeeCalculator _calculator;

        public FeesController(IFeeCalculator calculator)
        {
            _calculator = calculator;
        }

        [HttpGet("person/{personId:int}")]
        public async Task<ActionResult<decimal>> Person(int personId, CancellationToken ct)
        {
            decimal sum = await _calculator.PersonAnnualAsync(personId, ct);
            return Ok(sum);
        }

        [HttpGet("household/{householdId:int}")]
        public async Task<ActionResult<decimal>> Household(int householdId, CancellationToken ct)
        {
            decimal sum = await _calculator.HouseholdAnnualAsync(householdId, ct);
            return Ok(sum);
        }

        [HttpGet("sport/{sportId:int}")]
        public async Task<ActionResult<decimal>> Sport(int sportId, CancellationToken ct)
        {
            decimal sum = await _calculator.SportAnnualAsync(sportId, ct);
            return Ok(sum);
        }

        [HttpGet("all-sports")]
        public async Task<ActionResult<decimal>> AllSports(CancellationToken ct)
        {
            decimal sum = await _calculator.AllSportsAnnualAsync(ct);
            return Ok(sum);
        }

        [HttpGet("annual-income")]
        public async Task<ActionResult<decimal>> AnnualIncome(CancellationToken ct)
        {
            decimal sum = await _calculator.AllPersonsAnnualAsync(ct);
            return Ok(sum);
        }
    }
}