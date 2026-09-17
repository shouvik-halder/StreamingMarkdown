namespace StreamingMarkdown.Core.Models.Blocks;

public sealed class ListItem
{
    public IReadOnlyList<MarkdownBlock> Blocks { get; }

    public ListItem(IReadOnlyList<MarkdownBlock> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        Blocks = blocks;
    }
}