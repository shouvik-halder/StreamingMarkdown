namespace StreamingMarkdown.Core.Models.Blocks;

public sealed class ListBlock : MarkdownBlock
{
    public bool IsOrdered { get; }

    public IReadOnlyList<ListItem> Items { get; }

    public ListBlock(
    int position,
    bool isOrdered,
    IReadOnlyList<ListItem> items,
    Guid? id = null)
    : base(position, id)
    {
        ArgumentNullException.ThrowIfNull(items);

        IsOrdered = isOrdered;
        Items = items;
    }
}