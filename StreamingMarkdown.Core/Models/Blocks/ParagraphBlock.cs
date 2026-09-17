using StreamingMarkdown.Core.Models.Inlines;

namespace StreamingMarkdown.Core.Models.Blocks;

public sealed class ParagraphBlock : MarkdownBlock
{
    public IReadOnlyList<MarkdownInline> Inlines { get; }

    public ParagraphBlock(
    int position,
    IReadOnlyList<MarkdownInline> inlines,
    Guid? id = null)
    : base(position, id)
    {
        ArgumentNullException.ThrowIfNull(inlines);

        Inlines = inlines;
    }
}