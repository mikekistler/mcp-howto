using McpServer.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McpServer.Tools;

[McpServerToolType]
public static class UserProfileTool
{
    [McpServerTool(UseStructuredContent = true), Description("Gets a structured user profile with detailed information.")]
    public static UserProfile GetUserProfile(string userId)
    {
        return new UserProfile
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Age = 30,
            Address = new Address
            {
                Street = "123 Main St",
                City = "Anytown",
                State = "CA",
                ZipCode = "12345",
                Country = "USA"
            },
            Preferences = new UserPreferences
            {
                Theme = "dark",
                Language = "en-US",
                Notifications = true,
                EmailUpdates = false
            },
            CreatedAt = DateTime.UtcNow.AddYears(-2),
            LastLoginAt = DateTime.UtcNow.AddHours(-1)
        };
    }
}
