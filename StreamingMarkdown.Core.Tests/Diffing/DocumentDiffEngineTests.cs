using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Core.Parsing;
using StreamingMarkdown.Core.Streaming;

namespace StreamingMarkdown.Core.Tests.Diffing;

public sealed class DocumentDiffEngineTests
{
    private readonly IMarkdownParser _parser =
        new MarkdigMarkdownParser();

    private readonly IDocumentDiffEngine _diffEngine =
        new DocumentDiffEngine();

    [Fact]
    public void Compare_WhenDocumentsAreEqual_ShouldReturnNoChanges()
    {
        // Arrange
        var previous =
            _parser.Parse("# Pool");

        var current =
            _parser.Parse("# Pool");

        // Act
        var diff =
            _diffEngine.Compare(
                previous,
                current);

        // Assert
        Assert.False(diff.HasChanges);
        Assert.Empty(diff.Changes);
    }

    [Fact]
public void Compare_WhenBlockIsAdded_ShouldReportAdded()
{
    // Arrange
    var previous =
        _parser.Parse("""
            # Pool

            Water condition is good.
            """);

    var current =
        _parser.Parse("""
            # Pool

            Water condition is good.

            - Temperature: 25°C
            """);

    // Act
    var diff =
        _diffEngine.Compare(
            previous,
            current);

    // Assert
    Assert.True(diff.HasChanges);

    var change =
        Assert.Single(diff.Changes);

    Assert.Equal(
        MarkdownChangeType.Added,
        change.Type);

    Assert.Equal(
    2,
    change.CurrentIndex);

    Assert.Equal(
    -1,
    change.PreviousIndex);

    Assert.Null(change.Previous);

    Assert.IsType<ListBlock>(
        change.Current);
}

// [Fact]
// public void Compare_WhenBlockChanges_ShouldReportModified()
// {
//     // Arrange
//     var previous =
//         _parser.Parse(
//             "Current water condition is **Good**.");

//     var current =
//         _parser.Parse(
//             "Current water condition is **Excellent**.");

//     // Act
//     var diff =
//         _diffEngine.Compare(
//             previous,
//             current);

//     // Assert
//     Assert.True(diff.HasChanges);

//     var change =
//         Assert.Single(diff.Changes);

//     Assert.Equal(
//         MarkdownChangeType.Modified,
//         change.Type);

//     Assert.Equal(
//         0,
//         change.Index);

//     Assert.IsType<ParagraphBlock>(
//         change.Previous);

//     Assert.IsType<ParagraphBlock>(
//         change.Current);
// }

[Fact]
public void Compare_WhenBlockIsRemoved_ShouldReportRemoved()
{
    // Arrange
    var previous =
        _parser.Parse("""
            # Pool

            Water condition is good.

            - Temperature: 25°C
            """);

    var current =
        _parser.Parse("""
            # Pool

            Water condition is good.
            """);

    // Act
    var diff =
        _diffEngine.Compare(
            previous,
            current);

    // Assert
    Assert.True(diff.HasChanges);

    var change =
        Assert.Single(diff.Changes);

    Assert.Equal(
        MarkdownChangeType.Removed,
        change.Type);

    Assert.Equal(
    2,
    change.PreviousIndex);

Assert.Equal(
    -1,
    change.CurrentIndex);

    Assert.IsType<ListBlock>(
        change.Previous);

    Assert.Null(change.Current);
}

[Fact]
public void Compare_WhenBlockIsInserted_ShouldOnlyReportInsertion()
{
    // Arrange
    var previous =
        _parser.Parse("""
            # Pool

            Water condition is good.

            Final note.
            """);

    var current =
        _parser.Parse("""
            # Pool

            New information.

            Water condition is good.

            Final note.
            """);

    // Act
    var diff =
        _diffEngine.Compare(
            previous,
            current);

    // Assert
    var change =
        Assert.Single(diff.Changes);

    Assert.Equal(
        MarkdownChangeType.Added,
        change.Type);

    Assert.Equal(
        -1,
        change.PreviousIndex);

    Assert.Equal(
        1,
        change.CurrentIndex);
}

[Fact]
public void Compare_WhenBlockIsRemoved_ShouldOnlyReportRemoval()
{
    // Arrange
    var previous =
        _parser.Parse("""
            # Pool

            Temporary information.

            Water condition is good.
            """);

    var current =
        _parser.Parse("""
            # Pool

            Water condition is good.
            """);

    // Act
    var diff =
        _diffEngine.Compare(
            previous,
            current);

    // Assert
    var change =
        Assert.Single(diff.Changes);

    Assert.Equal(
        MarkdownChangeType.Removed,
        change.Type);

    Assert.Equal(
        1,
        change.PreviousIndex);

    Assert.Equal(
        -1,
        change.CurrentIndex);
}

[Fact]
public void Compare_WhenBlockChanges_ShouldReportModified()
{
    // Arrange
    var previous =
        _parser.Parse(
            "Current water condition is **Good**.");

    var current =
        _parser.Parse(
            "Current water condition is **Excellent**.");

    // Act
    var diff =
        _diffEngine.Compare(
            previous,
            current);

    // Assert
    var change =
        Assert.Single(diff.Changes);

    Assert.Equal(
        MarkdownChangeType.Modified,
        change.Type);

    Assert.Equal(
        0,
        change.PreviousIndex);

    Assert.Equal(
        0,
        change.CurrentIndex);
}

[Fact]
public void Append_WhenExistingBlockChanges_ShouldReportModified()
{
    // Arrange
    var processor =
        
        new MarkdownStreamProcessor(
    new MarkdownBuffer(),
    new MarkdigMarkdownParser(),
    new DocumentReconciler(),
    new DocumentDiffEngine());

    processor.Begin();

    // Act
    processor.Append("# Pool");

    var update =
        processor.Append(" Status");

    // Assert
    var change =
        Assert.Single(
            update.Diff.Changes);

    Assert.Equal(
        MarkdownChangeType.Modified,
        change.Type);

    Assert.Equal(
        0,
        change.PreviousIndex);

    Assert.Equal(
        0,
        change.CurrentIndex);
}

[Fact]
public void Append_WhenNewBlockAppears_ShouldReportAdded()
{
    // Arrange
    var processor =
        new MarkdownStreamProcessor(
    new MarkdownBuffer(),
    new MarkdigMarkdownParser(),
    new DocumentReconciler(),
    new DocumentDiffEngine());

    processor.Begin();

    processor.Append(
        "# Pool\n\nWater condition is good.");

    // Act
    var update =
        processor.Append(
            "\n\n- Temperature: 25°C");

    // Assert
    var change =
        Assert.Single(
            update.Diff.Changes);

    Assert.Equal(
        MarkdownChangeType.Added,
        change.Type);

    Assert.Equal(
        2,
        change.CurrentIndex);

    Assert.IsType<ListBlock>(
        change.Current);
}

[Fact]
public void StreamingMarkdown_ShouldGenerateExpectedChanges()
{
    // Arrange
    var processor =
        new MarkdownStreamProcessor(
    new MarkdownBuffer(),
    new MarkdigMarkdownParser(),
    new DocumentReconciler(),
    new DocumentDiffEngine());

    processor.Begin();

    // Act

    var update1 =
        processor.Append("# Pool");

    var update2 =
        processor.Append(" Status");

    var update3 =
        processor.Append(
            "\n\nWater condition is ");

    var update4 =
        processor.Append("**Good**");

    var update5 =
        processor.Append(
            "\n\n- Temperature: 25°C");

    // Assert

    Assert.Single(update1.Diff.Changes);

    Assert.Equal(
        MarkdownChangeType.Modified,
        Assert.Single(update2.Diff.Changes).Type);

    Assert.Equal(
        MarkdownChangeType.Added,
        Assert.Single(update3.Diff.Changes).Type);

    Assert.Equal(
        MarkdownChangeType.Modified,
        Assert.Single(update4.Diff.Changes).Type);

    Assert.Equal(
        MarkdownChangeType.Added,
        Assert.Single(update5.Diff.Changes).Type);
}

[Fact]
public void CompareIncremental_WhenBlockIsAdded_ShouldReturnDocumentIndex()
{
    var previous =
        _parser.Parse("""
            # Pool

            Water condition is good.
            """);

    var current =
        _parser.Parse("""
            # Pool

            Water condition is good.

            Final note.
            """);

    var diff =
        _diffEngine.CompareIncremental(
            previous,
            current,
            2);

    var change =
        Assert.Single(diff.Changes);

    Assert.Equal(
        MarkdownChangeType.Added,
        change.Type);

    Assert.Equal(
        -1,
        change.PreviousIndex);

    Assert.Equal(
        2,
        change.CurrentIndex);
}

[Fact]
public void CompareIncremental_WhenFinalBlockChanges_ShouldReportModified()
{
    var previous =
        _parser.Parse("""
            # Pool

            Water condition is good.
            """);

    var current =
        _parser.Parse("""
            # Pool

            Water condition is excellent.
            """);

    var diff =
        _diffEngine.CompareIncremental(
            previous,
            current,
            1);

    var change =
        Assert.Single(diff.Changes);

    Assert.Equal(
        MarkdownChangeType.Modified,
        change.Type);

    Assert.Equal(
        1,
        change.PreviousIndex);

    Assert.Equal(
        1,
        change.CurrentIndex);
}

[Fact]
public void CompareIncremental_WhenBlockIsRemoved_ShouldReturnDocumentIndex()
{
    var previous =
        _parser.Parse("""
            # Pool

            Water condition is good.

            Final note.
            """);

    var current =
        _parser.Parse("""
            # Pool

            Water condition is good.
            """);

    var diff =
        _diffEngine.CompareIncremental(
            previous,
            current,
            2);

    var change =
        Assert.Single(diff.Changes);

    Assert.Equal(
        MarkdownChangeType.Removed,
        change.Type);

    Assert.Equal(
        2,
        change.PreviousIndex);

    Assert.Equal(
        -1,
        change.CurrentIndex);
}

[Fact]
public void CompareIncremental_WhenBlockIsInsertedInAffectedRegion_ShouldOnlyReportInsertion()
{
    var previous =
        _parser.Parse("""
            # Pool

            Water condition is good.

            Final note.
            """);

    var current =
        _parser.Parse("""
            # Pool

            Water condition is good.

            New information.

            Final note.
            """);

    var diff =
        _diffEngine.CompareIncremental(
            previous,
            current,
            1);

    var change =
        Assert.Single(diff.Changes);

    Assert.Equal(
        MarkdownChangeType.Added,
        change.Type);

    Assert.Equal(
        -1,
        change.PreviousIndex);

    Assert.Equal(
        2,
        change.CurrentIndex);
}
}