using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Maui.Styling;

namespace StreamingMarkdown.Maui.Rendering.Views;

internal sealed class ParagraphView : IMarkdownBlockView
{
    private readonly Label _label;
    private readonly MarkdownInlineRenderer _inlineRenderer;
    private readonly MarkdownStyle _style;

    public Guid BlockId { get; private set; }

    public View View => _label;

    public ParagraphView(
        ParagraphBlock block,
        MarkdownStyle? style = null)
    {
        ArgumentNullException.ThrowIfNull(block);

        _style =
            style ?? new MarkdownStyle();

        _inlineRenderer =
            new MarkdownInlineRenderer(
                _style);

        _label =
            new Label
            {
                FontSize =
                    _style.BodyFontSize,

                LineBreakMode =
                    LineBreakMode.WordWrap,

                Margin =
                    new Thickness(
                        0,
                        0,
                        0,
                        _style.BlockSpacing)
            };

        Update(block);
    }

    public void Update(
        MarkdownBlock block)
    {
        if (block is not ParagraphBlock paragraph)
        {
            throw new ArgumentException(
                "Block must be a ParagraphBlock.",
                nameof(block));
        }

        BlockId =
            paragraph.Id;

        _label.FormattedText =
            _inlineRenderer.Render(
                paragraph.Inlines);
    }
}