
namespace StreamingMarkdown.Maui.Styling;

public sealed class MarkdownFontResolver : IMarkdownFontResolver
{
    private readonly MarkdownFontFamilySet? _fonts;

    public MarkdownFontResolver(MarkdownFontFamilySet? fonts)
    {
        _fonts = fonts;
    }

    public MarkdownFontResolution Resolve(
        string? fontFamily,
        FontAttributes attributes)
    {
        if (_fonts is null ||
            string.IsNullOrWhiteSpace(fontFamily) ||
            !string.Equals(
                fontFamily,
                _fonts.Regular,
                StringComparison.Ordinal))
        {
            return new MarkdownFontResolution(fontFamily, false);
        }

        var bold = attributes.HasFlag(FontAttributes.Bold);
        var italic = attributes.HasFlag(FontAttributes.Italic);

        var selected = (bold, italic) switch
        {
            (true, true) => _fonts.BoldItalic,
            (true, false) => _fonts.Bold,
            (false, true) => _fonts.Italic,
            _ => _fonts.Regular
        };

        if (!string.IsNullOrWhiteSpace(selected))
        {
            return new MarkdownFontResolution(selected, true);
        }

        // Use the regular face and let the platform simulate bold/italic.
        return new MarkdownFontResolution(_fonts.Regular, false);
    }
}