namespace StreamingMarkdown.Core.Models.Inlines;

public sealed class BoldInline : MarkdownInline
{
    public IReadOnlyList<MarkdownInline> Children { get; }

    public BoldInline(IReadOnlyList<MarkdownInline> children)
    {
        ArgumentNullException.ThrowIfNull(children);

        Children = children;
    }
}