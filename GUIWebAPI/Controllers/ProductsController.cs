using GUIWebAPI.Models;
using GUIWebAPI.Models.DTOs;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GUIWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly DBContext db;

        public ProductsController(DBContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetAll([FromQuery] int? categoryId = null, [FromQuery] string q = null)
        {
            IQueryable<Product> query = db.Products
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

            List<ProductReadDto> result = await query.OrderBy(p => p.Name).ThenBy(p => p.ProductId).ProjectToType<ProductReadDto>().ToListAsync();

            foreach (ProductReadDto dto in result)
            {
                dto.ImageUrl = MakeAbsoluteUrl(dto.ImageUrl);
            }

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductReadDto>> GetById(int id)
        {
            Product product = await db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            ProductReadDto dto = product.Adapt<ProductReadDto>();
            dto.ImageUrl = MakeAbsoluteUrl(dto.ImageUrl);

            return Ok(dto);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ProductReadDto>>> Search([FromQuery] string q, [FromQuery] int? categoryId = null)
        {
            if (string.IsNullOrWhiteSpace(q)) return Ok(Array.Empty<ProductReadDto>());
            string term = q.Trim();

            IQueryable<Product> query = db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => EF.Functions.Like(p.Name, "%" + term + "%"));

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            List<ProductReadDto> result = await query.OrderBy(p => p.Name).ThenBy(p => p.ProductId).ProjectToType<ProductReadDto>().ToListAsync();

            foreach (ProductReadDto dto in result)
            {
                dto.ImageUrl = MakeAbsoluteUrl(dto.ImageUrl);
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ProductReadDto>> Create([FromBody] ProductCreateDto input)
        {
            if (input == null) return BadRequest();

            bool categoryExists = await db.Categories.AnyAsync(c => c.CategoryId == input.CategoryId);
            if (!categoryExists)
            {
                return BadRequest(new { message = "Invalid CategoryId." });
            }

            if (input.ImageFileId.HasValue)
            {
                bool imageExists = await db.ImageFiles.AnyAsync(i => i.ImageFileId == input.ImageFileId.Value);
                if (!imageExists) return BadRequest(new { message = "Invalid ImageFileId." });
            }

            Product entity = input.Adapt<Product>();

            await db.Products.AddAsync(entity);
            await db.SaveChangesAsync();

            await db.Entry(entity).Reference(p => p.Category).LoadAsync();
            await db.Entry(entity).Reference(p => p.ImageFile).LoadAsync();

            ProductReadDto dto = entity.Adapt<ProductReadDto>();
            dto.ImageUrl = MakeAbsoluteUrl(dto.ImageUrl);

            return CreatedAtAction(nameof(GetById), new { id = entity.ProductId }, dto);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product input)
        {
            if (input == null || id != input.ProductId) return BadRequest();

            Product current = await db.Products.FirstOrDefaultAsync(p => p.ProductId == id);
            if (current == null) return NotFound();

            if (current.CategoryId != input.CategoryId)
            {
                bool categoryExists = await db.Categories.AnyAsync(c => c.CategoryId == input.CategoryId);
                if (!categoryExists)
                {
                    return BadRequest(new { message = "Invalid CategoryId." });
                }
            }

            if (current.ImageFileId != input.ImageFileId)
            {
                if (input.ImageFileId.HasValue)
                {
                    bool imageExists = await db.ImageFiles.AnyAsync(i => i.ImageFileId == input.ImageFileId.Value);
                    if (!imageExists) return BadRequest(new { message = "Invalid ImageFileId." });
                }
            }

            current.Name = input.Name?.Trim() ?? string.Empty;
            current.Price = input.Price;
            current.CategoryId = input.CategoryId;
            current.ImageFileId = input.ImageFileId;

            await db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            Product current = await db.Products.FirstOrDefaultAsync(p => p.ProductId == id);
            if (current == null) return NotFound();

            db.Products.Remove(current);
            await db.SaveChangesAsync();
            return NoContent();
        }

        private string MakeAbsoluteUrl(string virtualOrRelativePath)
        {
            if (string.IsNullOrWhiteSpace(virtualOrRelativePath)) return string.Empty;
            if (virtualOrRelativePath.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return virtualOrRelativePath;

            string baseUrl = string.Concat(Request.Scheme, "://", Request.Host.ToUriComponent());
            string path = virtualOrRelativePath.StartsWith('/') ? virtualOrRelativePath : '/' + virtualOrRelativePath;
            return baseUrl + path;
        }
    }
}