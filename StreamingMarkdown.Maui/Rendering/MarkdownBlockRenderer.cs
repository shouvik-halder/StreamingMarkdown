using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Maui.Rendering.Views;
using StreamingMarkdown.Maui.Styling;
using ListView = StreamingMarkdown.Maui.Rendering.Views.ListView;

namespace StreamingMarkdown.Maui.Rendering;

public sealed class MarkdownBlockRenderer
{
    private readonly MarkdownStyle _style;

    public MarkdownBlockRenderer(
        MarkdownStyle? style = null)
    {
        _style =
            style ?? new MarkdownStyle();
    }

    internal IMarkdownBlockView? Render(
        MarkdownBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);

        return block switch
{
    HeadingBlock heading =>
        new HeadingView(heading, _style),

    ParagraphBlock paragraph =>
        new ParagraphView(paragraph, _style),

    ListBlock list =>
        new ListView(list, _style),

    QuoteBlock quote =>
        new QuoteView(quote, _style),

    _ => null
};
    }
}