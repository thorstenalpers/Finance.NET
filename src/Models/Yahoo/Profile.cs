namespace Finance.Net.Models.Yahoo;

/// <summary>
/// Represents a profile with information about an entity, such as contact details, sector, industry, and governance.
/// </summary>
public record Profile
{
    /// <summary>
    /// Name of asset.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the address of the entity.
    /// </summary>
    public string? Adress { get; set; }

    /// <summary>
    /// Gets or sets the phone number of the entity.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the website URL of the entity.
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Gets or sets the sector in which the entity operates.
    /// </summary>
    public string? Sector { get; set; }

    /// <summary>
    /// Gets or sets the industry the entity belongs to.
    /// </summary>
    public string? Industry { get; set; }

    /// <summary>
    /// Gets or sets the number of employees in the entity.
    /// </summary>
    public long? CntEmployees { get; set; }

    /// <summary>
    /// Gets or sets the corporate governance information of the entity.
    /// </summary>
    public string? CorporateGovernance { get; set; }

    /// <summary>
    /// Gets or sets the description of the entity.
    /// </summary>
    public string? Description { get; set; }
}
