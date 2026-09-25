using StreamingMarkdown.Core.Models;

namespace StreamingMarkdown.Core.Parsing;

public sealed class IncrementalParseResult
{
    public MarkdownDocument Document { get; }
    public int ReusedBlockCount { get; }
    public int ReparseStart { get; }
    public int StableSourceEnd { get; }

    public IncrementalParseResult(
        MarkdownDocument document,
        int reusedBlockCount,
        int reparseStart,
        int stableSourceEnd)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentOutOfRangeException.ThrowIfNegative(reusedBlockCount);
        ArgumentOutOfRangeException.ThrowIfNegative(reparseStart);
        ArgumentOutOfRangeException.ThrowIfNegative(stableSourceEnd);

        Document = document;
        ReusedBlockCount = reusedBlockCount;
        ReparseStart = reparseStart;
        StableSourceEnd = stableSourceEnd;
    }
}