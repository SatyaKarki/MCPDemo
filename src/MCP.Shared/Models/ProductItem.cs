using System;

namespace MCP.Shared.Models;

public class ProductItem
{
 public long Id { get; set; }
 public string Name { get; set; } = string.Empty;
 public decimal Price { get; set; }
 public string? Description { get; set; }
 public bool IsActive { get; set; }
}
