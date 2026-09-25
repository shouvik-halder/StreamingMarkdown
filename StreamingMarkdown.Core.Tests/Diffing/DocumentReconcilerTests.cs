using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Core.Models.Inlines;

namespace StreamingMarkdown.Core.Tests.Diffing;

public sealed class DocumentReconcilerTests
{
    private readonly DocumentReconciler _reconciler = new();

    [Fact]
    public void Reconcile_WhenDocumentIsEmpty_ShouldRemainEmpty()
    {
        var previous = MarkdownDocument.Empty;
        var current = MarkdownDocument.Empty;

        var result =
            _reconciler.Reconcile(
                previous,
                current);

        Assert.Empty(result.Blocks);
    }

    [Fact]
    public void Reconcile_WhenBlockIsUnchanged_ShouldPreserveId()
    {
        var previousBlock =
            new ParagraphBlock(
                0,
                0,
                4,
                new MarkdownInline[]
                {
                    new TextInline("Hello")
                });

        var currentBlock =
            new ParagraphBlock(
                0,
                0,
                4,
                new MarkdownInline[]
                {
                    new TextInline("Hello")
                });

        var previous =
            new MarkdownDocument(
                new MarkdownBlock[]
                {
                    previousBlock
                });

        var current =
            new MarkdownDocument(
                new MarkdownBlock[]
                {
                    currentBlock
                });

        var result =
            _reconciler.Reconcile(
                previous,
                current);

        Assert.Single(result.Blocks);

        Assert.Equal(
            previousBlock.Id,
            result.Blocks[0].Id);
    }

    [Fact]
    public void Reconcile_WhenBlockIsNew_ShouldGenerateNewId()
    {
        var previousBlock =
            new ParagraphBlock(
                0,
                0,
                4,
                new MarkdownInline[]
                {
                    new TextInline("Hello")
                });

        var currentBlock =
            new ParagraphBlock(
                1,
                6,
                10,
                new MarkdownInline[]
                {
                    new TextInline("World")
                });

        var previous =
            new MarkdownDocument(
                new MarkdownBlock[]
                {
                    previousBlock
                });

        var current =
            new MarkdownDocument(
                new MarkdownBlock[]
                {
                    previousBlock,
                    currentBlock
                });

        var result =
            _reconciler.Reconcile(
                previous,
                current);

        Assert.Equal(
            2,
            result.Blocks.Count);

        Assert.Equal(
            previousBlock.Id,
            result.Blocks[0].Id);

        Assert.NotEqual(
            previousBlock.Id,
            result.Blocks[1].Id);
    }

    [Fact]
    public void Reconcile_WhenBlockMoves_ShouldPreserveId()
    {
        var first =
            new ParagraphBlock(
                0,
                0,
                4,
                new MarkdownInline[]
                {
                    new TextInline("First")
                });

        var second =
            new ParagraphBlock(
                1,
                6,
                11,
                new MarkdownInline[]
                {
                    new TextInline("Second")
                });

        var previous =
            new MarkdownDocument(
                new MarkdownBlock[]
                {
                    first,
                    second
                });

        var currentFirst =
            new ParagraphBlock(
                0,
                0,
                5,
                new MarkdownInline[]
                {
                    new TextInline("Second")
                });

        var currentSecond =
            new ParagraphBlock(
                1,
                7,
                11,
                new MarkdownInline[]
                {
                    new TextInline("First")
                });

        var current =
            new MarkdownDocument(
                new MarkdownBlock[]
                {
                    currentFirst,
                    currentSecond
                });

        var result =
            _reconciler.Reconcile(
                previous,
                current);

        Assert.Equal(
            second.Id,
            result.Blocks[0].Id);

        Assert.Equal(
            first.Id,
            result.Blocks[1].Id);
    }

    [Fact]
    public void Reconcile_WhenBlockContentChanges_ShouldPreserveId()
    {
        var previousBlock =
            new ParagraphBlock(
                0,
                0,
                4,
                new MarkdownInline[]
                {
                    new TextInline("Hello")
                });

        var currentBlock =
            new ParagraphBlock(
                0,
                0,
                10,
                new MarkdownInline[]
                {
                    new TextInline("Hello world")
                });

        var previous =
            new MarkdownDocument(
                new MarkdownBlock[]
                {
                    previousBlock
                });

        var current =
            new MarkdownDocument(
                new MarkdownBlock[]
                {
                    currentBlock
                });

        var result =
            _reconciler.Reconcile(
                previous,
                current);

        Assert.Single(result.Blocks);

        Assert.Equal(
            previousBlock.Id,
            result.Blocks[0].Id);
    }

    [Fact]
    public void Reconcile_WhenLastBlockGrows_ShouldPreserveId()
    {
        var heading =
            new HeadingBlock(
                0,
                0,
                6,
                1,
                new MarkdownInline[]
                {
                    new TextInline("Hello")
                });

        var previousParagraph =
            new ParagraphBlock(
                1,
                8,
                17,
                new MarkdownInline[]
                {
                    new TextInline("The quick")
                });

        var currentParagraph =
            new ParagraphBlock(
                1,
                8,
                27,
                new MarkdownInline[]
                {
                    new TextInline("The quick brown fox")
                });

        var previous =
            new MarkdownDocument(
                new MarkdownBlock[]
                {
                    heading,
                    previousParagraph
                });

        var current =
            new MarkdownDocument(
                new MarkdownBlock[]
                {
                    heading,
                    currentParagraph
                });

        var result =
            _reconciler.Reconcile(
                previous,
                current);

        Assert.Equal(
            2,
            result.Blocks.Count);

        Assert.Equal(
            previousParagraph.Id,
            result.Blocks[1].Id);
    }


    [Fact]
public void ReconcileIncremental_WhenPrefixIsReused_ShouldPreservePrefixIds()
{
    var first =
        new ParagraphBlock(
            0,
            0,
            5,
            new MarkdownInline[]
            {
                new TextInline("First")
            });

    var second =
        new ParagraphBlock(
            1,
            7,
            13,
            new MarkdownInline[]
            {
                new TextInline("Second")
            });

    var previous =
        new MarkdownDocument(
            new MarkdownBlock[]
            {
                first,
                second
            });

    var currentFirst =
        new ParagraphBlock(
            0,
            0,
            5,
            new MarkdownInline[]
            {
                new TextInline("First")
            });

    var currentSecond =
        new ParagraphBlock(
            1,
            7,
            21,
            new MarkdownInline[]
            {
                new TextInline("Second updated")
            });

    var current =
        new MarkdownDocument(
            new MarkdownBlock[]
            {
                currentFirst,
                currentSecond
            });

    var result =
        _reconciler.ReconcileIncremental(
            previous,
            current,
            1);

    Assert.Equal(
        2,
        result.Blocks.Count);

    // Stable prefix keeps its identity.
    Assert.Equal(
        first.Id,
        result.Blocks[0].Id);

    // Affected block keeps the identity of the previous
    // corresponding block.
    Assert.Equal(
        second.Id,
        result.Blocks[1].Id);
}
}