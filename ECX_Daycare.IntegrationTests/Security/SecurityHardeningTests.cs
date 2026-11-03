using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Daycare.Infrastructure.Identity;
using Daycare.Controllers;
using ECX_Daycare.Controllers;
using ECX_Daycare.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ECX_Daycare.IntegrationTests.Security;

public sealed class SecurityHardeningTests
{
    [Fact]
    public void StaffController_RequiresAdminPolicy()
    {
        var authorize = typeof(StaffController).GetCustomAttribute<AuthorizeAttribute>();

        authorize.Should().NotBeNull();
        authorize!.Policy.Should().Be("RequireAdmin");
    }

    [Fact]
    public async Task StaffDashboard_ForbidsStaffRequestingAnotherStaffId()
    {
        var mediator = new Mock<MediatR.IMediator>();
        var controller = new Daycare.Controllers.DashboardController(
            mediator.Object,
            NullLogger<Daycare.Controllers.DashboardController>.Instance);
        var ownStaffId = Guid.NewGuid();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.Role, "Staff"), new Claim("staffId", ownStaffId.ToString())],
                    "test"))
            }
        };

        var result = await controller.GetStaffDashboard(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task ExceptionMiddleware_DoesNotReturnExceptionDetails()
    {
        const string privateDetail = "sensitive database detail";
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException(privateDetail),
            NullLogger<ExceptionHandlingMiddleware>.Instance);
        var responseBody = new MemoryStream();
        var context = new DefaultHttpContext();
        context.Response.Body = responseBody;

        await middleware.InvokeAsync(context);

        responseBody.Position = 0;
        using var response = await JsonDocument.ParseAsync(responseBody);
        var body = response.RootElement.GetRawText();

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().StartWith("application/problem+json");
        body.Should().Contain("An unexpected error occurred");
        body.Should().NotContain(privateDetail);
    }

    [Fact]
    public void JwtTokenService_RejectsMissingSigningSecret()
    {
        var configuration = new ConfigurationBuilder().Build();

        var act = () => new JwtTokenService(
            Options.Create(new JwtSettings()),
            configuration);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*JWT_SECRET_KEY*");
    }

    [Fact]
    public void RefreshTokenHasher_StoresAStableDigestInsteadOfTheToken()
    {
        const string token = "a-raw-refresh-token";

        var hash = RefreshTokenHasher.Hash(token);

        hash.Should().HaveLength(64);
        hash.Should().NotBe(token);
        RefreshTokenHasher.Hash(token).Should().Be(hash);
    }
}
