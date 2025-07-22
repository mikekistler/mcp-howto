using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace McpServer.Models;

public class Product
{
    [Key]
    [Range(1, int.MaxValue)]
    [Description("Unique identifier for the product")]
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    [Description("Name of the product")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    [Description("Detailed description of the product")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    [DisplayFormat(DataFormatString = "{0:C}")]
    [Description("Price of the product in currency")]
    public double Price { get; set; }

    [Required]
    [StringLength(50)]
    [Description("Product category classification")]
    public string Category { get; set; } = string.Empty;

    [StringLength(50)]
    [Description("Brand or manufacturer of the product")]
    public string Brand { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    [Description("Number of items currently in stock")]
    public int InStock { get; set; }

    [Range(0.0, 5.0)]
    [Description("Customer rating from 0 to 5 stars")]
    public double Rating { get; set; }

    [Description("List of product features and highlights")]
    public List<string> Features { get; set; } = new List<string>();

    [Description("Technical specifications and details")]
    public Dictionary<string, string> Specifications { get; set; } = new Dictionary<string, string>();
}
