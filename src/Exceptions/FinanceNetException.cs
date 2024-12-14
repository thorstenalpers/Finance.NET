using System;

namespace Finance.Net.Exceptions;

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
