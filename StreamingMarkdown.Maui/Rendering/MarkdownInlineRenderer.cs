using StreamingMarkdown.Core.Models.Inlines;
using StreamingMarkdown.Maui.Styling;

namespace StreamingMarkdown.Maui.Rendering;

public sealed class MarkdownInlineRenderer
{
    private readonly MarkdownStyle _style;

    public MarkdownInlineRenderer(
        MarkdownStyle? style = null)
    {
        _style = style ?? new MarkdownStyle();
    }

    public FormattedString Render(
        IReadOnlyList<MarkdownInline> inlines)
    {
        ArgumentNullException.ThrowIfNull(inlines);

        var formattedString = new FormattedString();

        foreach (var inline in inlines)
        {
            AppendInline(
                formattedString,
                inline);
        }

        return formattedString;
    }

    private void AppendInline(
        FormattedString formattedString,
        MarkdownInline inline)
    {
        switch (inline)
        {
            case TextInline text:
                AppendText(
                    formattedString,
                    text.Text);

                break;

            case BoldInline bold:
                AppendChildren(
                    formattedString,
                    bold.Children,
                    FontAttributes.Bold);

                break;

            case ItalicInline italic:
                AppendChildren(
                    formattedString,
                    italic.Children,
                    FontAttributes.Italic);

                break;

            case HyperlinkInline link:
                AppendLink(
                    formattedString,
                    link);

                break;
        }
    }

    private static void AppendText(
        FormattedString formattedString,
        string text)
    {
        formattedString.Spans.Add(
            new Span
            {
                Text = text
            });
    }

    private void AppendChildren(
        FormattedString formattedString,
        IReadOnlyList<MarkdownInline> children,
        FontAttributes attributes)
    {
        var beforeCount =
            formattedString.Spans.Count;

        foreach (var child in children)
        {
            AppendInline(
                formattedString,
                child);
        }

        for (var i = beforeCount;
             i < formattedString.Spans.Count;
             i++)
        {
            formattedString.Spans[i].FontAttributes =
                attributes;
        }
    }

    private void AppendLink(
        FormattedString formattedString,
        HyperlinkInline link)
    {
        var text =
            ExtractText(link.Children);

        var span =
            new Span
            {
                Text = text,
                TextDecorations = TextDecorations.Underline
            };

        span.GestureRecognizers.Add(
            new TapGestureRecognizer
            {
                Command = new Command(
                    async () =>
                    {
                        try
                        {
                            await Launcher.Default.OpenAsync(
                                link.Url);
                        }
                        catch
                        {
                            // Ignore launcher failures.
                            // The renderer should not crash
                            // because a URL cannot be opened.
                        }
                    })
            });

        formattedString.Spans.Add(span);
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
                    result.Append(
                        ExtractText(
                            bold.Children));

                    break;

                case ItalicInline italic:
                    result.Append(
                        ExtractText(
                            italic.Children));

                    break;

                case HyperlinkInline link:
                    result.Append(
                        ExtractText(
                            link.Children));

                    break;
            }
        }

        return result.ToString();
    }
}