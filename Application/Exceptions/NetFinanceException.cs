using System;

namespace NetFinance.Application.Exceptions;

internal class NetFinanceException : Exception
{
	public NetFinanceException()
	{
	}

	public NetFinanceException(string message)
		: base(message)
	{
	}

	public NetFinanceException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
