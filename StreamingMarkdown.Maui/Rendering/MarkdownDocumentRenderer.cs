using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Maui.Styling;

namespace StreamingMarkdown.Maui.Rendering;

public sealed class MarkdownDocumentRenderer
{
    private readonly MarkdownBlockRenderer _blockRenderer;

    public MarkdownDocumentRenderer(
        MarkdownStyle? style = null)
    {
        _blockRenderer =
            new MarkdownBlockRenderer(style);
    }

    public View Render(
        MarkdownDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var layout =
            new VerticalStackLayout
            {
                Spacing = 0
            };

        foreach (var block in document.Blocks)
        {
            var view =
                RenderBlock(block);

            if (view is not null)
            {
                layout.Children.Add(view);
            }
        }

        return layout;
    }

    public View? RenderBlock(
        MarkdownBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);

        return _blockRenderer.Render(block);
    }
}