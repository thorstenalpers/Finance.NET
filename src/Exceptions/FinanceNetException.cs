using System;
using System.Diagnostics.CodeAnalysis;

namespace Finance.Net.Exceptions;

[ExcludeFromCodeCoverage]
public class FinanceNetException : Exception
{
    public FinanceNetException()
    {
    }

    public FinanceNetException(string message)
        : base(message)
    {
    }

    public FinanceNetException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
