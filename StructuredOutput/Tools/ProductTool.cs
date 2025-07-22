using McpServer.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McpServer.Tools;

[McpServerToolType]
public static class ProductTool
{
    [McpServerTool(UseStructuredContent = true), Description("Gets a list of structured product data with detailed information.")]
    public static List<Product> GetProducts(int count = 5)
    {
        var products = new List<Product>();
        var random = new Random();

        var productNames = new[] { "Laptop Pro", "Wireless Mouse", "Mechanical Keyboard", "USB-C Hub", "Monitor Stand" };
        var categories = new[] { "Electronics", "Accessories", "Peripherals", "Hardware" };
        var brands = new[] { "TechCorp", "GadgetPro", "DeviceMax", "ComponentPlus" };

        for (int i = 0; i < Math.Min(count, 10); i++)
        {
            products.Add(new Product
            {
                Id = i + 1,
                Name = productNames[i % productNames.Length],
                Description = $"High-quality {productNames[i % productNames.Length].ToLower()} for professional use",
                Price = Math.Round(random.NextDouble() * 500 + 50, 2),
                Category = categories[random.Next(categories.Length)],
                Brand = brands[random.Next(brands.Length)],
                InStock = random.Next(0, 100),
                Rating = Math.Round(random.NextDouble() * 2 + 3, 1),
                Features = new List<string>
                {
                    "Durable construction",
                    "Modern design",
                    "Easy to use"
                },
                Specifications = new Dictionary<string, string>
                {
                    { "Weight", $"{random.Next(1, 5)} lbs" },
                    { "Dimensions", $"{random.Next(10, 20)}x{random.Next(8, 15)}x{random.Next(2, 5)} inches" },
                    { "Warranty", "2 years" }
                }
            });
        }

        return products;
    }
}
