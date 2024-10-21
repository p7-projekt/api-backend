using Core.Sessions.Contracts;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Core.Sessions;

public class SessionExpirationJob : IJob
{

    private readonly ISessionRepository _sessionRepository;
    private readonly ILogger<SessionExpirationJob> _logger;

    public SessionExpirationJob(ISessionRepository sessionRepository, ILogger<SessionExpirationJob> logger)
    {
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _sessionRepository.DeleteExpiredSessions();
    }
    
    
}