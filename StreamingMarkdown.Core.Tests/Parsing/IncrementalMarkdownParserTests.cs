using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Models.Inlines;
using StreamingMarkdown.Core.Parsing;

namespace StreamingMarkdown.Core.Tests.Parsing;

public class IncrementalMarkdownParserTests
{
    [Fact]
    public void Parse_FirstDocument_ReturnsParsedDocument()
    {
        var parser =
            new IncrementalMarkdownParser(
                new MarkdigMarkdownParser());

        var result =
            parser.Parse(
                "# Hello\n\nThis is a paragraph.",
                MarkdownDocument.Empty);

        Assert.Equal(
            2,
            result.Document.Blocks.Count);
    }

    [Fact]
    public void Parse_UpdatedDocument_MatchesFullParser()
    {
        var fullParser =
            new MarkdigMarkdownParser();

        var incrementalParser =
            new IncrementalMarkdownParser(
                fullParser);

        var firstMarkdown =
            "# Hello\n\nFirst paragraph.";

        var secondMarkdown =
            "# Hello\n\nFirst paragraph.\n\nSecond paragraph.";

        var firstResult =
            incrementalParser.Parse(
                firstMarkdown,
                MarkdownDocument.Empty);

        var incrementalResult =
            incrementalParser.Parse(
                secondMarkdown,
                firstResult.Document);

        var expectedDocument =
            fullParser.Parse(secondMarkdown);

        Assert.Equal(
            expectedDocument.Blocks.Count,
            incrementalResult.Document.Blocks.Count);
    }

    [Fact]
    public void Parse_UpdatedDocument_PreservesStablePrefixIds()
    {
        var parser =
            new IncrementalMarkdownParser(
                new MarkdigMarkdownParser());

        var firstResult =
            parser.Parse(
                "# Hello\n\nFirst paragraph.\n\nSecond paragraph.",
                MarkdownDocument.Empty);

        var firstDocument =
            firstResult.Document;

        var firstHeadingId =
            firstDocument.Blocks[0].Id;

        var firstParagraphId =
            firstDocument.Blocks[1].Id;

        var updatedResult =
            parser.Parse(
                "# Hello\n\nFirst paragraph.\n\nSecond paragraph updated.",
                firstDocument);

        var updatedDocument =
            updatedResult.Document;

        Assert.Equal(
            firstHeadingId,
            updatedDocument.Blocks[0].Id);

        Assert.Equal(
            firstParagraphId,
            updatedDocument.Blocks[1].Id);
    }

    [Fact]
    public void Parse_UpdatedDocument_ReparsesFinalBlock()
    {
        var parser =
            new IncrementalMarkdownParser(
                new MarkdigMarkdownParser());

        var firstResult =
            parser.Parse(
                "# Hello\n\nFirst paragraph.",
                MarkdownDocument.Empty);

        var firstDocument =
            firstResult.Document;

        var originalParagraphId =
            firstDocument.Blocks[1].Id;

        var updatedResult =
            parser.Parse(
                "# Hello\n\nFirst paragraph updated.",
                firstDocument);

        var updatedDocument =
            updatedResult.Document;

        Assert.Equal(
            2,
            updatedDocument.Blocks.Count);

        Assert.NotEqual(
            Guid.Empty,
            updatedDocument.Blocks[1].Id);

        Assert.NotEqual(
            originalParagraphId,
            updatedDocument.Blocks[1].Id);
    }

    [Fact]
    public void Parse_UpdatedDocument_AppendedBlockIsIncluded()
    {
        var parser =
            new IncrementalMarkdownParser(
                new MarkdigMarkdownParser());

        var firstResult =
            parser.Parse(
                "# Hello\n\nFirst paragraph.",
                MarkdownDocument.Empty);

        var updatedResult =
            parser.Parse(
                "# Hello\n\nFirst paragraph.\n\nSecond paragraph.",
                firstResult.Document);

        var updatedDocument =
            updatedResult.Document;

        Assert.Equal(
            3,
            updatedDocument.Blocks.Count);

        Assert.Equal(
            "Second paragraph.",
            ((TextInline)
                ((StreamingMarkdown.Core.Models.Blocks.ParagraphBlock)
                    updatedDocument.Blocks[2])
                .Inlines[0])
            .Text);
    }

    [Fact]
    public void Parse_UpdatedDocument_PreservesSourcePositions()
    {
        var fullParser =
            new MarkdigMarkdownParser();

        var incrementalParser =
            new IncrementalMarkdownParser(
                fullParser);

        var firstMarkdown =
            "# Hello\n\nFirst paragraph.";

        var secondMarkdown =
            "# Hello\n\nFirst paragraph.\n\nSecond paragraph.";

        var firstResult =
            incrementalParser.Parse(
                firstMarkdown,
                MarkdownDocument.Empty);

        var incrementalResult =
            incrementalParser.Parse(
                secondMarkdown,
                firstResult.Document);

        var expectedDocument =
            fullParser.Parse(secondMarkdown);

        var actualDocument =
            incrementalResult.Document;

        Assert.Equal(
            expectedDocument.Blocks.Count,
            actualDocument.Blocks.Count);

        for (var i = 0;
             i < expectedDocument.Blocks.Count;
             i++)
        {
            Assert.Equal(
                expectedDocument.Blocks[i].SourceStart,
                actualDocument.Blocks[i].SourceStart);

            Assert.Equal(
                expectedDocument.Blocks[i].SourceEnd,
                actualDocument.Blocks[i].SourceEnd);
        }
    }


    [Fact]
public void Parse_WhenPreviousDocumentHasMultipleBlocks_ShouldReportStableBoundary()
{
    var parser =
        new IncrementalMarkdownParser(
            new MarkdigMarkdownParser());

    var firstMarkdown =
        "# Heading\n\n" +
        "First paragraph.\n\n" +
        "# Second Heading\n\n" +
        "Second paragraph.";

    var firstDocument =
        new MarkdigMarkdownParser()
            .Parse(firstMarkdown);

    var secondMarkdown =
        firstMarkdown +
        " More text.";

    var result =
        parser.Parse(
            secondMarkdown,
            firstDocument);

    Assert.Equal(
        firstDocument.Blocks.Count - 1,
        result.ReusedBlockCount);

    var stableBlock =
        firstDocument.Blocks[
            result.ReusedBlockCount - 1];

    Assert.Equal(
        stableBlock.SourceEnd,
        result.StableSourceEnd);

    Assert.Equal(
        firstDocument.Blocks[
            result.ReusedBlockCount].SourceStart,
        result.ReparseStart);
}

[Fact]
public void Parse_WhenStablePrefixExists_ShouldReuseStablePrefixWithoutReparsingIt()
{
    var parser =
        new IncrementalMarkdownParser(
            new MarkdigMarkdownParser());

    var firstMarkdown =
        "# Heading\n\n" +
        "First paragraph.\n\n" +
        "# Second Heading\n\n" +
        "Second paragraph.";

    var firstDocument =
        new MarkdigMarkdownParser()
            .Parse(firstMarkdown);

    var updatedMarkdown =
        firstMarkdown +
        " More text.";

    var result =
        parser.Parse(
            updatedMarkdown,
            firstDocument);

    Assert.Equal(3, result.ReusedBlockCount);

    Assert.Equal(
        firstDocument.Blocks[2].SourceEnd,
        result.StableSourceEnd);

    Assert.Equal(
        firstDocument.Blocks[3].SourceStart,
        result.ReparseStart);

    // Stable prefix must retain the exact previous block instances.
    Assert.Same(
        firstDocument.Blocks[0],
        result.Document.Blocks[0]);

    Assert.Same(
        firstDocument.Blocks[1],
        result.Document.Blocks[1]);

    Assert.Same(
        firstDocument.Blocks[2],
        result.Document.Blocks[2]);
}
}