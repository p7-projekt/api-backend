using Core.Exercises.Contracts;
using Core.Sessions.Contracts;
using Core.Shared;
using Core.Solutions.Contracts;
using Core.Solutions.Models;
using Infrastructure;
using Infrastructure.Authentication;
using Infrastructure.Authentication.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace IntegrationTest.Setup;

public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{

	protected override IHost CreateHost(IHostBuilder builder)
	{
		builder.ConfigureHostConfiguration(config =>
		{
			var testConfig = new Dictionary<string, string>
			{
				// Use the same key as AuthConstants.JwtSecret
				{"ASPNETCORE_ENVIRONMENT", "Development"}
			};
            
			config.AddInMemoryCollection(testConfig);
		});

		builder.ConfigureServices(services =>
		{
			Environment.SetEnvironmentVariable(AuthConstants.JwtSecret, "sdafdafdfasdfasdfasdfasfdasfdafdf"); 
			var iSesSub = Substitute.For<ISessionRepository>();
            var iExeciseSub = Substitute.For<IExerciseRepository>();
			var iHaskellSub = Substitute.For<IHaskellService>();
			var isolutionSub = Substitute.For<ISolutionRepository>();
            services.AddScoped<ISessionRepository>(_ => iSesSub);
			services.AddScoped<IExerciseRepository>(_ => iExeciseSub);
			services.AddScoped<IHaskellService>(_ => iHaskellSub);
			services.AddScoped<ISolutionRepository>(_ => isolutionSub);
		});
		
		return base.CreateHost(builder);
	}
}