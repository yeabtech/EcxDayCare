using Daycare.Domain.Entities;
using Daycare.Domain.ValueObjects;
using Daycare.Domain.Results;

namespace Daycare.Domain.Tests.Features.Parents;

/// <summary>
/// Unit tests for Parent entity
/// </summary>
public class ParentAggregateTests
{
    [Fact]
    public void Create_WithValidData_ReturnsParent()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john@example.com";
        var phone = "555-1234-5678";
        var address = "123 Main St";

        // Act
        var result = Parent.Create(firstName, lastName, email, phone, address);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(firstName, result.Value.Name.FirstName);
        Assert.Equal(lastName, result.Value.Name.LastName);
        Assert.Equal(email, result.Value.ContactInfo.Email);
        Assert.Equal(ParentStatus.Active, result.Value.Status);
    }

    [Fact]
    public void Create_WithInvalidEmail_ReturnsFailure()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "invalid-email";
        var phone = "555-1234-5678";
        var address = "123 Main St";

        // Act
        var result = Parent.Create(firstName, lastName, email, phone, address);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void Create_WithEmptyFirstName_ReturnsFailure()
    {
        // Arrange
        var firstName = "";
        var lastName = "Doe";
        var email = "john@example.com";
        var phone = "555-1234-5678";
        var address = "123 Main St";

        // Act
        var result = Parent.Create(firstName, lastName, email, phone, address);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_WithEmptyAddress_ReturnsFailure()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john@example.com";
        var phone = "555-1234-5678";
        var address = "";

        // Act
        var result = Parent.Create(firstName, lastName, email, phone, address);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Update_WithValidData_UpdatesParent()
    {
        // Arrange
        var result = Parent.Create("John", "Doe", "john@example.com", "555-1234-5678", "123 Main St");
        var parent = result.Value;

        // Act
        var updateResult = parent.Update("Jane", "Smith", "jane@example.com", "555-8765-4321", "456 Oak Ave", ParentStatus.Active);

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.Equal("Jane", parent.Name.FirstName);
        Assert.Equal("Smith", parent.Name.LastName);
        Assert.Equal("jane@example.com", parent.ContactInfo.Email);
    }

    [Fact]
    public void AddChild_WithValidChildId_AddsChild()
    {
        // Arrange
        var result = Parent.Create("John", "Doe", "john@example.com", "555-1234-5678", "123 Main St");
        var parent = result.Value;
        var childId = ChildId.Create();

        // Act
        var addResult = parent.AddChild(childId);

        // Assert
        Assert.True(addResult.IsSuccess);
        Assert.Contains(parent.ParentChildren, pc => pc.ChildId == childId);
    }

    [Fact]
    public void AddChild_WithDuplicateChild_ReturnsFailure()
    {
        // Arrange
        var result = Parent.Create("John", "Doe", "john@example.com", "555-1234-5678", "123 Main St");
        var parent = result.Value;
        var childId = ChildId.Create();
        parent.AddChild(childId);

        // Act
        var addResult = parent.AddChild(childId);

        // Assert
        Assert.False(addResult.IsSuccess);
    }

    [Fact]
    public void RemoveChild_WithExistingChild_RemovesChild()
    {
        // Arrange
        var result = Parent.Create("John", "Doe", "john@example.com", "555-1234-5678", "123 Main St");
        var parent = result.Value;
        var childId = ChildId.Create();
        parent.AddChild(childId);

        // Act
        var removeResult = parent.RemoveChild(childId);

        // Assert
        Assert.True(removeResult.IsSuccess);
        Assert.DoesNotContain(parent.ParentChildren, pc => pc.ChildId == childId);
    }

    [Fact]
    public void RemoveChild_WithNonExistentChild_ReturnsFailure()
    {
        // Arrange
        var result = Parent.Create("John", "Doe", "john@example.com", "555-1234-5678", "123 Main St");
        var parent = result.Value;
        var childId = ChildId.Create();

        // Act
        var removeResult = parent.RemoveChild(childId);

        // Assert
        Assert.False(removeResult.IsSuccess);
    }

    [Fact]
    public void Deactivate_SetsStatusToInactive()
    {
        // Arrange
        var result = Parent.Create("John", "Doe", "john@example.com", "555-1234-5678", "123 Main St");
        var parent = result.Value;

        // Act
        parent.Deactivate();

        // Assert
        Assert.Equal(ParentStatus.Inactive, parent.Status);
    }

    [Fact]
    public void Activate_SetsStatusToActive()
    {
        // Arrange
        var result = Parent.Create("John", "Doe", "john@example.com", "555-1234-5678", "123 Main St", ParentStatus.Inactive);
        var parent = result.Value;

        // Act
        parent.Activate();

        // Assert
        Assert.Equal(ParentStatus.Active, parent.Status);
    }

    [Fact]
    public void ParentId_Create_GeneratesUniqueIds()
    {
        // Act
        var id1 = ParentId.Create();
        var id2 = ParentId.Create();

        // Assert
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void ParentId_CreateFromGuid_CreatesCorrectly()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id = ParentId.Create(guid);

        // Assert
        Assert.Equal(guid, id.Value);
    }
}

/// <summary>
/// Unit tests for ContactInfo value object
/// </summary>
public class ContactInfoTests
{
    [Fact]
    public void Create_WithValidData_ReturnsContactInfo()
    {
        // Arrange
        var email = "john@example.com";
        var phone = "555-1234-5678";

        // Act
        var result = ContactInfo.Create(email, phone);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(email, result.Value.Email);
        Assert.Equal(phone, result.Value.PhoneNumber);
    }

    [Fact]
    public void Create_WithInvalidEmail_ReturnsFailure()
    {
        // Arrange
        var email = "invalid-email";
        var phone = "555-1234-5678";

        // Act
        var result = ContactInfo.Create(email, phone);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_WithInvalidPhone_ReturnsFailure()
    {
        // Arrange
        var email = "john@example.com";
        var phone = "123";

        // Act
        var result = ContactInfo.Create(email, phone);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var email = "john@example.com";
        var phone = "555-1234-5678";
        var contact1 = ContactInfo.Create(email, phone).Value;
        var contact2 = ContactInfo.Create(email, phone).Value;

        // Act
        var equals = contact1.Equals(contact2);

        // Assert
        Assert.True(equals);
    }

    [Fact]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var contact1 = ContactInfo.Create("john@example.com", "555-1234-5678").Value;
        var contact2 = ContactInfo.Create("jane@example.com", "555-8765-4321").Value;

        // Act
        var equals = contact1.Equals(contact2);

        // Assert
        Assert.False(equals);
    }
}
