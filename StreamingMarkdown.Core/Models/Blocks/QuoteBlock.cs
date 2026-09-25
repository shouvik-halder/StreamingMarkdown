namespace StreamingMarkdown.Core.Models.Blocks;

public sealed class QuoteBlock : MarkdownBlock
{
    public IReadOnlyList<MarkdownBlock> Blocks { get; }

    public QuoteBlock(
        int position,
        int sourceStart,
        int sourceEnd,
        IReadOnlyList<MarkdownBlock> blocks,
        Guid? id = null)
        : base(
            position,
            sourceStart,
            sourceEnd,
            id)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        Blocks = blocks;
    }
}