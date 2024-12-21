namespace Finance.Net.Models.Yahoo;

/// <summary>
/// Represents a profile with information about an entity, such as contact details, sector, industry, and governance.
/// </summary>
public record Profile
{
    /// <summary>
    /// Name of asset.
    /// </summary>
    /// <value>The name, or <c>null</c> if not provided.</value>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the address of the entity.
    /// </summary>
    /// <value>The address, or <c>null</c> if not available.</value>
    public string? Adress { get; set; }

    /// <summary>
    /// Gets or sets the phone number of the entity.
    /// </summary>
    /// <value>The phone number, or <c>null</c> if not available.</value>
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the website URL of the entity.
    /// </summary>
    /// <value>The website URL, or <c>null</c> if not available.</value>
    public string? Website { get; set; }

    /// <summary>
    /// Gets or sets the sector in which the entity operates.
    /// </summary>
    /// <value>The sector, or <c>null</c> if not specified.</value>
    public string? Sector { get; set; }

    /// <summary>
    /// Gets or sets the industry the entity belongs to.
    /// </summary>
    /// <value>The industry, or <c>null</c> if not specified.</value>
    public string? Industry { get; set; }

    /// <summary>
    /// Gets or sets the number of employees in the entity.
    /// </summary>
    /// <value>The number of employees, or <c>null</c> if not available.</value>
    public long? CntEmployees { get; set; }

    /// <summary>
    /// Gets or sets the corporate governance information of the entity.
    /// </summary>
    /// <value>The corporate governance details, or <c>null</c> if not available.</value>
    public string? CorporateGovernance { get; set; }

    /// <summary>
    /// Gets or sets the description of the entity.
    /// </summary>
    /// <value>The description, or <c>null</c> if not provided.</value>
    public string? Description { get; set; }
}
