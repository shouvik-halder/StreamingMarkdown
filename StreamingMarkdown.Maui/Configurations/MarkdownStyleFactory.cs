using StreamingMarkdown.Maui.Styling;

namespace StreamingMarkdown.Maui.Configuration;

internal static class MarkdownStyleFactory
{
    public static MarkdownStyle Create(
        StreamingMarkdownOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new MarkdownStyle
        {
            FontFamily = options.FontFamily,
            BodyFontSize = options.FontSize,
            TextColor = options.TextColor,

            Heading1FontSize = options.HeadingFontSize,
            Heading2FontSize = options.Heading2FontSize,
            Heading3FontSize = options.Heading3FontSize,
            Heading4FontSize = options.Heading4FontSize,
            Heading5FontSize = options.Heading5FontSize,
            Heading6FontSize = options.Heading6FontSize,

            BlockSpacing = options.BlockSpacing,
            ListIndent = options.ListIndent,
            QuoteIndent = options.QuoteIndent
        };
    }
}