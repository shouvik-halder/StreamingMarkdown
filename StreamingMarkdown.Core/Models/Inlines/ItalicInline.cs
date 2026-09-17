namespace StreamingMarkdown.Core.Models.Inlines;

public sealed class ItalicInline : MarkdownInline
{
    public IReadOnlyList<MarkdownInline> Children { get; }

    public ItalicInline(IReadOnlyList<MarkdownInline> children)
    {
        ArgumentNullException.ThrowIfNull(children);

        Children = children;
    }
}