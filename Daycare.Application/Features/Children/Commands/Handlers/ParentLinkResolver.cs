using Daycare.Application.DTOs;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Children.Commands.Handlers;

/// <summary>
/// Resolves the requested parent assignments posted with a child command into
/// validated <see cref="ParentChildLink"/> records, ensuring every referenced
/// parent actually exists before any child data is written.
/// </summary>
internal static class ParentLinkResolver
{
    /// <summary>
    /// Validates and resolves the supplied parent assignments.
    /// </summary>
    /// <param name="parentRepository">Repository used to verify parent existence.</param>
    /// <param name="assignments">The requested parents, or <c>null</c> to produce no links.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A successful result containing the resolved links (possibly empty), or a
    /// failure result when a parent cannot be found or an assignment is invalid.
    /// </returns>
    public static async Task<Result<IReadOnlyList<ParentChildLink>>> ResolveAsync(
        IParentRepository parentRepository,
        IReadOnlyList<ParentAssignmentDto>? assignments,
        CancellationToken cancellationToken = default)
    {
        if (parentRepository is null)
            throw new ArgumentNullException(nameof(parentRepository));

        if (assignments is null || assignments.Count == 0)
            return Result<IReadOnlyList<ParentChildLink>>.Success(Array.Empty<ParentChildLink>());

        var links = new List<ParentChildLink>(assignments.Count);
        var seen = new HashSet<Guid>();

        foreach (var assignment in assignments)
        {
            if (assignment.ParentId == Guid.Empty)
                return Result<IReadOnlyList<ParentChildLink>>.Failure(
                    "A parent assignment is missing a valid parent ID",
                    "INVALID_PARENT_ASSIGNMENT");

            if (!seen.Add(assignment.ParentId))
                return Result<IReadOnlyList<ParentChildLink>>.Failure(
                    $"Parent '{assignment.ParentId}' is assigned more than once",
                    "DUPLICATE_PARENT_ASSIGNMENT");

            var parentId = new ParentId(assignment.ParentId);
            var parent = await parentRepository.GetByIdAsync(parentId, cancellationToken);

            if (parent is null)
                return Result<IReadOnlyList<ParentChildLink>>.Failure(
                    $"Parent '{assignment.ParentId}' was not found",
                    "PARENT_NOT_FOUND");

            links.Add(new ParentChildLink(parentId, assignment.Role));
        }

        return Result<IReadOnlyList<ParentChildLink>>.Success(links);
    }
}
