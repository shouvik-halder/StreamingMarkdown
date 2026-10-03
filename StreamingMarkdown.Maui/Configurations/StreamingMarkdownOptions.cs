namespace StreamingMarkdown.Maui.Configuration;

public sealed class StreamingMarkdownOptions
{
    public string? FontFamily { get; set; }

    public Color TextColor { get; set; } = Color.FromArgb("#1E293B");

    public double FontSize { get; set; } = 16;

    public double HeadingFontSize { get; set; } = 32;

    public double Heading2FontSize { get; set; } = 28;
    public double Heading3FontSize { get; set; } = 24;
    public double Heading4FontSize { get; set; } = 20;
    public double Heading5FontSize { get; set; } = 18;
    public double Heading6FontSize { get; set; } = 16;

    public double BlockSpacing { get; set; } = 8;
    public double ListIndent { get; set; } = 20;
    public double QuoteIndent { get; set; } = 16;
}