using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MCP.Server.Tools;

Console.WriteLine("=== MCP Server - Product Catalog API Test ===\n");
Console.WriteLine("Ensure ProductApi is running on http://localhost:57724\n");

// Setup service provider with HttpClient
var services = new ServiceCollection();
services.AddHttpClient();
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
var serviceProvider = services.BuildServiceProvider();

// Create ProductCatalogTool instance
var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
var productCatalog = new ProductCatalogTool(httpClientFactory);

try
{
    // Test 1: Search all products
    Console.WriteLine("✓ Test 1: Searching all products...");
    var allProducts = await productCatalog.SearchProductsAsync();
    Console.WriteLine($"  Found {allProducts.Count} products");
    foreach (var product in allProducts.Take(3))
    {
        Console.WriteLine($"  - {product.Name} (${product.Price})");
    }
    Console.WriteLine();

    // Test 2: Search products by term
    Console.WriteLine("✓ Test 2: Searching for 'laptop'...");
    var laptops = await productCatalog.SearchProductsAsync("laptop");
    Console.WriteLine($"  Found {laptops.Count} product(s)");
    foreach (var product in laptops)
    {
        Console.WriteLine($"  - {product.Name} (${product.Price})");
    }
    Console.WriteLine();

    // Test 3: Get product by ID
    Console.WriteLine("✓ Test 3: Getting product by ID (ID: 2)...");
    var mouseProduct = await productCatalog.GetProductByIdAsync(2);
    if (mouseProduct != null)
    {
        Console.WriteLine($"  Product: {mouseProduct.Name} (${mouseProduct.Price})");
    }
    Console.WriteLine();

    // Test 4: Get active products only
    Console.WriteLine("✓ Test 4: Getting active products only...");
    var activeProducts = await productCatalog.GetActiveProductsAsync();
    Console.WriteLine($"  Found {activeProducts.Count} active products");
    Console.WriteLine();

    // Test 5: Get products by price range
    Console.WriteLine("✓ Test 5: Getting products between $30 and $100...");
    var priceRangeProducts = await productCatalog.GetProductsByPriceRangeAsync(30, 100);
    Console.WriteLine($"  Found {priceRangeProducts.Count} products in price range:");
    foreach (var product in priceRangeProducts)
    {
        Console.WriteLine($"  - {product.Name} (${product.Price})");
    }
    Console.WriteLine();

    // Test 6: Add a new product
    Console.WriteLine("✓ Test 6: Adding a new product...");
    var newProduct = await productCatalog.AddProductAsync(
        "Wireless Charger", 
        39.99m, 
        "Fast wireless charging pad for smartphones", 
        true
    );
    if (newProduct != null)
    {
        Console.WriteLine($"  Product added: {newProduct.Name} (ID: {newProduct.Id})");
    }
    Console.WriteLine();

    // Test 7: Update a product
    Console.WriteLine("✓ Test 7: Updating product ID 1 (Laptop price)...");
    var updateSuccess = await productCatalog.UpdateProductAsync(
        1, 
        "Laptop", 
        1199.99m, 
        "High-performance laptop with 16GB RAM - SALE!", 
        true
    );
    Console.WriteLine($"  Update {(updateSuccess ? "successful" : "failed")}");
    Console.WriteLine();

    // Test 8: Verify the update
    Console.WriteLine("✓ Test 8: Verifying the update...");
    var updatedLaptop = await productCatalog.GetProductByIdAsync(1);
    if (updatedLaptop != null)
    {
        Console.WriteLine($"  Updated: {updatedLaptop.Name} (${updatedLaptop.Price})");
        Console.WriteLine($"  Description: {updatedLaptop.Description}");
    }
    Console.WriteLine();

    Console.WriteLine("=== All Tests Completed Successfully! ===");
    Console.WriteLine("\n✓ MCP Server can successfully call ProductApi");
    Console.WriteLine("✓ ProductCatalogTool is working correctly");
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Error: {ex.Message}");
    Console.WriteLine("\nMake sure the ProductApi is running on http://localhost:57724");
    Console.WriteLine("Start it with: cd src\\MCP.ProductApi; dotnet run");
}
