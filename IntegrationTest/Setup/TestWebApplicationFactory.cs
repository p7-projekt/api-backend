using Core.Dashboards.Contracts;
using Core.Classrooms.Contracts;
using Core.Exercises.Contracts;
using Core.Languages.Contracts;
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
			var itokenRepoSub = Substitute.For<ITokenRepository>();
			var iUserRepo = Substitute.For<IUserRepository>();
			var iExeciseSub = Substitute.For<IExerciseRepository>();
			var iMozartSub = Substitute.For<IMozartService>();
			var iSolutionSub = Substitute.For<ISolutionRepository>();
			var iDashboardSub = Substitute.For<IDashboardRepository>();
      var iLanguageSub = Substitute.For<ILanguageRepository>();
			var iClassroomSub = Substitute.For<IClassroomRepository>();
			services.AddScoped<ITokenRepository>(_ => itokenRepoSub);
			services.AddScoped<IUserRepository>(_ => iUserRepo);
			services.AddScoped<ISessionRepository>(_ => iSesSub);
			services.AddScoped<IExerciseRepository>(_ => iExeciseSub);
			services.AddScoped<IMozartService>(_ => iMozartSub);
			services.AddScoped<ISolutionRepository>(_ => iSolutionSub);
			services.AddScoped<ILanguageRepository>(_ => iLanguageSub);
			services.AddScoped<IClassroomRepository>(_ => iClassroomSub);
			services.AddScoped<IDashboardRepository>(_ => iDashboardSub);
		});
		
		return base.CreateHost(builder);
	}
}