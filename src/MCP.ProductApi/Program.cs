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

// Seed the database with dummy product catalog data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    
    if (!db.Products.Any())
    {
        db.Products.AddRange(
            new ProductItem { Name = "Laptop", Price = 1299.99m, Description = "High-performance laptop with 16GB RAM", IsActive = true },
            new ProductItem { Name = "Wireless Mouse", Price = 29.99m, Description = "Ergonomic wireless mouse with USB receiver", IsActive = true },
            new ProductItem { Name = "Mechanical Keyboard", Price = 89.99m, Description = "RGB mechanical keyboard with cherry switches", IsActive = true },
            new ProductItem { Name = "USB-C Hub", Price = 49.99m, Description = "7-in-1 USB-C hub with HDMI and ethernet", IsActive = true },
            new ProductItem { Name = "Webcam HD", Price = 79.99m, Description = "1080p HD webcam with built-in microphone", IsActive = true },
            new ProductItem { Name = "Monitor 27\"", Price = 349.99m, Description = "27-inch 4K IPS monitor with HDR", IsActive = true },
            new ProductItem { Name = "Desk Lamp", Price = 39.99m, Description = "LED desk lamp with adjustable brightness", IsActive = true },
            new ProductItem { Name = "Headphones", Price = 149.99m, Description = "Noise-cancelling wireless headphones", IsActive = true },
            new ProductItem { Name = "External SSD 1TB", Price = 119.99m, Description = "Portable external SSD with USB 3.2", IsActive = true },
            new ProductItem { Name = "Smartphone Stand", Price = 19.99m, Description = "Adjustable aluminum smartphone stand", IsActive = false }
        );
        db.SaveChanges();
    }
}

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
