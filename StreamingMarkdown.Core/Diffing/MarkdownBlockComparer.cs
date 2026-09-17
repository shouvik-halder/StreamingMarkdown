using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Core.Models.Inlines;

namespace StreamingMarkdown.Core.Diffing;

internal static class MarkdownBlockComparer
{
    public static bool AreEquivalent(
        MarkdownBlock previous,
        MarkdownBlock current)
    {
        if (previous.GetType() != current.GetType())
        {
            return false;
        }

        return previous switch
        {
            HeadingBlock previousHeading
                when current is HeadingBlock currentHeading =>
                    AreHeadingEquivalent(
                        previousHeading,
                        currentHeading),

            ParagraphBlock previousParagraph
                when current is ParagraphBlock currentParagraph =>
                    AreParagraphEquivalent(
                        previousParagraph,
                        currentParagraph),

            ListBlock previousList
                when current is ListBlock currentList =>
                    AreListEquivalent(
                        previousList,
                        currentList),

            QuoteBlock previousQuote
                when current is QuoteBlock currentQuote =>
                    AreQuoteEquivalent(
                        previousQuote,
                        currentQuote),

            _ => false
        };
    }

    private static bool AreHeadingEquivalent(
        HeadingBlock previous,
        HeadingBlock current)
    {
        return previous.Level == current.Level &&
               AreInlinesEquivalent(
                   previous.Inlines,
                   current.Inlines);
    }

    private static bool AreParagraphEquivalent(
        ParagraphBlock previous,
        ParagraphBlock current)
    {
        return AreInlinesEquivalent(
            previous.Inlines,
            current.Inlines);
    }

    private static bool AreListEquivalent(
        ListBlock previous,
        ListBlock current)
    {
        if (previous.IsOrdered != current.IsOrdered)
        {
            return false;
        }

        if (previous.Items.Count != current.Items.Count)
        {
            return false;
        }

        for (var i = 0; i < previous.Items.Count; i++)
        {
            if (!AreListItemsEquivalent(
                    previous.Items[i],
                    current.Items[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AreListItemsEquivalent(
        ListItem previous,
        ListItem current)
    {
        if (previous.Blocks.Count !=
            current.Blocks.Count)
        {
            return false;
        }

        for (var i = 0; i < previous.Blocks.Count; i++)
        {
            if (!AreEquivalent(
                    previous.Blocks[i],
                    current.Blocks[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AreQuoteEquivalent(
        QuoteBlock previous,
        QuoteBlock current)
    {
        if (previous.Blocks.Count !=
            current.Blocks.Count)
        {
            return false;
        }

        for (var i = 0; i < previous.Blocks.Count; i++)
        {
            if (!AreEquivalent(
                    previous.Blocks[i],
                    current.Blocks[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AreInlinesEquivalent(
        IReadOnlyList<MarkdownInline> previous,
        IReadOnlyList<MarkdownInline> current)
    {
        if (previous.Count != current.Count)
        {
            return false;
        }

        for (var i = 0; i < previous.Count; i++)
        {
            if (!AreInlineEquivalent(
                    previous[i],
                    current[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AreInlineEquivalent(
        MarkdownInline previous,
        MarkdownInline current)
    {
        if (previous.GetType() != current.GetType())
        {
            return false;
        }

        return previous switch
        {
            TextInline previousText
                when current is TextInline currentText =>
                    previousText.Text == currentText.Text,

            BoldInline previousBold
                when current is BoldInline currentBold =>
                    AreInlinesEquivalent(
                        previousBold.Children,
                        currentBold.Children),

            ItalicInline previousItalic
                when current is ItalicInline currentItalic =>
                    AreInlinesEquivalent(
                        previousItalic.Children,
                        currentItalic.Children),

            HyperlinkInline previousLink
                when current is HyperlinkInline currentLink =>
                    previousLink.Url == currentLink.Url &&
                    AreInlinesEquivalent(
                        previousLink.Children,
                        currentLink.Children),

            _ => false
        };
    }
}