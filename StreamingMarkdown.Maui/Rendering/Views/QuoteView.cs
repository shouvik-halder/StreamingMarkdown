using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Maui.Styling;

namespace StreamingMarkdown.Maui.Rendering.Views;

internal sealed class QuoteView : IMarkdownBlockView
{
    private readonly VerticalStackLayout _layout;
    private readonly MarkdownInlineRenderer _inlineRenderer;
    private readonly MarkdownStyle _style;

    public Guid BlockId { get; private set; }

    public View View => _layout;

    public QuoteView(
        QuoteBlock block,
        MarkdownStyle? style = null)
    {
        ArgumentNullException.ThrowIfNull(block);

        _style = style ?? new MarkdownStyle();

        _inlineRenderer =
            new MarkdownInlineRenderer(_style);

        _layout = new VerticalStackLayout
        {
            Spacing = 4,
            Padding = new Thickness(
                12,
                4,
                8,
                4),
            Margin = new Thickness(
                _style.QuoteIndent,
                0,
                0,
                _style.BlockSpacing)
        };

        Update(block);
    }

    public void Update(MarkdownBlock block)
    {
        if (block is not QuoteBlock quote)
        {
            throw new ArgumentException(
                "Block must be a QuoteBlock.",
                nameof(block));
        }

        BlockId = quote.Id;

        _layout.Children.Clear();

        foreach (var childBlock in quote.Blocks)
        {
            if (childBlock is not ParagraphBlock paragraph)
            {
                continue;
            }

            var label = new Label
            {
                LineBreakMode =
                    LineBreakMode.WordWrap,
                FormattedText =
                    _inlineRenderer.Render(
                        paragraph.Inlines)
            };

            _layout.Children.Add(label);
        }
    }
}