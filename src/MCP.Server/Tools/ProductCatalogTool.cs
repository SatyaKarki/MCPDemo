using System.ComponentModel;
using System.Net.Http.Json;
using MCP.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace MCP.Server.Tools;

/// <summary>
/// Product Catalog Tool that demonstrates MCP Server calling an external API.
/// This tool makes HTTP requests to the ProductApi to manage products.
/// </summary>
[McpServerToolType]
public class ProductCatalogTool
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string ApiBaseUrl = "http://localhost:57724";

    public ProductCatalogTool(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [McpServerTool, Description("Search products in the catalog by name or list all products from the API.")]
    public async Task<List<ProductItem>> SearchProductsAsync(
        [Description("Search term to filter products by name (optional)")] string? searchTerm = null)
    {
        var client = _httpClientFactory.CreateClient();
        var products = await client.GetFromJsonAsync<List<ProductItem>>($"{ApiBaseUrl}/products");
        
        if (products == null) return new List<ProductItem>();
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            products = products.Where(p => 
                p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (p.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }
        
        return products;
    }

    [McpServerTool, Description("Get a specific product by ID from the catalog API.")]
    public async Task<ProductItem?> GetProductByIdAsync(
        [Description("Product ID")] long id)
    {
        var client = _httpClientFactory.CreateClient();
        try
        {
            return await client.GetFromJsonAsync<ProductItem>($"{ApiBaseUrl}/products/{id}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    [McpServerTool, Description("Add a new product to the catalog via API.")]
    public async Task<ProductItem?> AddProductAsync(
        [Description("Product name")] string name,
        [Description("Product price")] decimal price,
        [Description("Product description (optional)")] string? description = null,
        [Description("Is the product active? (default: true)")] bool isActive = true)
    {
        var client = _httpClientFactory.CreateClient();
        var productDto = new
        {
            Name = name,
            Price = price,
            Description = description,
            IsActive = isActive
        };
        
        var response = await client.PostAsJsonAsync($"{ApiBaseUrl}/products", productDto);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ProductItem>();
        }
        
        return null;
    }

    [McpServerTool, Description("Update an existing product in the catalog via API.")]
    public async Task<bool> UpdateProductAsync(
        [Description("Product ID")] long id,
        [Description("Product name")] string name,
        [Description("Product price")] decimal price,
        [Description("Product description (optional)")] string? description = null,
        [Description("Is the product active?")] bool isActive = true)
    {
        var client = _httpClientFactory.CreateClient();
        var productDto = new
        {
            Name = name,
            Price = price,
            Description = description,
            IsActive = isActive
        };
        
        var response = await client.PutAsJsonAsync($"{ApiBaseUrl}/products/{id}", productDto);
        return response.IsSuccessStatusCode;
    }

    [McpServerTool, Description("Delete a product from the catalog via API.")]
    public async Task<bool> DeleteProductAsync(
        [Description("Product ID")] long id)
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.DeleteAsync($"{ApiBaseUrl}/products/{id}");
        return response.IsSuccessStatusCode;
    }

    [McpServerTool, Description("Get active products only from the catalog.")]
    public async Task<List<ProductItem>> GetActiveProductsAsync()
    {
        var client = _httpClientFactory.CreateClient();
        var products = await client.GetFromJsonAsync<List<ProductItem>>($"{ApiBaseUrl}/products");
        
        return products?.Where(p => p.IsActive).ToList() ?? new List<ProductItem>();
    }

    [McpServerTool, Description("Get products within a price range from the catalog.")]
    public async Task<List<ProductItem>> GetProductsByPriceRangeAsync(
        [Description("Minimum price")] decimal minPrice,
        [Description("Maximum price")] decimal maxPrice)
    {
        var client = _httpClientFactory.CreateClient();
        var products = await client.GetFromJsonAsync<List<ProductItem>>($"{ApiBaseUrl}/products");
        
        return products?.Where(p => p.Price >= minPrice && p.Price <= maxPrice).ToList() 
               ?? new List<ProductItem>();
    }
}
