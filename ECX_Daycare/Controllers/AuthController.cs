using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Daycare.Application.Common.Interfaces;
using Daycare.Application.DTOs;
using Daycare.Infrastructure.Data;
using Daycare.Infrastructure.Identity;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Application.Features.Staff.Commands;
using Daycare.Application.Features.Staff.Queries;
using Daycare.Application.Features.Parents.Commands;
using Daycare.Application.Features.Parents.Queries;
using ECX_Daycare.Common;

namespace ECX_Daycare.Controllers;

/// <summary>
/// Controller handling authentication, registration, token refresh, and user identity.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly DaycareDbContext _dbContext;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly IValidator<RegisterRequestDto> _registerValidator;
    private readonly IValidator<RefreshTokenRequestDto> _refreshValidator;
    private readonly ILogger<AuthController> _logger;
    private readonly IStaffRepository _staffRepository;
    private readonly IMessageBus _messageBus;
    private readonly IParentRepository _parentRepository;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IJwtTokenService jwtTokenService,
        DaycareDbContext dbContext,
        IValidator<LoginRequestDto> loginValidator,
        IValidator<RegisterRequestDto> registerValidator,
        IValidator<RefreshTokenRequestDto> refreshValidator,
        ILogger<AuthController> logger,
        IStaffRepository staffRepository,
        IMessageBus messageBus,
        IParentRepository parentRepository)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
        _dbContext = dbContext;
        _loginValidator = loginValidator;
        _registerValidator = registerValidator;
        _refreshValidator = refreshValidator;
        _logger = logger;
        _staffRepository = staffRepository;
        _messageBus = messageBus;
        _parentRepository = parentRepository;
    }

    /// <summary>
    /// Authenticates a user and generates JWT access and refresh tokens.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            _logger.LogWarning("Failed login attempt for email {Email}", request.Email);
            return Unauthorized(new { message = "Invalid email or password." });
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Inactive user {Email} attempted login", request.Email);
            return Unauthorized(new { message = "Your account is inactive. Please contact an administrator." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? user.Role ?? "Parent";

        var accessToken = _jwtTokenService.GenerateAccessToken(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            primaryRole,
            user.StaffId,
            user.ParentId);

        var refreshTokenString = _jwtTokenService.GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = RefreshTokenHasher.Hash(refreshTokenString),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.RefreshTokens.Add(refreshTokenEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new AuthResponseDto(
            AccessToken: accessToken,
            RefreshToken: refreshTokenString,
            ExpiresIn: 3600,
            User: new CurrentUserDto(
                Id: user.Id,
                Email: user.Email!,
                FirstName: user.FirstName,
                LastName: user.LastName,
                Role: primaryRole,
                StaffId: user.StaffId,
                ParentId: user.ParentId
            )
        );

        return Ok(response);
    }

    /// <summary>
    /// Registers a new user account with role-based assignment.
    /// </summary>
    [Authorize(Policy = "RequireAdmin")]
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Conflict(new { message = "A user with this email address already exists." });
        }

        var targetRole = string.IsNullOrWhiteSpace(request.Role) ? "Parent" : request.Role;
        if (!await _roleManager.RoleExistsAsync(targetRole))
        {
            await _roleManager.CreateAsync(new IdentityRole<Guid>(targetRole));
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = targetRole,
            StaffId = request.StaffId,
            ParentId = request.ParentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        await _userManager.AddToRoleAsync(user, targetRole);

        var accessToken = _jwtTokenService.GenerateAccessToken(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            targetRole,
            user.StaffId,
            user.ParentId);

        var refreshTokenString = _jwtTokenService.GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = RefreshTokenHasher.Hash(refreshTokenString),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.RefreshTokens.Add(refreshTokenEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new AuthResponseDto(
            AccessToken: accessToken,
            RefreshToken: refreshTokenString,
            ExpiresIn: 3600,
            User: new CurrentUserDto(
                Id: user.Id,
                Email: user.Email,
                FirstName: user.FirstName,
                LastName: user.LastName,
                Role: targetRole,
                StaffId: user.StaffId,
                ParentId: user.ParentId
            )
        );

        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>Lists registered accounts for the admin access management page.</summary>
    [Authorize(Policy = "RequireAdmin")]
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .OrderByDescending(user => user.CreatedAt)
            .ToListAsync(cancellationToken);

        var rows = new List<ManagedUserResponse>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            rows.Add(new ManagedUserResponse(
                user.Id,
                user.Email ?? user.UserName ?? string.Empty,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                roles.FirstOrDefault() ?? user.Role,
                user.IsActive,
                user.EmailConfirmed,
                user.StaffId,
                user.ParentId,
                user.CreatedAt));
        }

        return Ok(ECX_Daycare.Common.ApiResponse<IReadOnlyList<ManagedUserResponse>>.Success(rows));
    }

    /// <summary>Creates a login account linked to an existing staff or parent record.</summary>
    [Authorize(Policy = "RequireAdmin")]
    [HttpPost("users")]
    public async Task<IActionResult> CreateManagedUser(
        [FromBody] CreateManagedUserRequest request,
        CancellationToken cancellationToken)
    {
        var role = request.Role?.Trim();
        if (role is not ("Staff" or "Parent"))
            return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail("Choose either a Staff or Parent account."));
        if (request.ProfileId == Guid.Empty)
            return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail("Choose the staff or parent profile to link."));
        if (string.IsNullOrWhiteSpace(request.Email) || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(request.Email))
            return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail("Enter a valid login email address."));
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail("The password must be at least 6 characters."));
        if (await _userManager.FindByEmailAsync(request.Email.Trim()) is not null)
            return Conflict(ECX_Daycare.Common.ApiResponse<object>.Fail("An account already uses this email address."));

        string firstName;
        string lastName;
        string? phone;
        Guid? staffId = null;
        Guid? parentId = null;

        if (role == "Staff")
        {
            var staff = await _staffRepository.GetByIdAsync(StaffId.Create(request.ProfileId), cancellationToken);
            if (staff is null)
                return NotFound(ECX_Daycare.Common.ApiResponse<object>.Fail("The selected staff profile was not found."));
            if (await _userManager.Users.AnyAsync(user => user.StaffId == request.ProfileId, cancellationToken))
                return Conflict(ECX_Daycare.Common.ApiResponse<object>.Fail("That staff profile already has a login account."));

            firstName = staff.FirstName;
            lastName = staff.LastName;
            phone = staff.Phone;
            staffId = request.ProfileId;
        }
        else
        {
            var selectedParentId = ParentId.Create(request.ProfileId);
            var parent = await _dbContext.Parents.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == selectedParentId, cancellationToken);
            if (parent is null)
                return NotFound(ECX_Daycare.Common.ApiResponse<object>.Fail("The selected parent profile was not found."));
            if (await _userManager.Users.AnyAsync(user => user.ParentId == request.ProfileId, cancellationToken))
                return Conflict(ECX_Daycare.Common.ApiResponse<object>.Fail("That parent profile already has a login account."));

            firstName = parent.Name.FirstName;
            lastName = parent.Name.LastName;
            phone = parent.ContactInfo.PhoneNumber;
            parentId = request.ProfileId;
        }

        if (!await _roleManager.RoleExistsAsync(role))
            await _roleManager.CreateAsync(new IdentityRole<Guid>(role));

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phone,
            Role = role,
            StaffId = staffId,
            ParentId = parentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail(string.Join(" ", createResult.Errors.Select(error => error.Description))));

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail(string.Join(" ", roleResult.Errors.Select(error => error.Description))));
        }

        return StatusCode(StatusCodes.Status201Created,
            ECX_Daycare.Common.ApiResponse<ManagedUserResponse>.Success(
                new ManagedUserResponse(user.Id, user.Email!, user.FirstName, user.LastName, user.PhoneNumber,
                    role, user.IsActive, user.EmailConfirmed, user.StaffId, user.ParentId, user.CreatedAt),
                "Login account created and linked."));
    }

    /// <summary>Enables or disables an account without deleting its history.</summary>
    [Authorize(Policy = "RequireAdmin")]
    [HttpPatch("users/{id:guid}/status")]
    public async Task<IActionResult> SetManagedUserStatus(
        Guid id,
        [FromBody] SetManagedUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        var callerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (Guid.TryParse(callerId, out var currentUserId) && currentUserId == id && !request.IsActive)
            return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail("You cannot disable your own account."));

        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound(ECX_Daycare.Common.ApiResponse<object>.Fail("Account not found."));

        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail(string.Join(" ", result.Errors.Select(error => error.Description))));

        return Ok(ECX_Daycare.Common.ApiResponse<object>.Success(true,
            request.IsActive ? "Account enabled." : "Account disabled."));
    }

    /// <summary>
    /// Refreshes an expired or active JWT access token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _refreshValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        var tokenRecord = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(
                rt => rt.Token == RefreshTokenHasher.Hash(request.RefreshToken),
                cancellationToken);

        if (tokenRecord == null || tokenRecord.IsRevoked || tokenRecord.IsExpired)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token." });
        }

        var user = tokenRecord.User;
        if (user == null || !user.IsActive)
        {
            return Unauthorized(new { message = "User account not found or inactive." });
        }

        // Revoke current refresh token and issue a replacement (Rotation strategy)
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        tokenRecord.RevokedAt = DateTime.UtcNow;
        tokenRecord.ReplacedByToken = RefreshTokenHasher.Hash(newRefreshToken);

        var newRefreshTokenRecord = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = RefreshTokenHasher.Hash(newRefreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.RefreshTokens.Add(newRefreshTokenRecord);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? user.Role ?? "Parent";

        var newAccessToken = _jwtTokenService.GenerateAccessToken(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            primaryRole,
            user.StaffId,
            user.ParentId);

        var response = new AuthResponseDto(
            AccessToken: newAccessToken,
            RefreshToken: newRefreshToken,
            ExpiresIn: 3600,
            User: new CurrentUserDto(
                Id: user.Id,
                Email: user.Email!,
                FirstName: user.FirstName,
                LastName: user.LastName,
                Role: primaryRole,
                StaffId: user.StaffId,
                ParentId: user.ParentId
            )
        );

        return Ok(response);
    }

    /// <summary>
    /// Revokes a refresh token, logging out the user from that session.
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] RevokeTokenRequestDto? request, CancellationToken cancellationToken)
    {
        if (request != null && !string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            var tokenRecord = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(
                    rt => rt.Token == RefreshTokenHasher.Hash(request.RefreshToken),
                    cancellationToken);

            if (tokenRecord != null && !tokenRecord.IsRevoked)
            {
                tokenRecord.RevokedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
        else if (User.Identity?.IsAuthenticated == true)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (Guid.TryParse(userIdStr, out var userId))
            {
                var activeTokens = await _dbContext.RefreshTokens
                    .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                    .ToListAsync(cancellationToken);

                foreach (var token in activeTokens)
                {
                    token.RevokedAt = DateTime.UtcNow;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        return Ok(new { message = "Logged out successfully." });
    }

    /// <summary>
    /// Returns the staff profile linked to the authenticated staff login.
    /// </summary>
    [Authorize(Roles = "Staff")]
    [HttpGet("me/staff-profile")]
    public async Task<IActionResult> GetMyStaffProfile(CancellationToken cancellationToken)
    {
        var account = await GetCurrentAccountAsync();
        if (account is null)
            return Unauthorized(ECX_Daycare.Common.ApiResponse<object>.Fail("The signed-in account could not be found."));
        if (!account.StaffId.HasValue)
            return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail("This login is not linked to a staff profile."));

        var result = await _messageBus.SendAsync(new GetStaffByIdQuery(account.StaffId.Value), cancellationToken);
        if (result.IsFailure)
            return NotFound(ECX_Daycare.Common.ApiResponse<object>.Fail(result.Error));

        return Ok(ECX_Daycare.Common.ApiResponse<StaffDto>.Success(result.Value));
    }

    /// <summary>
    /// Updates basic details for the authenticated staff member only.
    /// </summary>
    [Authorize(Roles = "Staff")]
    [HttpPut("me/staff-profile")]
    public async Task<IActionResult> UpdateMyStaffProfile(
        [FromBody] UpdateMyStaffProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName)
            || string.IsNullOrWhiteSpace(request.LastName)
            || string.IsNullOrWhiteSpace(request.Phone)
            || string.IsNullOrWhiteSpace(request.Email)
            || !new EmailAddressAttribute().IsValid(request.Email))
            return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail("Enter a valid email, first name, last name, and phone."));

        var account = await GetCurrentAccountAsync();
        if (account is null)
            return Unauthorized(ECX_Daycare.Common.ApiResponse<object>.Fail("The signed-in account could not be found."));
        if (!account.StaffId.HasValue)
            return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail("This login is not linked to a staff profile."));

        var current = await _messageBus.SendAsync(new GetStaffByIdQuery(account.StaffId.Value), cancellationToken);
        if (current.IsFailure)
            return NotFound(ECX_Daycare.Common.ApiResponse<object>.Fail(current.Error));

        var profile = current.Value!;
        var staffId = StaffId.Create(account.StaffId.Value);
        if (await _staffRepository.ExistsByEmailAsync(request.Email.Trim(), staffId, cancellationToken))
            return Conflict(ECX_Daycare.Common.ApiResponse<object>.Fail("Another staff profile already uses that email address."));

        var normalizedEmail = _userManager.NormalizeEmail(request.Email.Trim());
        var emailInUse = await _userManager.Users.AnyAsync(user =>
            user.Id != account.Id && user.NormalizedEmail == normalizedEmail, cancellationToken);
        if (emailInUse)
            return Conflict(ECX_Daycare.Common.ApiResponse<object>.Fail("Another account already uses that email address."));

        var previous = (account.FirstName, account.LastName, account.PhoneNumber, account.Email, account.UserName);
        var setUsername = await _userManager.SetUserNameAsync(account, request.Email.Trim());
        if (!setUsername.Succeeded)
            return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail(
                string.Join(" ", setUsername.Errors.Select(error => error.Description))));

        var setEmail = await _userManager.SetEmailAsync(account, request.Email.Trim());
        if (!setEmail.Succeeded)
        {
            await _userManager.SetUserNameAsync(account, previous.UserName);
            return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail(
                string.Join(" ", setEmail.Errors.Select(error => error.Description))));
        }

        account.FirstName = request.FirstName.Trim();
        account.LastName = request.LastName.Trim();
        account.PhoneNumber = request.Phone.Trim();
        account.UpdatedAt = DateTime.UtcNow;

        var identityResult = await _userManager.UpdateAsync(account);
        if (!identityResult.Succeeded)
        {
            await _userManager.SetEmailAsync(account, previous.Email);
            await _userManager.SetUserNameAsync(account, previous.UserName);
            account.FirstName = previous.FirstName;
            account.LastName = previous.LastName;
            account.PhoneNumber = previous.PhoneNumber;
            await _userManager.UpdateAsync(account);
            return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail(
                string.Join(" ", identityResult.Errors.Select(error => error.Description))));
        }

        var updateResult = await _messageBus.SendAsync(new UpdateStaffCommand(
            profile.Id,
            request.FirstName,
            request.LastName,
            profile.Role,
            request.Phone,
            request.Email.Trim(),
            profile.HireDate,
            profile.ShiftStart,
            profile.ShiftEnd,
            profile.ClassroomId,
            request.Certifications), cancellationToken);

        if (updateResult.IsFailure)
        {
            await _userManager.SetEmailAsync(account, previous.Email);
            await _userManager.SetUserNameAsync(account, previous.UserName);
            account.FirstName = previous.FirstName;
            account.LastName = previous.LastName;
            account.PhoneNumber = previous.PhoneNumber;
            account.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(account);
            return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail(updateResult.Error));
        }

        var refreshed = await _messageBus.SendAsync(new GetStaffByIdQuery(profile.Id), cancellationToken);
        return Ok(ECX_Daycare.Common.ApiResponse<StaffDto>.Success(refreshed.Value!, "Staff profile updated."));
    }

    /// <summary>Returns the parent profile linked to the authenticated parent account.</summary>
    [Authorize(Roles = "Parent")]
    [HttpGet("me/parent-profile")]
    public async Task<IActionResult> GetMyParentProfile(CancellationToken cancellationToken)
    {
        var account = await GetCurrentAccountAsync();
        if (account is null) return Unauthorized(ECX_Daycare.Common.ApiResponse<object>.Fail("The signed-in account could not be found."));
        if (!account.ParentId.HasValue) return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail("This login is not linked to a parent record."));

        var result = await _messageBus.SendAsync(new GetParentQuery(account.ParentId.Value), cancellationToken);
        if (result.IsFailure) return NotFound(ECX_Daycare.Common.ApiResponse<object>.Fail(result.Error));
        return Ok(ECX_Daycare.Common.ApiResponse<ParentDto>.Success(result.Value!));
    }

    /// <summary>Updates contact details on the signed-in parent's own profile and login.</summary>
    [Authorize(Roles = "Parent")]
    [HttpPut("me/parent-profile")]
    public async Task<IActionResult> UpdateMyParentProfile(
        [FromBody] UpdateMyParentProfileRequest request,
        CancellationToken cancellationToken)
    {
        var firstName = request.FirstName?.Trim();
        var lastName = request.LastName?.Trim();
        var email = request.Email?.Trim();
        var phoneNumber = request.PhoneNumber?.Trim();
        var address = request.Address?.Trim();
        if (string.IsNullOrWhiteSpace(firstName)
            || firstName.Length > 100
            || string.IsNullOrWhiteSpace(lastName)
            || lastName.Length > 100
            || string.IsNullOrWhiteSpace(phoneNumber)
            || string.IsNullOrWhiteSpace(email)
            || !new EmailAddressAttribute().IsValid(email)
            || string.IsNullOrWhiteSpace(address))
            return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail("Enter your name, a valid email, phone number, and address."));

        var account = await GetCurrentAccountAsync();
        if (account is null) return Unauthorized(ECX_Daycare.Common.ApiResponse<object>.Fail("The signed-in account could not be found."));
        if (!account.ParentId.HasValue) return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail("This login is not linked to a parent record."));

        var parentId = ParentId.Create(account.ParentId.Value);
        var parent = await _parentRepository.GetByIdAsync(parentId, cancellationToken);
        if (parent is null) return NotFound(ECX_Daycare.Common.ApiResponse<object>.Fail("Parent not found."));

        if (await _parentRepository.ExistsByEmailAsync(email, parentId, cancellationToken))
            return Conflict(ECX_Daycare.Common.ApiResponse<object>.Fail("Another parent profile already uses that email address."));

        var normalizedEmail = _userManager.NormalizeEmail(email);
        if (await _userManager.Users.AnyAsync(user => user.Id != account.Id && user.NormalizedEmail == normalizedEmail, cancellationToken))
            return Conflict(ECX_Daycare.Common.ApiResponse<object>.Fail("Another account already uses that email address."));

        var updateResult = parent.Update(
            firstName,
            lastName,
            email,
            phoneNumber,
            address,
            parent.Status,
            parent.Notes);
        if (updateResult.IsFailure)
            return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail(updateResult.Error));

        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
        return await executionStrategy.ExecuteAsync<IActionResult>(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var usernameResult = await _userManager.SetUserNameAsync(account, email);
            if (!usernameResult.Succeeded)
                return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail(string.Join(" ", usernameResult.Errors.Select(error => error.Description))));

            var emailResult = await _userManager.SetEmailAsync(account, email);
            if (!emailResult.Succeeded)
                return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail(string.Join(" ", emailResult.Errors.Select(error => error.Description))));

            account.FirstName = firstName;
            account.LastName = lastName;
            account.PhoneNumber = phoneNumber;
            account.UpdatedAt = DateTime.UtcNow;
            var identityResult = await _userManager.UpdateAsync(account);
            if (!identityResult.Succeeded)
                return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail(string.Join(" ", identityResult.Errors.Select(error => error.Description))));

            await _parentRepository.UpdateAsync(parent, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var updatedProfile = new ParentDto(
                parent.ParentId.Value,
                parent.Name.FirstName,
                parent.Name.LastName,
                parent.ContactInfo.Email,
                parent.ContactInfo.PhoneNumber,
                parent.Address,
                parent.Status.ToString(),
                parent.Notes,
                parent.CreatedAt,
                parent.UpdatedAt);

            return Ok(ECX_Daycare.Common.ApiResponse<ParentDto>.Success(updatedProfile, "Parent profile updated."));
        });
    }

    /// <summary>
    /// Changes the password for the authenticated staff account after verifying the current password.
    /// </summary>
    [Authorize]
    [HttpPost("me/change-password")]
    public async Task<IActionResult> ChangeMyPassword(
        [FromBody] ChangeMyPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrEmpty(request.NewPassword))
            return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail("Enter your current password and a new password."));
        if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
            return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail("The new password and confirmation do not match."));
        if (request.NewPassword.Length < 8)
            return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail("The new password must be at least 8 characters."));

        var account = await GetCurrentAccountAsync();
        if (account is null)
            return Unauthorized(ECX_Daycare.Common.ApiResponse<object>.Fail("The signed-in account could not be found."));

        var result = await _userManager.ChangePasswordAsync(account, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return BadRequest(ECX_Daycare.Common.ApiResponse<object>.Fail(
                string.Join(" ", result.Errors.Select(error => error.Description))));

        return Ok(ECX_Daycare.Common.ApiResponse<object>.Success(true, "Password changed successfully."));
    }

    private async Task<ApplicationUser?> GetCurrentAccountAsync()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(userId, out var id)
            ? await _userManager.FindByIdAsync(id.ToString())
            : null;
    }

    /// <summary>
    /// Returns the currently authenticated user's profile and permissions.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized(new { message = "Invalid token subject." });
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? user.Role ?? "Parent";

        var result = new CurrentUserDto(
            Id: user.Id,
            Email: user.Email ?? string.Empty,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Role: primaryRole,
            StaffId: user.StaffId,
            ParentId: user.ParentId
        );

        return Ok(result);
    }
}

public sealed record UpdateMyStaffProfileRequest(
    string FirstName,
    string LastName,
    string Phone,
    string Email,
    string? Certifications);

public sealed record ChangeMyPasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword);

public sealed record UpdateMyParentProfileRequest(
    [Required, StringLength(100)] string FirstName,
    [Required, StringLength(100)] string LastName,
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(30)] string PhoneNumber,
    [Required, StringLength(300)] string Address);

public sealed record CreateManagedUserRequest(
    string Email,
    string Password,
    string Role,
    Guid ProfileId);

public sealed record SetManagedUserStatusRequest(bool IsActive);

public sealed record ManagedUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string Role,
    bool IsActive,
    bool EmailConfirmed,
    Guid? StaffId,
    Guid? ParentId,
    DateTime CreatedAt);
