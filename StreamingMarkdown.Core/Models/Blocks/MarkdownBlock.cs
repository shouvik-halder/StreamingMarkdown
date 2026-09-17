namespace StreamingMarkdown.Core.Models.Blocks;

public abstract class MarkdownBlock
{
    public Guid Id { get; }

    public int Position { get; }

    protected MarkdownBlock(
        int position,
        Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        Position = position;
    }
}