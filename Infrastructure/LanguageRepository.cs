using Core.Languages.Contracts;
using Core.Languages.Models;
using Dapper;
using Infrastructure.Persistence.Contracts;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

public class LanguageRepository : ILanguageRepository
{
	private readonly IDbConnectionFactory _connectionFactory;
	private readonly ILogger<LanguageRepository> _logger;

	public LanguageRepository(IDbConnectionFactory connectionFactory, ILogger<LanguageRepository> logger)
	{
		_connectionFactory = connectionFactory;
		_logger = logger;
	}

	public async Task<List<LanguageSupport>> GetLanguagesAsync()
	{
		var query = "SELECT language_id AS id, language FROM language_support";
		using var con = await _connectionFactory.CreateConnectionAsync();
		var result = await con.QueryAsync<LanguageSupport>(query);
		return result.ToList();
	}
}