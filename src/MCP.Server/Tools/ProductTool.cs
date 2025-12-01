using System.ComponentModel;
using MCP.Shared.Models;
using ModelContextProtocol.Server;
using System.Collections.Concurrent;

namespace MCP.Server.Tools;

[McpServerToolType]
public static class ProductTool
{
 private static readonly ConcurrentDictionary<long, ProductItem> _products = new();
 private static long _nextId =1;

 [McpServerTool, Description("Create a product item.")]
 public static ProductItem CreateProduct(
 [Description("Product name")] string name,
 [Description("Price")] decimal price,
 [Description("Description")] string? description = null,
 [Description("Is active")] bool isActive = true)
 {
 var id = Interlocked.Increment(ref _nextId);
 var product = new ProductItem { Id = id, Name = name, Price = price, Description = description, IsActive = isActive };
 _products[product.Id] = product;
 return product;
 }

 [McpServerTool, Description("Get a product by id.")]
 public static ProductItem? GetProduct([Description("Product id")] long id)
 {
 return _products.TryGetValue(id, out var p) ? p : null;
 }

 [McpServerTool, Description("List all products.")]
 public static List<ProductItem> ListProducts()
 {
 return _products.Values.OrderByDescending(p => p.Id).ToList();
 }

 [McpServerTool, Description("Update a product.")]
 public static ProductItem? UpdateProduct([Description("Product id")] long id,
 [Description("Product name")] string? name = null,
 [Description("Price")] decimal? price = null,
 [Description("Description")] string? description = null,
 [Description("Is active")] bool? isActive = null)
 {
 if (!_products.TryGetValue(id, out var product)) return null;
 if (name is not null) product.Name = name;
 if (price.HasValue) product.Price = price.Value;
 if (description is not null) product.Description = description;
 if (isActive.HasValue) product.IsActive = isActive.Value;
 return product;
 }

 [McpServerTool, Description("Delete a product by id.")]
 public static bool DeleteProduct([Description("Product id")] long id)
 {
 return _products.TryRemove(id, out _);
 }
}
