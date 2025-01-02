namespace Finance.Net.Models.Yahoo;

/// <summary>
/// Represents a profile.
/// </summary>
public record Profile
{
    /// <summary>
    /// The address.
    /// </summary>
    public string? Adress { get; set; }

    /// <summary>
    /// The phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// The website URL.
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// The sector in which the entity operates.
    /// </summary>
    public string? Sector { get; set; }

    /// <summary>
    /// The industry the entity belongs to.
    /// </summary>
    public string? Industry { get; set; }

    /// <summary>
    /// The number of employees.
    /// </summary>
    public long? CntEmployees { get; set; }

    /// <summary>
    /// The description.
    /// </summary>
    public string? Description { get; set; }
}
