using System;
using Finance.Net.Exceptions;
using NUnit.Framework;

namespace Finance.Net.Tests.Exceptions;

[TestFixture]
[Category("Unit")]
public class FinanceNetExceptionTests
{
		[Test]
		public void FinanceNetException_Create_NotNull()
		{
				// Arrange
				// Act
				var financeNetException = new FinanceNetException();

				// Assert
				Assert.That(financeNetException, Is.Not.Null);
		}

		[Test]
		public void FinanceNetException_WithInnerException_NotNull()
		{
				// Arrange
				var exception = new Exception("Test");
				// Act
				var financeNetException = new FinanceNetException("message", exception);

				// Assert
				Assert.That(financeNetException, Is.Not.Null);
				Assert.That(financeNetException.InnerException.Message, Is.EqualTo(exception.Message));
		}
}
