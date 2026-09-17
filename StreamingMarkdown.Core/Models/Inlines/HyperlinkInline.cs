namespace StreamingMarkdown.Core.Models.Inlines;

public sealed class HyperlinkInline : MarkdownInline
{
    public string Url { get; }

    public IReadOnlyList<MarkdownInline> Children { get; }

    public HyperlinkInline(
        string url,
        IReadOnlyList<MarkdownInline> children)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(children);

        Url = url;
        Children = children;
    }
}