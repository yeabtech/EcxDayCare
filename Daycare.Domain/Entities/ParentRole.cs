namespace Daycare.Domain.Entities;

/// <summary>
/// Role a parent/guardian has for a specific child (stored on the ParentChildren join table).
/// </summary>
public enum ParentRole
{
    /// <summary>
    /// Generic guardian when the specific relationship is unknown (also the default for legacy rows).
    /// </summary>
    Guardian = 0,

    /// <summary>
    /// The child's mother.
    /// </summary>
    Mother = 1,

    /// <summary>
    /// The child's father.
    /// </summary>
    Father = 2
}