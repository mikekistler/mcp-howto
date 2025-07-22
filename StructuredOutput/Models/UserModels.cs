using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace McpServer.Models;

public class UserProfile
{
    [Key]
    [Required]
    [StringLength(50, MinimumLength = 1)]
    [Description("Unique identifier for the user")]
    public string Id { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 1)]
    [Description("User's first name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 1)]
    [Description("User's last name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    [Description("User's email address")]
    public string Email { get; set; } = string.Empty;

    [Range(13, 120)]
    [Description("User's age in years")]
    public int Age { get; set; }

    [Description("User's physical address information")]
    public Address Address { get; set; } = new Address();

    [Description("User's application preferences and settings")]
    public UserPreferences Preferences { get; set; } = new UserPreferences();

    [Required]
    [Description("Timestamp when the user account was created")]
    public DateTime CreatedAt { get; set; }

    [Description("Timestamp of the user's last login")]
    public DateTime LastLoginAt { get; set; }
}

public class Address
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    [Description("Street address including house number and street name")]
    public string Street { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    [Description("City name")]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 1)]
    [Description("State or province name")]
    public string State { get; set; } = string.Empty;

    [Required]
    [StringLength(20, MinimumLength = 1)]
    [Description("Postal or ZIP code")]
    public string ZipCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    [Description("Country name")]
    public string Country { get; set; } = string.Empty;
}

public class UserPreferences
{
    [StringLength(20)]
    [Description("User interface theme preference (e.g., 'dark', 'light')")]
    public string Theme { get; set; } = string.Empty;

    [StringLength(10)]
    [Description("Preferred language code (e.g., 'en', 'es', 'fr')")]
    public string Language { get; set; } = string.Empty;

    [Description("Whether the user wants to receive notifications")]
    public bool Notifications { get; set; }

    [Description("Whether the user wants to receive email updates")]
    public bool EmailUpdates { get; set; }
}
