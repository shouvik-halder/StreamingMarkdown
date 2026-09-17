using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Maui.Styling;

namespace StreamingMarkdown.Maui.Rendering;

public sealed class MarkdownBlockRenderer
{
    private readonly MarkdownInlineRenderer _inlineRenderer;
    private readonly MarkdownStyle _style;

    public MarkdownBlockRenderer(
        MarkdownStyle? style = null)
    {
        _style = style ?? new MarkdownStyle();

        _inlineRenderer =
            new MarkdownInlineRenderer(_style);
    }

    public View? Render(
        MarkdownBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);

        return block switch
        {
            HeadingBlock heading =>
                RenderHeading(heading),

            ParagraphBlock paragraph =>
                RenderParagraph(paragraph),

            ListBlock list =>
                RenderList(list),

            QuoteBlock quote =>
                RenderQuote(quote),

            _ => null
        };
    }

    private View RenderHeading(
        HeadingBlock heading)
    {
        return new Label
        {
            FormattedText =
                _inlineRenderer.Render(
                    heading.Inlines),

            FontSize =
                GetHeadingFontSize(
                    heading.Level),

            FontAttributes =
                FontAttributes.Bold,

            Margin = new Thickness(
                0,
                _style.BlockSpacing,
                0,
                _style.BlockSpacing)
        };
    }

    private View RenderParagraph(
        ParagraphBlock paragraph)
    {
        return new Label
        {
            FormattedText =
                _inlineRenderer.Render(
                    paragraph.Inlines),

            FontSize =
                _style.BodyFontSize,

            LineBreakMode =
                LineBreakMode.WordWrap,

            Margin = new Thickness(
                0,
                0,
                0,
                _style.BlockSpacing)
        };
    }

    private View RenderList(
        ListBlock list)
    {
        var layout =
            new VerticalStackLayout
            {
                Spacing = 4,
                Margin = new Thickness(
                    _style.ListIndent,
                    0,
                    0,
                    _style.BlockSpacing)
            };

        for (var i = 0;
             i < list.Items.Count;
             i++)
        {
            var item =
                list.Items[i];

            layout.Children.Add(
                RenderListItem(
                    list,
                    item,
                    i));
        }

        return layout;
    }

    private View RenderListItem(
        ListBlock list,
        ListItem item,
        int index)
    {
        var row =
            new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(
                        GridLength.Auto),

                    new ColumnDefinition(
                        GridLength.Star)
                }
            };

        var marker =
            new Label
            {
                Text =
                    list.IsOrdered
                        ? $"{index + 1}."
                        : "•",

                FontSize =
                    _style.BodyFontSize,

                Margin =
                    new Thickness(
                        0,
                        0,
                        8,
                        0)
            };

        Grid.SetColumn(
            marker,
            0);

        row.Children.Add(marker);

        var content =
            new VerticalStackLayout
            {
                Spacing = 4
            };

        foreach (var block in item.Blocks)
        {
            var view =
                Render(block);

            if (view is not null)
            {
                content.Children.Add(view);
            }
        }

        Grid.SetColumn(
            content,
            1);

        row.Children.Add(content);

        return row;
    }

    private View RenderQuote(
        QuoteBlock quote)
    {
        var container =
            new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(
                        new GridLength(
                            4,
                            GridUnitType.Absolute)),

                    new ColumnDefinition(
                        GridLength.Star)
                },

                Margin =
                    new Thickness(
                        _style.QuoteIndent,
                        4,
                        0,
                        _style.BlockSpacing)
            };

        var indicator =
            new BoxView
            {
                Color =
                    Colors.Gray
            };

        Grid.SetColumn(
            indicator,
            0);

        container.Children.Add(
            indicator);

        var content =
            new VerticalStackLayout
            {
                Spacing = 4,
                Padding = new Thickness(
                    12,
                    0,
                    0,
                    0)
            };

        foreach (var block in quote.Blocks)
        {
            var view =
                Render(block);

            if (view is not null)
            {
                content.Children.Add(view);
            }
        }

        Grid.SetColumn(
            content,
            1);

        container.Children.Add(
            content);

        return container;
    }

    private double GetHeadingFontSize(
        int level)
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