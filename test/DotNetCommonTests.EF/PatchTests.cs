using DotNetCommons.EF;
using FluentAssertions;

namespace DotNetCommonTests.EF;

[TestClass]
public class PatchTests
{
    private Patch _patch = null!;

    [TestInitialize]
    public void Setup()
    {
        _patch = new Patch();
    }

    public class TestDto
    {
        [Patch] public int Id { get; set; }

        [Patch(true)] public string? Name { get; set; }

        [Patch] public DateTime Timestamp { get; set; }

        // Not marked as updateable
        public string? Secret { get; set; }
    }

    [TestMethod]
    public void Update_ShouldApplyChanges_ToMarkedProperties()
    {
        var existing = new TestDto
        {
            Id        = 1,
            Name      = "Alice",
            Timestamp = new DateTime(2024, 1, 1),
            Secret    = "unchanged"
        };

        var incoming = new TestDto
        {
            Id        = 1,
            Name      = "Bob",
            Timestamp = new DateTime(2025, 6, 24),
            Secret    = "changed"
        };

        var result = _patch.UpdateObject(existing, incoming);

        result.Should().BeTrue();
        existing.Name.Should().Be("Bob");
        existing.Timestamp.Should().Be(new DateTime(2025, 6, 24));
        existing.Secret.Should().Be("unchanged"); // Not updateable
    }

    [TestMethod]
    public void Update_ShouldRemoveValue_WhenNullAndFlagIsSet()
    {
        var existing = new TestDto { Id = 2, Name = "Charlie" };
        var incoming = new TestDto { Id = 2, Name = null };

        var result = _patch.UpdateObject(existing, incoming);

        result.Should().BeTrue();
        existing.Name.Should().BeNull();
    }

    [TestMethod]
    public void Patch_ShouldAddNewObjects_WhenEnabled()
    {
        var existing = new List<TestDto>();
        var load = new List<TestDto>
        {
            new() { Id = 1, Name = "Delta", Timestamp = DateTime.UtcNow }
        };

        var changes = _patch.Update(PatchMode.AllowNewObjects, x => x.Id, existing, load);
        changes.Should().Be(1);
        existing.Should().ContainSingle()
            .Which.Name.Should().Be("Delta");
    }

    [TestMethod]
    public void Patch_ShouldRemoveOldObjects_WhenEnabled()
    {
        var existing = new List<TestDto>
        {
            new() { Id = 1, Name = "Echo" }
        };
        var load = new List<TestDto>(); // Empty list = remove all

        var changes = _patch.Update(PatchMode.AllowRemovals, x => x.Id, existing, load);
        changes.Should().Be(1);
        existing.Should().BeEmpty();
    }

    [TestMethod]
    public void Patch_ShouldUpdateExistingObjects_WhenBothListsMatch()
    {
        var existing = new List<TestDto>
        {
            new() { Id = 1, Name = "Frank", Timestamp = new DateTime(2023, 1, 1) }
        };
        var load = new List<TestDto>
        {
            new() { Id = 1, Name = "George", Timestamp = new DateTime(2025, 6, 24) }
        };

        var changes = _patch.Update(PatchMode.AllowAll, x => x.Id, existing, load);

        changes.Should().Be(1);
        existing[0].Name.Should().Be("George");
        existing[0].Timestamp.Should().Be(new DateTime(2025, 6, 24));
    }
}