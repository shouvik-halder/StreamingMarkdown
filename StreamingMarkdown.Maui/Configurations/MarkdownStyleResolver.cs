using StreamingMarkdown.Maui.Styling;

namespace StreamingMarkdown.Maui.Configuration;

internal static class MarkdownStyleResolver
{
    public static MarkdownStyle Resolve(
        StreamingMarkdownOptions options,
        MarkdownStyle? inlineStyle)
    {
        ArgumentNullException.ThrowIfNull(options);

        var defaults = new MarkdownStyle();

        return new MarkdownStyle
        {
            FontFamily = inlineStyle?.HasFontFamily == true
                ? inlineStyle.FontFamily
                : options.FontFamily ?? defaults.FontFamily,

            TextColor = inlineStyle?.HasTextColor == true
                ? inlineStyle.TextColor
                : options.TextColor ?? defaults.TextColor,

            BodyFontSize = inlineStyle?.HasBodyFontSize == true
                ? inlineStyle.BodyFontSize
                : options.FontSize ?? defaults.BodyFontSize,

            LineHeight = inlineStyle?.HasLineHeight == true
                ? inlineStyle.LineHeight
                : options.LineHeight ?? defaults.LineHeight,

            Heading1FontSize = inlineStyle?.HasHeading1FontSize == true
                ? inlineStyle.Heading1FontSize
                : options.HeadingFontSize ?? defaults.Heading1FontSize,

            Heading2FontSize = inlineStyle?.HasHeading2FontSize == true
                ? inlineStyle.Heading2FontSize
                : options.Heading2FontSize ?? defaults.Heading2FontSize,

            Heading3FontSize = inlineStyle?.HasHeading3FontSize == true
                ? inlineStyle.Heading3FontSize
                : options.Heading3FontSize ?? defaults.Heading3FontSize,

            Heading4FontSize = inlineStyle?.HasHeading4FontSize == true
                ? inlineStyle.Heading4FontSize
                : options.Heading4FontSize ?? defaults.Heading4FontSize,

            Heading5FontSize = inlineStyle?.HasHeading5FontSize == true
                ? inlineStyle.Heading5FontSize
                : options.Heading5FontSize ?? defaults.Heading5FontSize,

            Heading6FontSize = inlineStyle?.HasHeading6FontSize == true
                ? inlineStyle.Heading6FontSize
                : options.Heading6FontSize ?? defaults.Heading6FontSize,

            BlockSpacing = inlineStyle?.HasBlockSpacing == true
                ? inlineStyle.BlockSpacing
                : options.BlockSpacing ?? defaults.BlockSpacing,

            ListIndent = inlineStyle?.HasListIndent == true
                ? inlineStyle.ListIndent
                : options.ListIndent ?? defaults.ListIndent,

            QuoteIndent = inlineStyle?.HasQuoteIndent == true
                ? inlineStyle.QuoteIndent
                : options.QuoteIndent ?? defaults.QuoteIndent
        };
    }
}