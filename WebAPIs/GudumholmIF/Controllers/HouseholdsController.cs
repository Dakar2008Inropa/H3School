using GudumholmIF.Models;
using GudumholmIF.Models.Application;
using GudumholmIF.Models.DTOs.Household;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GudumholmIF.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HouseholdsController : ControllerBase
    {
        private readonly ClubContext db;

        public HouseholdsController(ClubContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HouseholdDto>>> GetAll(CancellationToken ct)
        {
            List<Household> entities = await db.HouseHolds.Include(h => h.Members).ToListAsync(ct);
            List<HouseholdDto> dtos = entities.Adapt<List<HouseholdDto>>();

            return Ok(dtos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<HouseholdDto>> Get(int id, CancellationToken ct)
        {
            Household entity = await db.HouseHolds.Include(h => h.Members).SingleOrDefaultAsync(h => h.Id == id, ct);
            if (entity == null) return NotFound();
            HouseholdDto dto = entity.Adapt<HouseholdDto>();
            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<HouseholdDto>> Create([FromBody] HouseholdCreateDto dto, CancellationToken ct)
        {
            Household entity = dto.Adapt<Household>();
            db.HouseHolds.Add(entity);
            await db.SaveChangesAsync(ct);

            HouseholdDto resultDto = entity.Adapt<HouseholdDto>();
            return CreatedAtAction(nameof(Get), new { id = entity.Id }, resultDto);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] HouseholdUpdateDto dto, CancellationToken ct)
        {
            Household entity = await db.HouseHolds.FirstOrDefaultAsync(h => h.Id == id, ct);
            if (entity == null) return NotFound();

            dto.Adapt(entity);

            await db.SaveChangesAsync(ct);

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            Household entity = await db.HouseHolds.FirstOrDefaultAsync(h => h.Id == id, ct);
            if (entity == null) return NotFound();

            if (entity.Members.Count > 0) return Conflict("Household cannot be deleted while it has members.");

            db.HouseHolds.Remove(entity);
            await db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}