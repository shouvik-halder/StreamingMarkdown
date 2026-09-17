using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Core.Models.Inlines;
using StreamingMarkdown.Core.Parsing;
using StreamingMarkdown.Core.Streaming;

namespace StreamingMarkdown.Core.Tests.Streaming;

public sealed class MarkdownStreamProcessorTests
{
    private static MarkdownStreamProcessor CreateProcessor()
    {
        return new MarkdownStreamProcessor(
    new MarkdownBuffer(),
    new MarkdigMarkdownParser(),
    new DocumentReconciler(),
    new DocumentDiffEngine());
    }

    [Fact]
    public void Begin_ShouldStartStreaming()
    {
        // Arrange
        var processor = CreateProcessor();

        // Act
        var update = processor.Begin();

        // Assert
        Assert.False(update.IsCompleted);
        Assert.Empty(update.Document.Blocks);
        Assert.Equal(1, update.Version);
    }

    [Fact]
    public void Append_ShouldParseAccumulatedMarkdown()
    {
        // Arrange
        var processor = CreateProcessor();

        processor.Begin();

        // Act
        processor.Append("# Pool");
        var update =
            processor.Append(" Status");

        // Assert
        Assert.Single(update.Document.Blocks);

        var heading =
            Assert.IsType<HeadingBlock>(
                update.Document.Blocks[0]);

        Assert.Equal(
            "Pool Status",
            ((StreamingMarkdown.Core.Models.Inlines.TextInline)
                heading.Inlines[0]).Text);
    }

    [Fact]
    public void Complete_ShouldMarkUpdateAsCompleted()
    {
        // Arrange
        var processor = CreateProcessor();

        processor.Begin();

        processor.Append(
            "# Pool\n\nWater is good.");

        // Act
        var update =
            processor.Complete();

        // Assert
        Assert.True(update.IsCompleted);

        Assert.Equal(
            2,
            update.Document.Blocks.Count);
    }


    [Fact]
public void StreamingChunks_ShouldProduceSameDocumentAsCompleteMarkdown()
{
    // Arrange
    const string completeMarkdown = """
        # Pool Status

        Current water condition is **Good**.

        - Temperature: 25°C
        - PH: 7.2
        """;

    var processor = CreateProcessor();

    processor.Begin();

    // Act
    processor.Append("# Pool");
    processor.Append(" Status");
    processor.Append("\n\nCurrent water ");
    processor.Append("condition is ");
    processor.Append("**Good**");
    processor.Append(".\n\n");
    processor.Append("- Temperature: ");
    processor.Append("25°C\n");
    processor.Append("- PH: ");
    processor.Append("7.2");

    var streamedResult =
        processor.Complete();

    var directParser =
        new MarkdigMarkdownParser();

    var directResult =
        directParser.Parse(
            completeMarkdown);

    // Assert
    Assert.Equal(
        directResult.Blocks.Count,
        streamedResult.Document.Blocks.Count);
}

[Fact]
public void Append_WhenBlockGrows_ShouldPreserveBlockId()
{
    var processor =
        new MarkdownStreamProcessor(
            new MarkdownBuffer(),
            new MarkdigMarkdownParser(),
            new DocumentReconciler(),
            new DocumentDiffEngine());

    processor.Begin();

    var firstUpdate =
        processor.Append("# Hel");

    var firstBlock =
        Assert.Single(
            firstUpdate.Document.Blocks);

    var firstId =
        firstBlock.Id;

    var secondUpdate =
        processor.Append("lo");

    var secondBlock =
        Assert.Single(
            secondUpdate.Document.Blocks);

    Assert.Equal(
        firstId,
        secondBlock.Id);

    var thirdUpdate =
        processor.Append(" World");

    var thirdBlock =
        Assert.Single(
            thirdUpdate.Document.Blocks);

    Assert.Equal(
        firstId,
        thirdBlock.Id);
}

[Fact]
public void Append_WhenNewBlockAppears_ShouldCreateNewBlockId()
{
    var processor =
        new MarkdownStreamProcessor(
            new MarkdownBuffer(),
            new MarkdigMarkdownParser(),
            new DocumentReconciler(),
            new DocumentDiffEngine());

    processor.Begin();

    var firstUpdate =
        processor.Append("# Hello");

    var heading =
        Assert.Single(
            firstUpdate.Document.Blocks);

    var headingId =
        heading.Id;

    var secondUpdate =
        processor.Append("\n\nThis is streaming.");

    Assert.Equal(
        2,
        secondUpdate.Document.Blocks.Count);

    var secondHeading =
        secondUpdate.Document.Blocks[0];

    var paragraph =
        secondUpdate.Document.Blocks[1];

    Assert.Equal(
        headingId,
        secondHeading.Id);

    Assert.NotEqual(
        headingId,
        paragraph.Id);
}
[Fact]
public void Append_WhenIncompleteBoldBecomesComplete_ShouldPreserveBlockId()
{
    var processor =
        new MarkdownStreamProcessor(
            new MarkdownBuffer(),
            new MarkdigMarkdownParser(),
            new DocumentReconciler(),
            new DocumentDiffEngine());

    processor.Begin();

    var firstUpdate =
        processor.Append("**Hel");

    var firstBlock =
        Assert.Single(
            firstUpdate.Document.Blocks);

    var firstId =
        firstBlock.Id;

    var firstParagraph =
        Assert.IsType<ParagraphBlock>(
            firstBlock);

    Assert.Equal(
        2,
        firstParagraph.Inlines.Count);

    var firstMarker =
        Assert.IsType<TextInline>(
            firstParagraph.Inlines[0]);

    var firstText =
        Assert.IsType<TextInline>(
            firstParagraph.Inlines[1]);

    Assert.Equal(
        "**",
        firstMarker.Text);

    Assert.Equal(
        "Hel",
        firstText.Text);

    var secondUpdate =
        processor.Append("lo**");

    var secondBlock =
        Assert.Single(
            secondUpdate.Document.Blocks);

    Assert.Equal(
        firstId,
        secondBlock.Id);

    var secondParagraph =
        Assert.IsType<ParagraphBlock>(
            secondBlock);

    var secondInline =
        Assert.Single(
            secondParagraph.Inlines);

    var bold =
        Assert.IsType<BoldInline>(
            secondInline);

    var text =
        Assert.IsType<TextInline>(
            Assert.Single(
                bold.Children));

    Assert.Equal(
        "Hello",
        text.Text);
}
[Fact]
public void Append_WhenIncompleteLinkBecomesComplete_ShouldPreserveBlockId()
{
    var processor =
        new MarkdownStreamProcessor(
            new MarkdownBuffer(),
            new MarkdigMarkdownParser(),
            new DocumentReconciler(),
            new DocumentDiffEngine());

    processor.Begin();

    var firstUpdate =
        processor.Append("[OpenAI");

    var firstBlock =
        Assert.Single(
            firstUpdate.Document.Blocks);

    var firstId =
        firstBlock.Id;

    var secondUpdate =
        processor.Append(
            "](https://openai.com)");

    var secondBlock =
        Assert.Single(
            secondUpdate.Document.Blocks);

    Assert.Equal(
        firstId,
        secondBlock.Id);

    var paragraph =
        Assert.IsType<ParagraphBlock>(
            secondBlock);

    var link =
        Assert.IsType<HyperlinkInline>(
            Assert.Single(paragraph.Inlines));

    Assert.Equal(
        "https://openai.com",
        link.Url);
}
}