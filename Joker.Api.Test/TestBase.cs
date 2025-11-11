using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Joker.Api.Test;

public class TestBase<T> where T : class
{
	protected readonly IConfiguration _configuration;
	protected readonly ILogger<T> _logger;
	protected readonly IServiceProvider _serviceProvider;

	public TestBase()
	{
		// Build configuration from user secrets
		_configuration = new ConfigurationBuilder()
			.AddUserSecrets<T>()
			.Build();

		// Setup logging
		var services = new ServiceCollection();
		_ = services.AddLogging(builder =>
			builder.AddConsole()
			.SetMinimumLevel(LogLevel.Debug));

		_serviceProvider = services.BuildServiceProvider();
		_logger = _serviceProvider.GetRequiredService<ILogger<T>>();
	}
}