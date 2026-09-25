namespace StreamingMarkdown.Core.Models.Blocks;

public abstract class MarkdownBlock
{
    public Guid Id { get; }

    public int Position { get; }

    public int SourceStart { get; }

    public int SourceEnd { get; }

    protected MarkdownBlock(
        int position,
        int sourceStart,
        int sourceEnd,
        Guid? id = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sourceStart);

        ArgumentOutOfRangeException.ThrowIfLessThan(sourceEnd, sourceStart);

        Id = id ?? Guid.NewGuid();

        Position = position;

        SourceStart = sourceStart;

        SourceEnd = sourceEnd;
    }
}