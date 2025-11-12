using GudumholmIF.Interfaces;
using GudumholmIF.Models;
using GudumholmIF.Models.Application;
using GudumholmIF.Models.DTOs.BoardAndPersonSport;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GudumholmIF.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BoardRolesController : ControllerBase
    {
        private readonly ClubContext db;
        private readonly IMembershipService membership;

        public BoardRolesController(ClubContext db, IMembershipService membership)
        {
            this.db = db;
            this.membership = membership;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BoardRoleDto>>> GetAll(CancellationToken ct)
        {
            List<BoardRole> roles = await db.BoardRoles.AsNoTracking().ToListAsync(ct);
            return Ok(roles.Adapt<List<BoardRoleDto>>());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<BoardRoleDto>> Get(int id, CancellationToken ct)
        {
            BoardRole role = await db.BoardRoles.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (role == null) return NotFound();
            return Ok(role.Adapt<BoardRoleDto>());
        }

        [HttpGet("by-person/{personId:int}")]
        public async Task<ActionResult<IEnumerable<BoardRoleDto>>> ByPerson(int personId, CancellationToken ct)
        {
            bool exists = await db.Persons.AnyAsync(x => x.Id == personId, ct);
            if (!exists) return NotFound("Person not found.");

            List<BoardRole> roles = await db.BoardRoles.Where(r => r.PersonId == personId).AsNoTracking().ToListAsync(ct);

            return Ok(roles.Adapt<List<BoardRoleDto>>());
        }

        [HttpPost]
        public async Task<ActionResult<BoardRoleDto>> Create([FromBody] BoardRoleCreateDto dto, CancellationToken ct)
        {
            bool hasOpen = await db.BoardRoles.AnyAsync(r => r.PersonId == dto.PersonId && r.To == null, ct);
            if (hasOpen) return Conflict("Person already has an open board role.");

            BoardRole role = dto.Adapt<BoardRole>();
            db.BoardRoles.Add(role);
            await db.SaveChangesAsync(ct);

            await membership.RecalculateAsync(dto.PersonId, "Assigned board role", ct);

            return CreatedAtAction(nameof(Get), new { id = role.Id }, role.Adapt<BoardRoleDto>());
        }

        [HttpPost("{id:int}/close")]
        public async Task<IActionResult> Close(int id, [FromBody] BoardRoleCloseDto dto, CancellationToken ct)
        {
            BoardRole role = await db.BoardRoles.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (role == null) return NotFound();

            if (role.To != null) return NoContent();

            role.To = dto.To;
            await db.SaveChangesAsync(ct);

            await membership.RecalculateAsync(role.PersonId, "Closed board role", ct);

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            BoardRole role = await db.BoardRoles.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (role == null) return NotFound();

            int personId = role.PersonId;

            db.BoardRoles.Remove(role);

            await db.SaveChangesAsync(ct);

            await membership.RecalculateAsync(personId, "Deleted board role", ct);

            return NoContent();
        }
    }
}