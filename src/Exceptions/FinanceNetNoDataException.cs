using System;

namespace Finance.Net.Exceptions;

/// <summary>
/// Represents a well-formed provider response that carries no data for the request -
/// an unknown symbol, an empty result set, a report with no rows.
/// </summary>
/// <remarks>
/// This is a permanent answer, not a transient transport failure: the retry policy does
/// not retry it. It derives from <see cref="FinanceNetException"/>, so existing handlers
/// that catch <see cref="FinanceNetException"/> keep working unchanged.
/// </remarks>
public class FinanceNetNoDataException : FinanceNetException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FinanceNetNoDataException"/> class.
    /// </summary>
    public FinanceNetNoDataException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FinanceNetNoDataException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public FinanceNetNoDataException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FinanceNetNoDataException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public FinanceNetNoDataException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
