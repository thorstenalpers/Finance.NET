using System;
using System.Diagnostics.CodeAnalysis;

namespace DotNetFinance.Exceptions;

[ExcludeFromCodeCoverage]
public class DotNetFinanceException : Exception
{
	public DotNetFinanceException()
	{
	}

	public DotNetFinanceException(string message)
		: base(message)
	{
	}

	public DotNetFinanceException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
