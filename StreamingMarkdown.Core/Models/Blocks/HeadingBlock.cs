using StreamingMarkdown.Core.Models.Inlines;

namespace StreamingMarkdown.Core.Models.Blocks;

public sealed class HeadingBlock : MarkdownBlock
{
    public int Level { get; }

    public IReadOnlyList<MarkdownInline> Inlines { get; }

    public HeadingBlock(
    int position,
    int level,
    IReadOnlyList<MarkdownInline> inlines,
    Guid? id = null)
    : base(position, id)
    {
        if (level is < 1 or > 6)
            throw new ArgumentOutOfRangeException(nameof(level));

        ArgumentNullException.ThrowIfNull(inlines);

        Level = level;
        Inlines = inlines;
    }
}