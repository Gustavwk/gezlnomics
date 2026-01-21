using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Application.Abstractions;

public interface IInfrastructureRegistrar
{
    void Register(IServiceCollection services, IConfiguration configuration);
}
