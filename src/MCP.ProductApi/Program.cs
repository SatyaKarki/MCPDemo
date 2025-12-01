using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MCP.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Add CORS for browser-based Swagger/UI testing (demo only)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<ProductDbContext>(opt => opt.UseInMemoryDatabase("ProductsDb"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply CORS before serving Swagger/UI and endpoints
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/products", async (ProductDbContext db) => Results.Ok(await db.Products.ToListAsync()));

app.MapGet("/products/{id:long}", async (long id, ProductDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

app.MapPost("/products", async (ProductDto dto, ProductDbContext db) =>
{
    var product = new ProductItem { Name = dto.Name, Price = dto.Price, Description = dto.Description, IsActive = dto.IsActive };
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/products/{product.Id}", product);
});

app.MapPut("/products/{id:long}", async (long id, ProductDto dto, ProductDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();
    product.Name = dto.Name;
    product.Price = dto.Price;
    product.Description = dto.Description;
    product.IsActive = dto.IsActive;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/products/{id:long}", async (long id, ProductDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();
    db.Products.Remove(product);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

public class ProductDto
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }
    public DbSet<ProductItem> Products => Set<ProductItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });
    }
}
