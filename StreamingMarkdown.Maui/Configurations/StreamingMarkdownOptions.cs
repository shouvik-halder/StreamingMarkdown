namespace StreamingMarkdown.Maui.Configuration;

public sealed class StreamingMarkdownOptions
{
    public string? FontFamily { get; set; }

    public Color? TextColor { get; set; }

    public double? FontSize { get; set; }

    public double? LineHeight { get; set; }

    public double? HeadingFontSize { get; set; }
    public double? Heading2FontSize { get; set; }
    public double? Heading3FontSize { get; set; }
    public double? Heading4FontSize { get; set; }
    public double? Heading5FontSize { get; set; }
    public double? Heading6FontSize { get; set; }

    public double? BlockSpacing { get; set; }
    public double? ListIndent { get; set; }
    public double? QuoteIndent { get; set; }
}