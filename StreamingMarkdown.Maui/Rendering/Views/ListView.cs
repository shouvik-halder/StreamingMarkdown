
using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Maui.Styling;

namespace StreamingMarkdown.Maui.Rendering.Views;

internal sealed class ListView : IMarkdownBlockView
{
    private readonly VerticalStackLayout _layout;
    private readonly MarkdownInlineRenderer _inlineRenderer;
    private readonly MarkdownStyle _style;

    public Guid BlockId { get; private set; }

    public View View => _layout;

    public ListView(
        ListBlock block,
        MarkdownStyle? style = null,
        IMarkdownFontResolver? fontResolver = null)
    {
        ArgumentNullException.ThrowIfNull(block);

        _style = style ?? new MarkdownStyle();

        _inlineRenderer =
            new MarkdownInlineRenderer(_style, fontResolver);

        _layout = new VerticalStackLayout
        {
            Spacing = 4,
            Margin = new Thickness(
                _style.ListIndent,
                0,
                0,
                _style.BlockSpacing)
        };

        Update(block);
    }

    public void Update(MarkdownBlock block)
    {
        if (block is not ListBlock list)
        {
            throw new ArgumentException(
                "Block must be a ListBlock.",
                nameof(block));
        }

        BlockId = list.Id;
        _layout.Children.Clear();

        for (var index = 0; index < list.Items.Count; index++)
        {
            var itemView = CreateItemView(
                list.Items[index],
                index,
                list.IsOrdered);

            if (itemView is not null)
                _layout.Children.Add(itemView);
        }
    }

    private View? CreateItemView(
        ListItem item,
        int index,
        bool isOrdered)
    {
        foreach (var block in item.Blocks)
        {
            if (block is not ParagraphBlock paragraph)
                continue;

            var label = new Label
            {
                FontFamily = _style.FontFamily,
                FontSize = _style.BodyFontSize,
                LineBreakMode = LineBreakMode.WordWrap,
                TextColor = _style.TextColor,
                LineHeight = _style.LineHeight
            };

            var formattedText = new FormattedString();

            var prefix = isOrdered
                ? $"{index + 1}. "
                : "• ";

            formattedText.Spans.Add(new Span
            {
                Text = prefix,
                FontFamily = _style.FontFamily,
                FontSize = _style.BodyFontSize,
                TextColor = _style.TextColor,
                LineHeight = _style.LineHeight
            });

            var content = _inlineRenderer.Render(paragraph.Inlines);

            foreach (var span in content.Spans)
                formattedText.Spans.Add(span);

            label.FormattedText = formattedText;

            return label;
        }

        return null;
    }
}