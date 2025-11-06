using GUIWebAPI.Controllers;
using GUIWebAPI.Mapping;
using GUIWebAPI.Models;
using GUIWebAPI.Models.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace WebAPIUnitTest
{
    internal static class TestHelper
    {
        static TestHelper()
        {
            MapsterConfig.RegisterGlobal();
        }

        public static DBContext CreateInMemoryDbContext(string databaseName)
        {
            DbContextOptions<DBContext> options = new DbContextOptionsBuilder<DBContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            return new DBContext(options);
        }

        public static ControllerContext CreateControllerContext(string scheme, HostString host)
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = scheme;
            httpContext.Request.Host = host;

            return new ControllerContext
            {
                HttpContext = httpContext
            };
        }
    }

    public class ProductsControllerTests
    {
        [Fact]
        public async Task GetAll_ReturnsOk_WithPaginationHeaders_AndAbsoluteImageUrls()
        {
            string dbName = nameof(GetAll_ReturnsOk_WithPaginationHeaders_AndAbsoluteImageUrls);
            DBContext db = TestHelper.CreateInMemoryDbContext(dbName);

            Category cat = new Category { CategoryId = 1, Name = "Phones" };
            ImageFile img = new ImageFile { ImageFileId = 1, FileName = "p1.jpg", RelativePath = "/images/p1.jpg" };
            Product p1 = new Product { ProductId = 1, Name = "A Phone", Price = 100M, CategoryId = cat.CategoryId, Category = cat, ImageFileId = img.ImageFileId, ImageFile = img };
            Product p2 = new Product { ProductId = 2, Name = "B Phone", Price = 120M, CategoryId = cat.CategoryId, Category = cat, ImageFileId = img.ImageFileId, ImageFile = img };

            await db.Categories.AddAsync(cat);
            await db.ImageFiles.AddAsync(img);
            await db.Products.AddRangeAsync(p1, p2);
            await db.SaveChangesAsync();

            Mock<ILogger<ProductsController>> loggerMock = new Mock<ILogger<ProductsController>>();
            ProductsController controller = new ProductsController(db, loggerMock.Object);
            controller.ControllerContext = TestHelper.CreateControllerContext("https", new HostString("example.test"));

            ActionResult<IEnumerable<ProductReadDto>> action = await controller.GetAll(pageNumber: 1, pageSize: 10);

            OkObjectResult ok = Assert.IsType<OkObjectResult>(action.Result);
            List<ProductReadDto> items = Assert.IsAssignableFrom<IEnumerable<ProductReadDto>>(ok.Value).ToList();

            Assert.True(items.Count >= 2);
            foreach (ProductReadDto dto in items)
            {
                Assert.StartsWith("https://example.test/", dto.ImageUrl, StringComparison.OrdinalIgnoreCase);
            }

            IHeaderDictionary headers = controller.Response.Headers;
            Assert.True(headers.ContainsKey("X-Total-Count"));
            Assert.True(headers.ContainsKey("X-Total-Pages"));
            Assert.Equal("1", headers["X-Page-Number"].ToString());
            Assert.Equal("10", headers["X-Page-Size"].ToString());
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenProductDoesNotExist()
        {
            DBContext db = TestHelper.CreateInMemoryDbContext(nameof(GetById_ReturnsNotFound_WhenProductDoesNotExist));
            Mock<ILogger<ProductsController>> loggerMock = new Mock<ILogger<ProductsController>>();
            ProductsController controller = new ProductsController(db, loggerMock.Object);
            controller.ControllerContext = TestHelper.CreateControllerContext("http", new HostString("localhost"));

            ActionResult<ProductReadDto> action = await controller.GetById(9999);

            Assert.IsType<NotFoundResult>(action.Result);
        }
    }

    public class CategoriesControllerTests
    {
        [Fact]
        public async Task GetAll_ReturnsFlatDtos_WhenIncludeProductsIsFalse()
        {
            DBContext db = TestHelper.CreateInMemoryDbContext(nameof(GetAll_ReturnsFlatDtos_WhenIncludeProductsIsFalse));

            Category c1 = new Category { CategoryId = 1, Name = "Books" };
            Category c2 = new Category { CategoryId = 2, Name = "Games" };
            Product p1 = new Product { ProductId = 1, Name = "Book A", Price = 10M, CategoryId = c1.CategoryId };
            Product p2 = new Product { ProductId = 2, Name = "Game A", Price = 50M, CategoryId = c2.CategoryId };

            await db.Categories.AddRangeAsync(c1, c2);
            await db.Products.AddRangeAsync(p1, p2);
            await db.SaveChangesAsync();

            CategoriesController controller = new CategoriesController(db);
            controller.ControllerContext = TestHelper.CreateControllerContext("http", new HostString("localhost"));

            ActionResult<IEnumerable<Category>> action = await controller.GetAll(includeProducts: false);

            OkObjectResult ok = Assert.IsType<OkObjectResult>(action.Result);
            List<CategoryReadDto> flat = Assert.IsAssignableFrom<IEnumerable<CategoryReadDto>>(ok.Value).ToList();

            Assert.Equal(2, flat.Count);
            Assert.All(flat, dto => Assert.True(dto.CategoryId > 0));
            Assert.All(flat, dto => Assert.False(string.IsNullOrWhiteSpace(dto.Name)));
        }

        [Fact]
        public async Task GetById_ReturnsWithProducts_WhenIncludeProductsIsTrue()
        {
            DBContext db = TestHelper.CreateInMemoryDbContext(nameof(GetById_ReturnsWithProducts_WhenIncludeProductsIsTrue));

            Category c = new Category { CategoryId = 10, Name = "Accessories" };
            Product p1 = new Product { ProductId = 101, Name = "Case", Price = 15M, CategoryId = c.CategoryId };
            Product p2 = new Product { ProductId = 102, Name = "Charger", Price = 25M, CategoryId = c.CategoryId };

            await db.Categories.AddAsync(c);
            await db.Products.AddRangeAsync(p1, p2);
            await db.SaveChangesAsync();

            CategoriesController controller = new CategoriesController(db);
            controller.ControllerContext = TestHelper.CreateControllerContext("http", new HostString("localhost"));

            ActionResult<object> action = await controller.GetById(c.CategoryId, includeProducts: true);

            OkObjectResult ok = Assert.IsType<OkObjectResult>(action.Result);
            CategoryWithProductsReadDto dto = Assert.IsType<CategoryWithProductsReadDto>(ok.Value);
            Assert.Equal(c.CategoryId, dto.CategoryId);
            Assert.Equal(c.Name, dto.Name);
            Assert.NotNull(dto.Products);
            Assert.True(dto.Products.Count >= 2);
        }
    }

    public class ImageFilesControllerTests
    {
        [Fact]
        public async Task GetAll_MapsToReadDto_AndMakesAbsoluteUrl()
        {
            DBContext db = TestHelper.CreateInMemoryDbContext(nameof(GetAll_MapsToReadDto_AndMakesAbsoluteUrl));

            ImageFile i1 = new ImageFile { ImageFileId = 1, FileName = "x.jpg", RelativePath = "/images/x.jpg" };
            ImageFile i2 = new ImageFile { ImageFileId = 2, FileName = "y.png", RelativePath = "/images/y.png" };
            await db.ImageFiles.AddRangeAsync(i1, i2);
            await db.SaveChangesAsync();

            Mock<IWebHostEnvironment> envMock = new Mock<IWebHostEnvironment>();
            envMock.SetupGet(e => e.WebRootPath).Returns("C:\\fake\\wwwroot");

            ImageFilesController controller = new ImageFilesController(db, envMock.Object);
            controller.ControllerContext = TestHelper.CreateControllerContext("https", new HostString("cdn.example"));

            ActionResult<IEnumerable<ImageFileReadDto>> action = await controller.GetAll();

            OkObjectResult ok = Assert.IsType<OkObjectResult>(action.Result);
            List<ImageFileReadDto> dtos = Assert.IsAssignableFrom<IEnumerable<ImageFileReadDto>>(ok.Value).ToList();

            Assert.Equal(2, dtos.Count);
            Assert.All(dtos, d => Assert.StartsWith("https://cdn.example/", d.Url, StringComparison.OrdinalIgnoreCase));
        }
    }
}