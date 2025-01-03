using System;

namespace Finance.Net.Exceptions;

/// <summary>
/// Represents errors that occur within the FinanceNet system.
/// </summary>
public class FinanceNetException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FinanceNetException"/> class.
    /// </summary>
    public FinanceNetException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FinanceNetException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public FinanceNetException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FinanceNetException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public FinanceNetException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
