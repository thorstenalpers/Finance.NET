using System.ComponentModel.DataAnnotations;

namespace Finance.Net.Models.DataHub;

/// <summary>
/// Represents a financial instrument listed on the Nasdaq stock exchange.
/// </summary>
public record NasdaqInstrument
{
    /// <summary>
    /// The ticker symbol of the instrument.
    /// </summary>
    [Required] public string? Symbol { get; set; }

    /// <summary>
    /// The company name associated with the instrument.
    /// </summary>
    public string? Name { get; set; }
}
