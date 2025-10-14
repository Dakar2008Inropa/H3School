using GUIWebAPI.Models;
using GUIWebAPI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GUIWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly DBContext _db;

        public ProductsController(DBContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetAll([FromQuery] int? categoryId = null, [FromQuery] string q = null)
        {
            IQueryable<Product> query = _db.Products
                .AsNoTracking()
                .Include(p => p.Category);

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                string term = q.Trim();
                query = query.Where(p => EF.Functions.Like(p.Name, "%" + term + "%"));
            }

            List<Product> items = await query
                .OrderBy(p => p.Name)
                .ThenBy(p => p.ProductId)
                .ToListAsync();

            List<ProductReadDto> result = items
                .Select(p => new ProductReadDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name
                })
                .ToList();

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductReadDto>> GetById(int id)
        {
            Product product = await _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            ProductReadDto dto = new ProductReadDto 
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name
            };

            return Ok(dto);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ProductReadDto>>> Search([FromQuery] string q, [FromQuery] int? categoryId = null)
        {
            if (string.IsNullOrWhiteSpace(q)) return Ok(Array.Empty<ProductReadDto>());
            string term = q.Trim();

            IQueryable<Product> query = _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => EF.Functions.Like(p.Name, "%" + term + "%"));

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            List<Product> items = await query
                .OrderBy(p => p.Name)
                .ThenBy(p => p.ProductId)
                .ToListAsync();

            List<ProductReadDto> result = items.Select(p => new ProductReadDto
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name
            }).ToList();

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ProductReadDto>> Create([FromBody] ProductCreateDto input)
        {
            if (input == null) return BadRequest();

            bool categoryExists = await _db.Categories.AnyAsync(c => c.CategoryId == input.CategoryId);
            if (!categoryExists)
            {
                return BadRequest(new { message = "Invalid CategoryId." });
            }

            Product entity = new Product
            {
                Name = input.Name?.Trim() ?? string.Empty,
                Price = input.Price,
                CategoryId = input.CategoryId
            };

            await _db.Products.AddAsync(entity);
            await _db.SaveChangesAsync();

            await _db.Entry(entity).Reference(p => p.Category).LoadAsync();

            ProductReadDto dto = new ProductReadDto
            {
                ProductId = entity.ProductId,
                Name = entity.Name,
                Price = entity.Price,
                CategoryId = entity.CategoryId,
                CategoryName = entity.Category?.Name
            };

            return CreatedAtAction(nameof(GetById), new { id = entity.ProductId }, dto);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product input)
        {
            if (input == null || id != input.ProductId) return BadRequest();

            Product current = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == id);
            if (current == null) return NotFound();

            if (current.CategoryId != input.CategoryId)
            {
                bool categoryExists = await _db.Categories.AnyAsync(c => c.CategoryId == input.CategoryId);
                if (!categoryExists)
                {
                    return BadRequest(new { message = "Invalid CategoryId." });
                }
            }

            current.Name = input.Name?.Trim() ?? string.Empty;
            current.Price = input.Price;
            current.CategoryId = input.CategoryId;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            Product current = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == id);
            if (current == null) return NotFound();

            _db.Products.Remove(current);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}