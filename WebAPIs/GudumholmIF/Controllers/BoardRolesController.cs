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

        public BoardRolesController(ClubContext db)
        {
            this.db = db;
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

        [HttpPost]
        public async Task<ActionResult<BoardRoleDto>> Create([FromBody] BoardRoleCreateDto dto, CancellationToken ct)
        {
            bool hasOpen = await db.BoardRoles.AnyAsync(r => r.PersonId == dto.PersonId && r.To == null, ct);
            if (hasOpen) return Conflict("Person already has an open board role.");

            BoardRole role = dto.Adapt<BoardRole>();
            db.BoardRoles.Add(role);
            await db.SaveChangesAsync(ct);

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
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            BoardRole role = await db.BoardRoles.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (role == null) return NotFound();

            db.BoardRoles.Remove(role);
            await db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}