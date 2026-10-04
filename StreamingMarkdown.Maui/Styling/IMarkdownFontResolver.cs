
namespace StreamingMarkdown.Maui.Styling;

public interface IMarkdownFontResolver
{
    MarkdownFontResolution Resolve(
        string? fontFamily,
        FontAttributes attributes);
}