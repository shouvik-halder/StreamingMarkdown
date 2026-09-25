namespace StreamingMarkdown.Core.Models.Blocks;

public sealed class ListBlock : MarkdownBlock
{
    public bool IsOrdered { get; }

    public IReadOnlyList<ListItem> Items { get; }

    public ListBlock(
        int position,
        int sourceStart,
        int sourceEnd,
        bool isOrdered,
        IReadOnlyList<ListItem> items,
        Guid? id = null)
        : base(
            position,
            sourceStart,
            sourceEnd,
            id)
    {
        ArgumentNullException.ThrowIfNull(items);

        IsOrdered = isOrdered;

        Items = items;
    }
}