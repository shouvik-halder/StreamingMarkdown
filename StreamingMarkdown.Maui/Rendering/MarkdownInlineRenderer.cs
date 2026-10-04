
using System.Text;
using StreamingMarkdown.Core.Models.Inlines;
using StreamingMarkdown.Maui.Styling;

namespace StreamingMarkdown.Maui.Rendering;

public sealed class MarkdownInlineRenderer
{
    private readonly MarkdownStyle _style;
    private readonly IMarkdownFontResolver? _fontResolver;

    public MarkdownInlineRenderer(
        MarkdownStyle? style = null,
        IMarkdownFontResolver? fontResolver = null)
    {
        _style = style ?? new MarkdownStyle();
        _fontResolver = fontResolver;
    }

    public FormattedString Render(
        IReadOnlyList<MarkdownInline> inlines,
        double? fontSize = null)
    {
        ArgumentNullException.ThrowIfNull(inlines);

        var result = new FormattedString();

        AppendInlines(
            result,
            inlines,
            FontAttributes.None,
            fontSize);

        return result;
    }

    private void AppendInlines(
        FormattedString result,
        IReadOnlyList<MarkdownInline> inlines,
        FontAttributes attributes,
        double? fontSize)
    {
        foreach (var inline in inlines)
        {
            switch (inline)
            {
                case TextInline text:
                    result.Spans.Add(
                        CreateSpan(text.Text, attributes, fontSize));
                    break;

                case BoldInline bold:
                    AppendInlines(
                        result,
                        bold.Children,
                        attributes | FontAttributes.Bold,
                        fontSize);
                    break;

                case ItalicInline italic:
                    AppendInlines(
                        result,
                        italic.Children,
                        attributes | FontAttributes.Italic,
                        fontSize);
                    break;

                case HyperlinkInline link:
                    AppendLink(
                        result,
                        link,
                        attributes,
                        fontSize);
                    break;
            }
        }
    }

    private void AppendLink(
        FormattedString result,
        HyperlinkInline link,
        FontAttributes attributes,
        double? fontSize)
    {
        var linkText = new FormattedString();

        AppendInlines(
            linkText,
            link.Children,
            attributes,
            fontSize);

        foreach (var childSpan in linkText.Spans)
        {
            childSpan.TextDecorations |= TextDecorations.Underline;

            childSpan.GestureRecognizers.Add(
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

            result.Spans.Add(childSpan);
        }
    }

    private Span CreateSpan(
        string text,
        FontAttributes attributes,
        double? fontSize)
    {
        var span = new Span
        {
            Text = text,
            FontSize = fontSize ?? _style.BodyFontSize,
            TextColor = _style.TextColor,
            LineHeight = _style.LineHeight,
            FontAttributes = attributes
        };

        var resolution = _fontResolver?.Resolve(
            _style.FontFamily,
            attributes);

        if (resolution is { } resolved &&
            !string.IsNullOrWhiteSpace(resolved.FontFamily))
        {
            span.FontFamily = resolved.FontFamily;

            if (resolved.HasDedicatedFontFace)
            {
                // The selected custom face already represents
                // the requested weight/style.
                span.FontAttributes = FontAttributes.None;
            }
            else
            {
                span.FontAttributes = attributes;
            }
        }
        else if (!string.IsNullOrWhiteSpace(_style.FontFamily))
        {
            span.FontFamily = _style.FontFamily;
        }

        return span;
    }
}