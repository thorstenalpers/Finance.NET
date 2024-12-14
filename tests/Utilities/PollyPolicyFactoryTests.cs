using System;
using System.Threading.Tasks;
using Finance.Net.Exceptions;
using Finance.Net.Interfaces;
using Finance.Net.Utilities;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Finance.Net.Tests.Utilities;

[TestFixture]
[Category("Unit")]
public class PollyPolicyFactoryTests
{
		private Mock<ILogger<IAlphaVantageService>> _mockLogger;

		[SetUp]
		public void SetUp()
		{
				_mockLogger = new Mock<ILogger<IAlphaVantageService>>();
		}

		[Test]
		public async Task GetRetryPolicy_ValidFlow_Returns()
		{
				// Arrange
				const int retryCount = 1;
				ILogger logger = null;

				// Act
				var policy = PollyPolicyFactory.GetRetryPolicy(retryCount, logger);

				var result = await policy.ExecuteAsync(() => Task.FromResult(100));

				// Assert
				Assert.That(result, Is.EqualTo(100));
		}

		[Test]
		public void GetRetryPolicy_ExceptionsFlow_Throws()
		{
				// Arrange + Act
				const int retryCount = 3;
				var policy = PollyPolicyFactory.GetRetryPolicy(retryCount, _mockLogger.Object);

				// Assert
				Assert.ThrowsAsync<FinanceNetException>(async () => await policy.ExecuteAsync<int>(() => throw new FinanceNetException("Message")));

				_mockLogger.Verify(
						logger => logger.Log(
								LogLevel.Warning,
								It.IsAny<EventId>(),
								It.Is<It.IsAnyType>((v, _) =>
										v.ToString().Contains("Retry 1 after 00:00:01 due to ")),
								It.IsAny<Exception>(),
								It.IsAny<Func<It.IsAnyType, Exception, string>>()),
						Times.Exactly(1));

				_mockLogger.Verify(
						logger => logger.Log(
								LogLevel.Warning,
								It.IsAny<EventId>(),
								It.Is<It.IsAnyType>((v, _) =>
										v.ToString().Contains("Retry 2 after 00:00:02 due to ")),
								It.IsAny<Exception>(),
								It.IsAny<Func<It.IsAnyType, Exception, string>>()),
						Times.Exactly(1));

				_mockLogger.Verify(
						logger => logger.Log(
						LogLevel.Warning,
						It.IsAny<EventId>(),
						It.Is<It.IsAnyType>((v, _) =>
								v.ToString().Contains("Retry 3 after 00:00:03 due to ")),
						It.IsAny<Exception>(),
						It.IsAny<Func<It.IsAnyType, Exception, string>>()),
						Times.Exactly(1));
		}
}
