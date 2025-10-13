using GUIWebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GUIWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly DBContext _db;

        public CategoriesController(DBContext db) 
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetAll([FromQuery] bool includeProducts = false, [FromQuery] string q = null) 
        {
            IQueryable<Category> query = _db.Categories.AsNoTracking();

            if (includeProducts)
                query = query.Include(c => c.Products);

            if (!string.IsNullOrWhiteSpace(q)) 
            {
                string term = q.Trim();
                query = query.Where(c => EF.Functions.Like(c.Name, "%" + term + "%"));
            }

            List<Category> items = await query.OrderBy(c => c.Name).ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Category>> GetById(int id, [FromQuery] bool includeProducts = false)
        {
            IQueryable<Category> query = _db.Categories.AsNoTracking();
            if (includeProducts)
            {
                query = query.Include(c => c.Products);
            }

            Category category = await query.FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null) return NotFound();

            return Ok(category);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Category>>> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return Ok(Array.Empty<Category>());
            string term = q.Trim();

            List<Category> items = await _db.Categories
                .AsNoTracking()
                .Where(c => EF.Functions.Like(c.Name, "%" + term + "%"))
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(items);
        }

        [HttpPost]
        public async Task<ActionResult<Category>> Create([FromBody] Category input)
        {
            if (input == null) return BadRequest();

            Category entity = new Category
            {
                Name = input.Name?.Trim() ?? string.Empty
            };

            await _db.Categories.AddAsync(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.CategoryId }, entity);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Category input)
        {
            if (input == null || id != input.CategoryId) return BadRequest();

            Category current = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
            if (current == null) return NotFound();

            current.Name = input.Name?.Trim() ?? string.Empty;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            Category current = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
            if (current == null) return NotFound();

            _db.Categories.Remove(current);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}