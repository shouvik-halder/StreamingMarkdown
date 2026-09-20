using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Maui.Styling;

namespace StreamingMarkdown.Maui.Rendering.Views;

internal sealed class HeadingView : IMarkdownBlockView
{
    private readonly Label _label;
    private readonly MarkdownInlineRenderer _inlineRenderer;
    private readonly MarkdownStyle _style;

    public Guid BlockId { get; private set; }

    public View View => _label;

    public HeadingView(
        HeadingBlock block,
        MarkdownStyle? style = null)
    {
        ArgumentNullException.ThrowIfNull(block);

        _style = style ?? new MarkdownStyle();

        _inlineRenderer =
            new MarkdownInlineRenderer(_style);

        _label = new Label
        {
            LineBreakMode = LineBreakMode.WordWrap,
            Margin = new Thickness(
                0,
                0,
                0,
                _style.BlockSpacing)
        };

        Update(block);
    }

    public void Update(MarkdownBlock block)
    {
        if (block is not HeadingBlock heading)
        {
            throw new ArgumentException(
                "Block must be a HeadingBlock.",
                nameof(block));
        }

        BlockId = heading.Id;

        _label.FontSize =
            GetFontSize(heading.Level);

        _label.FormattedText =
            _inlineRenderer.Render(heading.Inlines);
    }

    private double GetFontSize(int level)
    {
        return level switch
        {
            1 => _style.Heading1FontSize,
            2 => _style.Heading2FontSize,
            3 => _style.Heading3FontSize,
            4 => _style.Heading4FontSize,
            5 => _style.Heading5FontSize,
            6 => _style.Heading6FontSize,
            _ => _style.BodyFontSize
        };
    }
}