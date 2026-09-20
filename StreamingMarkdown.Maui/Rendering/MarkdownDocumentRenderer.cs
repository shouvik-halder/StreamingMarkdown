using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Maui.Rendering.Views;
using StreamingMarkdown.Maui.Styling;

namespace StreamingMarkdown.Maui.Rendering;

public sealed class MarkdownDocumentRenderer
{
    private readonly MarkdownBlockRenderer _blockRenderer;

    public MarkdownDocumentRenderer(
        MarkdownStyle? style = null)
    {
        _blockRenderer =
            new MarkdownBlockRenderer(
                style);
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
            var blockView =
                RenderBlock(block);

            if (blockView is not null)
            {
                layout.Children.Add(
                    blockView.View);
            }
        }

        return layout;
    }

    internal IMarkdownBlockView? RenderBlock(
        MarkdownBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);

        return _blockRenderer.Render(
            block);
    }
}