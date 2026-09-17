namespace StreamingMarkdown.Core.Models.Inlines;

public sealed class TextInline : MarkdownInline
{
    public string Text { get; }

    public TextInline(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        Text = text;
    }
}