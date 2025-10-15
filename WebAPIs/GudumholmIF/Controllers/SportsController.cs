using GudumholmIF.Models;
using GudumholmIF.Models.Application;
using GudumholmIF.Models.DTOs.Sport;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GudumholmIF.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class SportsController : ControllerBase
    {
        private readonly ClubContext db;

        public SportsController(ClubContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SportDto>>> GetAll(CancellationToken ct)
        {
            List<Sport> entities = await db.Sports.AsNoTracking().ToListAsync(ct);
            List<SportDto> dtos = entities.Adapt<List<SportDto>>();
            return Ok(dtos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SportDto>> Get(int id, CancellationToken ct)
        {
            Sport entity = await db.Sports.FirstOrDefaultAsync(s => s.Id == id, ct);
            if (entity == null) return NotFound();
            SportDto dto = entity.Adapt<SportDto>();
            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<SportDto>> Create([FromBody] SportCreateDto dto, CancellationToken ct)
        {
            Sport entity = dto.Adapt<Sport>();
            db.Sports.Add(entity);

            SportFeeHistory history = new SportFeeHistory
            {
                Sport = entity,
                AnnualFee = entity.AnnualFee,
                EffectiveFrom = DateOnly.FromDateTime(DateTime.Today),
                Reason = "Initial fee"
            };
            db.SportFeeHistories.Add(history);

            await db.SaveChangesAsync(ct);

            SportDto result = entity.Adapt<SportDto>();
            return CreatedAtAction(nameof(Get), new { id = entity.Id }, result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] SportUpdateDto dto, CancellationToken ct)
        {
            Sport entity = await db.Sports.FirstOrDefaultAsync(s => s.Id == id, ct);
            if (entity == null) return NotFound();

            bool feeChanged = dto.AnnualFee != entity.AnnualFee;
            dto.Adapt(entity);

            if (feeChanged)
            {
                SportFeeHistory history = new SportFeeHistory
                {
                    SportId = entity.Id,
                    AnnualFee = entity.AnnualFee,
                    EffectiveFrom = DateOnly.FromDateTime(DateTime.Today),
                    Reason = "Updated via API"
                };
                db.SportFeeHistories.Add(history);
            }

            await db.SaveChangesAsync(ct);
            return NoContent();
        }

        [HttpPost("{id:int}/fee-change")]
        public async Task<IActionResult> ChangeFee(int id, [FromBody] SportFeeChangeDto dto, CancellationToken ct)
        {
            Sport entity = await db.Sports.FirstOrDefaultAsync(s => s.Id == id, ct);
            if (entity == null) return NotFound();

            entity.AnnualFee = dto.AnnualFee;
            SportFeeHistory history = dto.Adapt<SportFeeHistory>();
            history.SportId = id;
            db.SportFeeHistories.Add(history);

            await db.SaveChangesAsync(ct);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
        {
            Sport entity = await db.Sports.FirstOrDefaultAsync(s => s.Id == id, ct);
            if (entity == null) return NotFound();

            if (!entity.IsActive) return NoContent();

            entity.IsActive = false;
            await db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}