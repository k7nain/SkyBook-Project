using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace FlyzenApi.Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Simplified validation behavior for demo (usually injected Validators and thrown ValidationException)
            return await next();
        }
    }
}