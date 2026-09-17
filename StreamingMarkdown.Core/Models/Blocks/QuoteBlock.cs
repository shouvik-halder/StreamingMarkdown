namespace StreamingMarkdown.Core.Models.Blocks;

public sealed class QuoteBlock : MarkdownBlock
{
    public IReadOnlyList<MarkdownBlock> Blocks { get; }

    public QuoteBlock(
    int position,
    IReadOnlyList<MarkdownBlock> blocks,
    Guid? id = null)
    : base(position, id)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        Blocks = blocks;
    }
}