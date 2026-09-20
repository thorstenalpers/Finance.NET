using System;

namespace Finance.Net.Exceptions;

/// <summary>
/// Represents a provider refusing the request for the configured account - an endpoint
/// that needs a premium plan, an invalid or missing API key.
/// </summary>
/// <remarks>
/// This is a permanent answer, not a transient transport failure: the retry policy does
/// not retry it. It derives from <see cref="FinanceNetException"/>, so existing handlers
/// that catch <see cref="FinanceNetException"/> keep working unchanged.
/// </remarks>
public class FinanceNetAccessDeniedException : FinanceNetException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FinanceNetAccessDeniedException"/> class.
    /// </summary>
    public FinanceNetAccessDeniedException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FinanceNetAccessDeniedException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public FinanceNetAccessDeniedException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FinanceNetAccessDeniedException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public FinanceNetAccessDeniedException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
