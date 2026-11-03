using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Daycare.Domain.Repositories;

namespace ECX_Daycare.IntegrationTests.Fixtures;

/// <summary>
/// Custom WebApplicationFactory for integration testing with mocked dependencies.
/// </summary>
public class DaycareWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Mock<IChildRepository> _childRepositoryMock;

    public DaycareWebApplicationFactory()
    {
        _childRepositoryMock = new Mock<IChildRepository>();
    }

    public Mock<IChildRepository> ChildRepositoryMock => _childRepositoryMock;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace the real repository with our mock
            var descriptor = services.FirstOrDefault(
                d => d.ServiceType == typeof(IChildRepository));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddScoped(_ => _childRepositoryMock.Object);
        });
    }
}
