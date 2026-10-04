
using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Maui.Rendering.Views;
using StreamingMarkdown.Maui.Styling;
using ListView = StreamingMarkdown.Maui.Rendering.Views.ListView;

namespace StreamingMarkdown.Maui.Rendering;

public sealed class MarkdownBlockRenderer
{
    private readonly MarkdownStyle _style;
    private readonly IMarkdownFontResolver? _fontResolver;

    public MarkdownBlockRenderer(
        MarkdownStyle? style = null,
        IMarkdownFontResolver? fontResolver = null)
    {
        _style = style ?? new MarkdownStyle();
        _fontResolver = fontResolver;
    }

    
internal IMarkdownBlockView? Render(MarkdownBlock block)
{
    ArgumentNullException.ThrowIfNull(block);

    return block switch
    {
        HeadingBlock heading =>
            new HeadingView(heading, _style, _fontResolver),

        ParagraphBlock paragraph =>
            new ParagraphView(paragraph, _style, _fontResolver),

        ListBlock list =>
            new ListView(list, _style, _fontResolver),

        QuoteBlock quote =>
            new QuoteView(quote, _style, _fontResolver),

        _ => null
    };
}
}