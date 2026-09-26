using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Models.Blocks;

namespace StreamingMarkdown.Core.Parsing;

public sealed class IncrementalMarkdownParser
    : IIncrementalMarkdownParser
{
    private readonly IMarkdownParser _fullParser;

    public IncrementalMarkdownParser(
        IMarkdownParser fullParser)
    {
        ArgumentNullException.ThrowIfNull(fullParser);

        _fullParser = fullParser;
    }

    public IncrementalParseResult Parse(
        string markdown,
        MarkdownDocument previousDocument)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        ArgumentNullException.ThrowIfNull(previousDocument);

        // First parse: there is nothing to reuse.
        if (previousDocument.Blocks.Count == 0)
        {
            var document =
                _fullParser.Parse(markdown);

            return new IncrementalParseResult(
    document,
    0,
    0,
    0);
        }

        // Conservatively treat the final block as potentially
        // affected by newly appended Markdown.
        var reusableBlockCount =
            previousDocument.Blocks.Count - 1;

        if (reusableBlockCount <= 0)
        {
            var document =
                _fullParser.Parse(markdown);

            return new IncrementalParseResult(
    document,
    0,
    0,
    0);
        }

        var reusableBlocks =
            previousDocument.Blocks
                .Take(reusableBlockCount)
                .ToList();

        var reparseStart =
            previousDocument.Blocks[
                reusableBlockCount].SourceStart;

        if (reparseStart < 0 ||
            reparseStart >= markdown.Length)
        {
            var document =
                _fullParser.Parse(markdown);

            return new IncrementalParseResult(
    document,
    0,
    0,
    0);
        }

        var suffix =
            markdown[reparseStart..];

        var parsedSuffix =
            _fullParser.Parse(suffix);

        var result =
            new List<MarkdownBlock>(
                reusableBlocks.Count +
                parsedSuffix.Blocks.Count);

        result.AddRange(reusableBlocks);

        foreach (var block in parsedSuffix.Blocks)
        {
            result.Add(
                OffsetBlock(
                    block,
                    reparseStart,
                    result.Count));
        }

        var incrementalDocument =
    new MarkdownDocument(result);

var stableSourceEnd =
    previousDocument.Blocks[
        reusableBlockCount - 1].SourceEnd;

return new IncrementalParseResult(
    incrementalDocument,
    reusableBlockCount,
    reparseStart,
    stableSourceEnd);
    }

    private static MarkdownBlock OffsetBlock(
        MarkdownBlock block,
        int sourceOffset,
        int position)
    {
        return block switch
        {
            HeadingBlock heading =>
                new HeadingBlock(
                    position,
                    heading.SourceStart + sourceOffset,
                    heading.SourceEnd + sourceOffset,
                    heading.Level,
                    heading.Inlines,
                    heading.Id),

            ParagraphBlock paragraph =>
                new ParagraphBlock(
                    position,
                    paragraph.SourceStart + sourceOffset,
                    paragraph.SourceEnd + sourceOffset,
                    paragraph.Inlines,
                    paragraph.Id),

            ListBlock list =>
                new ListBlock(
                    position,
                    list.SourceStart + sourceOffset,
                    list.SourceEnd + sourceOffset,
                    list.IsOrdered,
                    list.Items,
                    list.Id),

            QuoteBlock quote =>
                new QuoteBlock(
                    position,
                    quote.SourceStart + sourceOffset,
                    quote.SourceEnd + sourceOffset,
                    quote.Blocks,
                    quote.Id),

            _ => throw new NotSupportedException(
                $"Unsupported block type: {block.GetType().Name}")
        };
    }

    public IncrementalParseResult ParseSuffix(
    string suffix,
    MarkdownDocument previousDocument,
    int reparseStart)
{
    ArgumentNullException.ThrowIfNull(suffix);
    ArgumentNullException.ThrowIfNull(previousDocument);
    ArgumentOutOfRangeException.ThrowIfNegative(reparseStart);

    if (previousDocument.Blocks.Count == 0)
    {
        var document =
            _fullParser.Parse(suffix);

        return new IncrementalParseResult(
            document,
            0,
            reparseStart,
            0);
    }

    var reusableBlockCount =
        previousDocument.Blocks.Count - 1;

    if (reusableBlockCount <= 0)
    {
        var document =
            _fullParser.Parse(suffix);

        return new IncrementalParseResult(
            document,
            0,
            reparseStart,
            0);
    }

    var parsedSuffix =
    _fullParser.Parse(suffix);

var offsetSuffix =
    new List<MarkdownBlock>(
        parsedSuffix.Blocks.Count);

foreach (var block in parsedSuffix.Blocks)
{
    offsetSuffix.Add(
        OffsetBlock(
            block,
            reparseStart,
            reusableBlockCount +
            offsetSuffix.Count));
}

var result =
    new CompositeMarkdownBlockList(
        new PrefixMarkdownBlockList(
            previousDocument.Blocks,
            reusableBlockCount),
        offsetSuffix);

var incrementalDocument =
    new MarkdownDocument(result);

    var stableSourceEnd =
        previousDocument.Blocks[
            reusableBlockCount - 1].SourceEnd;

    return new IncrementalParseResult(
        incrementalDocument,
        reusableBlockCount,
        reparseStart,
        stableSourceEnd);
}
}