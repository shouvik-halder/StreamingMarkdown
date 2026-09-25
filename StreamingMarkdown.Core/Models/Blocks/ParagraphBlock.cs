using StreamingMarkdown.Core.Models.Inlines;

namespace StreamingMarkdown.Core.Models.Blocks;

public sealed class ParagraphBlock : MarkdownBlock
{
    public IReadOnlyList<MarkdownInline> Inlines { get; }

    public ParagraphBlock(
        int position,
        int sourceStart,
        int sourceEnd,
        IReadOnlyList<MarkdownInline> inlines,
        Guid? id = null)
        : base(
            position,
            sourceStart,
            sourceEnd,
            id)
    {
        ArgumentNullException.ThrowIfNull(inlines);

        Inlines = inlines;
    }
}