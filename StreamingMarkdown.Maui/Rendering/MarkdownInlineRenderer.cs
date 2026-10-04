
using StreamingMarkdown.Core.Models.Inlines;
using StreamingMarkdown.Maui.Styling;

namespace StreamingMarkdown.Maui.Rendering;

public sealed class MarkdownInlineRenderer
{
    private readonly MarkdownStyle _style;

    public MarkdownInlineRenderer(MarkdownStyle? style = null)
    {
        _style = style ?? new MarkdownStyle();
    }

    public FormattedString Render(
        IReadOnlyList<MarkdownInline> inlines,
        double? fontSize = null)
    {
        ArgumentNullException.ThrowIfNull(inlines);

        var formattedString = new FormattedString();

        foreach (var inline in inlines)
        {
            AppendInline(formattedString, inline, fontSize);
        }

        return formattedString;
    }

    private void AppendInline(
        FormattedString formattedString,
        MarkdownInline inline,
        double? fontSize)
    {
        switch (inline)
        {
            case TextInline text:
                AppendText(formattedString, text.Text, fontSize);
                break;

            case BoldInline bold:
                AppendChildren(
                    formattedString,
                    bold.Children,
                    FontAttributes.Bold,
                    fontSize);
                break;

            case ItalicInline italic:
                AppendChildren(
                    formattedString,
                    italic.Children,
                    FontAttributes.Italic,
                    fontSize);
                break;

            case HyperlinkInline link:
                AppendLink(formattedString, link, fontSize);
                break;
        }
    }

    private void AppendText(
        FormattedString formattedString,
        string text,
        double? fontSize)
    {
        var span = CreateSpan(text, fontSize);
        formattedString.Spans.Add(span);
    }

    private void AppendChildren(
        FormattedString formattedString,
        IReadOnlyList<MarkdownInline> children,
        FontAttributes attributes,
        double? fontSize)
    {
        var beforeCount = formattedString.Spans.Count;

        foreach (var child in children)
        {
            AppendInline(formattedString, child, fontSize);
        }

        for (var i = beforeCount; i < formattedString.Spans.Count; i++)
        {
            formattedString.Spans[i].FontAttributes |= attributes;
        }
    }

    private void AppendLink(
        FormattedString formattedString,
        HyperlinkInline link,
        double? fontSize)
    {
        var span = CreateSpan(
            ExtractText(link.Children),
            fontSize);

        span.TextDecorations = TextDecorations.Underline;

        span.GestureRecognizers.Add(
            new TapGestureRecognizer
            {
                Command = new Command(async () =>
                {
                    try
                    {
                        await Launcher.Default.OpenAsync(link.Url);
                    }
                    catch
                    {
                        // Ignore launcher failures.
                    }
                })
            });

        formattedString.Spans.Add(span);
    }

    private Span CreateSpan(string text, double? fontSize)
    {
        var span = new Span
        {
            Text = text,
            FontSize = fontSize ?? _style.BodyFontSize,
            TextColor = _style.TextColor,
            LineHeight = _style.LineHeight
        };

        if (!string.IsNullOrWhiteSpace(_style.FontFamily))
        {
            span.FontFamily = _style.FontFamily;
        }

        return span;
    }

    private static string ExtractText(
        IReadOnlyList<MarkdownInline> inlines)
    {
        var result = new System.Text.StringBuilder();

        foreach (var inline in inlines)
        {
            switch (inline)
            {
                case TextInline text:
                    result.Append(text.Text);
                    break;

                case BoldInline bold:
                    result.Append(ExtractText(bold.Children));
                    break;

                case ItalicInline italic:
                    result.Append(ExtractText(italic.Children));
                    break;

                case HyperlinkInline link:
                    result.Append(ExtractText(link.Children));
                    break;
            }
        }

        return result.ToString();
    }
}