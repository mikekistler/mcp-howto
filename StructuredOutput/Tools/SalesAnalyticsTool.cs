using McpServer.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McpServer.Tools;

[McpServerToolType]
public static class SalesAnalyticsTool
{
    [McpServerTool(UseStructuredContent = true), Description("Gets structured sales analytics data with metrics and trends.")]
    public static SalesAnalytics GetSalesAnalytics(string period = "monthly")
    {
        var random = new Random();
        var analytics = new SalesAnalytics
        {
            Period = period,
            TotalRevenue = Math.Round(random.NextDouble() * 100000 + 50000, 2),
            TotalOrders = random.Next(500, 2000),
            AverageOrderValue = Math.Round(random.NextDouble() * 200 + 50, 2),
            ConversionRate = Math.Round(random.NextDouble() * 0.05 + 0.02, 4),
            TopProducts = new List<TopProduct>
            {
                new TopProduct { Name = "Laptop Pro", Sales = random.Next(50, 200), Revenue = Math.Round(random.NextDouble() * 10000 + 5000, 2) },
                new TopProduct { Name = "Wireless Mouse", Sales = random.Next(100, 300), Revenue = Math.Round(random.NextDouble() * 5000 + 2000, 2) },
                new TopProduct { Name = "Mechanical Keyboard", Sales = random.Next(30, 150), Revenue = Math.Round(random.NextDouble() * 8000 + 3000, 2) }
            },
            RegionalData = new List<RegionalSales>
            {
                new RegionalSales { Region = "North America", Revenue = Math.Round(random.NextDouble() * 40000 + 20000, 2), Orders = random.Next(200, 800) },
                new RegionalSales { Region = "Europe", Revenue = Math.Round(random.NextDouble() * 30000 + 15000, 2), Orders = random.Next(150, 600) },
                new RegionalSales { Region = "Asia Pacific", Revenue = Math.Round(random.NextDouble() * 25000 + 10000, 2), Orders = random.Next(100, 400) }
            },
            GeneratedAt = DateTime.UtcNow
        };

        return analytics;
    }
}
