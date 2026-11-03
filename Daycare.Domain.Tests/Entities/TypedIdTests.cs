using Daycare.Domain.Entities;
using FluentAssertions;

namespace Daycare.Domain.Tests.Entities;

/// <summary>
/// Tests for TypedId<T> strongly-typed identifier pattern.
/// </summary>
public class TypedIdTests
{
    // Concrete implementations for testing
    public sealed class TestId : TypedId<TestId>
    {
        // Parameterless constructor needed for Activator.CreateInstance in TypedId<T>.From()
        public TestId() { }
        public TestId(Guid value) : base(value) { }
    }

    public sealed class AnotherId : TypedId<AnotherId>
    {
        public AnotherId() { }
        public AnotherId(Guid value) : base(value) { }
    }

    [Fact]
    public void Constructor_WithValidGuid_CreatesInstance()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id = TestId.From(guid);

        // Assert
        id.Value.Should().Be(guid);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ThrowsException()
    {
        // Arrange & Act & Assert
        var act = () => TestId.From(Guid.Empty);
        // Activator.CreateInstance wraps the exception, so we catch the outer exception
        act.Should().Throw<Exception>()
            .And.InnerException.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = TestId.From(guid);
        var id2 = TestId.From(guid);

        // Act & Assert
        id1.Equals(id2).Should().BeTrue();
        id1.Should().Be(id2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ReturnsFalse()
    {
        // Arrange
        var id1 = TestId.From(Guid.NewGuid());
        var id2 = TestId.From(Guid.NewGuid());

        // Act & Assert
        id1.Equals(id2).Should().BeFalse();
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var testId = TestId.From(guid);
        var anotherId = AnotherId.From(guid);

        // Act & Assert
        testId.Equals((object)anotherId).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var id = TestId.From(Guid.NewGuid());

        // Act & Assert
        id.Equals(null).Should().BeFalse();
        (id == null).Should().BeFalse();
        (id != null).Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithSameValue_ReturnsTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = TestId.From(guid);
        var id2 = TestId.From(guid);

        // Act & Assert
        (id1 == id2).Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_WithDifferentValue_ReturnsTrue()
    {
        // Arrange
        var id1 = TestId.From(Guid.NewGuid());
        var id2 = TestId.From(Guid.NewGuid());

        // Act & Assert
        (id1 != id2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ReturnsSameHashCode()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = TestId.From(guid);
        var id2 = TestId.From(guid);

        // Act & Assert
        id1.GetHashCode().Should().Be(id2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id = TestId.From(guid);

        // Act & Assert
        id.ToString().Should().Be(guid.ToString());
    }

    [Fact]
    public void TryParse_WithValidGuid_ReturnsSuccess()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var guidString = guid.ToString();

        // Act
        var result = TestId.TryParse(guidString, out var id);

        // Assert
        result.Should().BeTrue();
        id.Should().NotBeNull();
        id!.Value.Should().Be(guid);
    }

    [Fact]
    public void TryParse_WithInvalidFormat_ReturnsFalse()
    {
        // Arrange
        var invalidString = "not-a-guid";

        // Act
        var result = TestId.TryParse(invalidString, out var id);

        // Assert
        result.Should().BeFalse();
        id.Should().BeNull();
    }

    [Fact]
    public void TryParse_WithEmptyGuid_ReturnsFalse()
    {
        // Arrange
        var emptyGuidString = Guid.Empty.ToString();

        // Act
        var result = TestId.TryParse(emptyGuidString, out var id);

        // Assert
        result.Should().BeFalse();
        id.Should().BeNull();
    }

    [Fact]
    public void TryParse_WithNullString_ReturnsFalse()
    {
        // Act
        var result = TestId.TryParse(null, out var id);

        // Assert
        result.Should().BeFalse();
        id.Should().BeNull();
    }

    [Fact]
    public void CanBeUsedInCollections_AsDictionaryKey()
    {
        // Arrange
        var id1 = TestId.From(Guid.NewGuid());
        var id2 = TestId.From(Guid.NewGuid());
        var dictionary = new Dictionary<TestId, string>();

        // Act
        dictionary[id1] = "value1";
        dictionary[id2] = "value2";

        // Assert
        dictionary.Should().HaveCount(2);
        dictionary[id1].Should().Be("value1");
        dictionary[id2].Should().Be("value2");
    }

    [Fact]
    public void CanBeUsedInCollections_InHashSet()
    {
        // Arrange
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        var id1a = TestId.From(guid1);
        var id1b = TestId.From(guid1);
        var id2 = TestId.From(guid2);

        var set = new HashSet<TestId> { id1a, id2 };

        // Act & Assert
        set.Should().HaveCount(2);
        set.Contains(id1b).Should().BeTrue(); // Same value as id1a
        set.Contains(id2).Should().BeTrue();
    }
}
