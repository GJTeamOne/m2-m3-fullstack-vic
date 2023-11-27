using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication.Core.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                TResponse response = await next();
                stopwatch.Stop();
                _logger.LogInformation($"Handled {typeof(TRequest).Name} in {stopwatch.ElapsedMilliseconds} ms");
                return response;
            }
            catch (System.Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, $"An error occurred while handling {typeof(TRequest).Name}: {ex.Message}");
                throw;
            }
        }
    }
}
