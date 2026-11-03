using Daycare.Domain.Entities;
using FluentAssertions;

namespace Daycare.Domain.Tests.Entities;

/// <summary>
/// Tests for BaseEntity<TId> pattern.
/// </summary>
public class BaseEntityTests
{
    // Concrete implementations for testing
    public sealed class TestId : TypedId<TestId>
    {
        public TestId() { }
        public TestId(Guid value) : base(value) { }
    }

    public sealed class TestEntity : BaseEntity<TestId>
    {
        public string Name { get; set; } = string.Empty;

        public TestEntity(TestId id, string name = "Test") : base(id)
        {
            Name = name;
        }

        // For ORM deserialization
        public TestEntity() { }

        public void UpdateName(string newName)
        {
            Name = newName;
            UpdateTimestamp();
        }
    }

    [Fact]
    public void Constructor_CreatesEntityWithAuditFields()
    {
        // Arrange
        var id = TestId.From(Guid.NewGuid());
        var beforeCreation = DateTime.UtcNow;

        // Act
        var entity = new TestEntity(id, "Test Entity");

        var afterCreation = DateTime.UtcNow;

        // Assert
        entity.Id.Should().Be(id);
        entity.Name.Should().Be("Test Entity");
        entity.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        entity.CreatedAt.Should().BeOnOrBefore(afterCreation);
        // Allow small time difference due to microsecond precision
        entity.UpdatedAt.Should().BeCloseTo(entity.CreatedAt, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public void Constructor_WithNullId_ThrowsException()
    {
        // Act & Assert
        var act = () => new TestEntity(null!, "Test");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UpdateTimestamp_UpdatesUpdatedAtToCurrentTime()
    {
        // Arrange
        var id = TestId.From(Guid.NewGuid());
        var entity = new TestEntity(id);
        var originalUpdatedAt = entity.UpdatedAt;

        // Wait a bit to ensure timestamp difference
        System.Threading.Thread.Sleep(10);

        var beforeUpdate = DateTime.UtcNow;

        // Act
        entity.UpdateName("Updated Name");

        var afterUpdate = DateTime.UtcNow;

        // Assert
        entity.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        entity.UpdatedAt.Should().BeOnOrAfter(beforeUpdate);
        entity.UpdatedAt.Should().BeOnOrBefore(afterUpdate);
        // Compare without microseconds due to precision loss
        entity.CreatedAt.ToUniversalTime().AddTicks(-(entity.CreatedAt.Ticks % 10000))
            .Should().Be(originalUpdatedAt.ToUniversalTime().AddTicks(-(originalUpdatedAt.Ticks % 10000)));
    }

    [Fact]
    public void Equals_WithSameId_ReturnsTrue()
    {
        // Arrange
        var id = TestId.From(Guid.NewGuid());
        var entity1 = new TestEntity(id, "Entity 1");
        var entity2 = new TestEntity(id, "Entity 2");

        // Act & Assert
        entity1.Equals(entity2).Should().BeTrue();
        (entity1 == entity2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentId_ReturnsFalse()
    {
        // Arrange
        var id1 = TestId.From(Guid.NewGuid());
        var id2 = TestId.From(Guid.NewGuid());
        var entity1 = new TestEntity(id1);
        var entity2 = new TestEntity(id2);

        // Act & Assert
        entity1.Equals(entity2).Should().BeFalse();
        (entity1 == entity2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        // Arrange
        var id = TestId.From(Guid.NewGuid());
        var entity = new TestEntity(id);

        // Act & Assert
        entity.Equals("not an entity").Should().BeFalse();
        entity.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithDifferentId_ReturnsTrue()
    {
        // Arrange
        var id1 = TestId.From(Guid.NewGuid());
        var id2 = TestId.From(Guid.NewGuid());
        var entity1 = new TestEntity(id1);
        var entity2 = new TestEntity(id2);

        // Act & Assert
        (entity1 != entity2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameId_ReturnsSameHashCode()
    {
        // Arrange
        var id = TestId.From(Guid.NewGuid());
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        // Act & Assert
        entity1.GetHashCode().Should().Be(entity2.GetHashCode());
    }

    [Fact]
    public void CanBeUsedInCollections_AsDictionaryKey()
    {
        // Arrange
        var id1 = TestId.From(Guid.NewGuid());
        var id2 = TestId.From(Guid.NewGuid());
        var entity1 = new TestEntity(id1, "Entity 1");
        var entity2 = new TestEntity(id2, "Entity 2");
        var dictionary = new Dictionary<TestEntity, string>();

        // Act
        dictionary[entity1] = "value1";
        dictionary[entity2] = "value2";

        // Assert
        dictionary.Should().HaveCount(2);
        dictionary[entity1].Should().Be("value1");
        dictionary[entity2].Should().Be("value2");
    }

    [Fact]
    public void CanBeUsedInCollections_InHashSet()
    {
        // Arrange
        var id1 = TestId.From(Guid.NewGuid());
        var id2 = TestId.From(Guid.NewGuid());
        var entity1a = new TestEntity(id1);
        var entity1b = new TestEntity(id1); // Same ID
        var entity2 = new TestEntity(id2);

        var set = new HashSet<TestEntity> { entity1a, entity2 };

        // Act & Assert
        set.Should().HaveCount(2);
        set.Contains(entity1b).Should().BeTrue(); // Same ID as entity1a
        set.Contains(entity2).Should().BeTrue();
    }

    [Fact]
    public void AuditFields_AreInUtcTime()
    {
        // Arrange
        var id = TestId.From(Guid.NewGuid());

        // Act
        var entity = new TestEntity(id);

        // Assert
        entity.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
        entity.UpdatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void MultipleUpdates_TrackAllChanges()
    {
        // Arrange
        var id = TestId.From(Guid.NewGuid());
        var entity = new TestEntity(id);
        var originalCreatedAt = entity.CreatedAt;

        var updateTimes = new List<DateTime>();

        // Act & Assert
        for (int i = 0; i < 3; i++)
        {
            System.Threading.Thread.Sleep(10);
            entity.UpdateName($"Update {i}");
            updateTimes.Add(entity.UpdatedAt);

            // Verify CreatedAt never changes
            entity.CreatedAt.Should().Be(originalCreatedAt);
        }

        // Verify all updates have increasing timestamps
        updateTimes.Should().BeInAscendingOrder();
    }
}
