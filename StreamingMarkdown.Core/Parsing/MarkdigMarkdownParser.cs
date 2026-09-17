
using System.Text;
using Markdig;

using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Core.Models.Inlines;

using MarkdigBlock = Markdig.Syntax.Block;
using MarkdigHeadingBlock = Markdig.Syntax.HeadingBlock;
using MarkdigParagraphBlock = Markdig.Syntax.ParagraphBlock;
using MarkdigQuoteBlock = Markdig.Syntax.QuoteBlock;
using MarkdigListBlock = Markdig.Syntax.ListBlock;
using MarkdigListItem = Markdig.Syntax.ListItemBlock;

using MarkdigContainerInline = Markdig.Syntax.Inlines.ContainerInline;
using MarkdigInline = Markdig.Syntax.Inlines.Inline;
using MarkdigLiteralInline = Markdig.Syntax.Inlines.LiteralInline;
using MarkdigEmphasisInline = Markdig.Syntax.Inlines.EmphasisInline;
using MarkdigLinkInline = Markdig.Syntax.Inlines.LinkInline;

namespace StreamingMarkdown.Core.Parsing;

public sealed class MarkdigMarkdownParser : IMarkdownParser
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdigMarkdownParser()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .Build();
    }

    public MarkdownDocument Parse(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);

        var markdownDocument = Markdig.Markdown.Parse(
            markdown,
            _pipeline);

        var blocks = new List<MarkdownBlock>();

        var position = 0;

        foreach (var block in markdownDocument)
        {
            var parsedBlock = ParseBlock(
                block,
                position,
                markdown);

            if (parsedBlock is not null)
            {
                blocks.Add(parsedBlock);
                position++;
            }
        }

        return new MarkdownDocument(blocks);
    }

    private static MarkdownBlock? ParseBlock(
        MarkdigBlock block,
        int position,
        string markdown)
    {
        return block switch
        {
            MarkdigHeadingBlock heading =>
                ParseHeading(
                    heading,
                    position),

            MarkdigParagraphBlock paragraph =>
                ParseParagraph(
                    paragraph,
                    position,
                    markdown),

            MarkdigQuoteBlock quote =>
                ParseQuote(
                    quote,
                    position,
                    markdown),

            MarkdigListBlock list =>
                ParseList(
                    list,
                    position,
                    markdown),

            _ => null
        };
    }

    private static HeadingBlock ParseHeading(
        MarkdigHeadingBlock heading,
        int position)
    {
        var inlines = ParseInlines(
            heading.Inline);

        return new HeadingBlock(
            position,
            heading.Level,
            inlines);
    }

    private static ParagraphBlock ParseParagraph(
        MarkdigParagraphBlock paragraph,
        int position,
        string markdown)
    {
        var sourceText =
    ExtractSourceText(
        paragraph,
        markdown);

        // Markdig does not always create a LinkInline
        // for an incomplete link such as:
        //
        // [OpenAI
        //
        // In that situation the opening '[' can be
        // discarded by the AST. Preserve the original
        // paragraph source instead.
        if (ContainsIncompleteLink(sourceText))
        {
            return new ParagraphBlock(
                position,
                new MarkdownInline[]
                {
                    new TextInline(sourceText)
                });
        }

        var inlines = ParseInlines(
            paragraph.Inline);

        return new ParagraphBlock(
            position,
            inlines);
    }

    private static QuoteBlock ParseQuote(
        MarkdigQuoteBlock quote,
        int position,
        string markdown)
    {
        var blocks = new List<MarkdownBlock>();

        var childPosition = 0;

        foreach (var child in quote)
        {
            var parsedBlock = ParseBlock(
                child,
                childPosition,
                markdown);

            if (parsedBlock is not null)
            {
                blocks.Add(parsedBlock);
                childPosition++;
            }
        }

        return new QuoteBlock(
            position,
            blocks);
    }

    private static ListBlock ParseList(
        MarkdigListBlock list,
        int position,
        string markdown)
    {
        var items = new List<ListItem>();

        foreach (var item in list)
        {
            if (item is not MarkdigListItem listItem)
            {
                continue;
            }

            var blocks = new List<MarkdownBlock>();

            var childPosition = 0;

            foreach (var child in listItem)
            {
                var parsedBlock = ParseBlock(
                    child,
                    childPosition,
                    markdown);

                if (parsedBlock is not null)
                {
                    blocks.Add(parsedBlock);
                    childPosition++;
                }
            }

            items.Add(
                new ListItem(blocks));
        }

        return new ListBlock(
            position,
            list.IsOrdered,
            items);
    }

    private static IReadOnlyList<MarkdownInline> ParseInlines(
        MarkdigContainerInline? container)
    {
        if (container is null)
        {
            return [];
        }

        var result = new List<MarkdownInline>();

        foreach (var inline in container)
        {
            var parsedInline = ParseInline(
                inline);

            if (parsedInline is not null)
            {
                result.Add(parsedInline);
            }
        }

        return result;
    }

    private static MarkdownInline? ParseInline(
        MarkdigInline inline)
    {
        return inline switch
        {
            MarkdigLiteralInline literal =>
                new TextInline(
                    literal.Content.ToString()),

            MarkdigEmphasisInline emphasis =>
                ParseEmphasis(emphasis),

            MarkdigLinkInline link =>
                ParseLink(link),

            _ => CreateFallbackInline(inline)
        };
    }

    private static MarkdownInline ParseEmphasis(
        MarkdigEmphasisInline emphasis)
    {
        var children = ParseInlines(
            emphasis);

        return emphasis.DelimiterCount switch
        {
            1 => new ItalicInline(children),

            2 => new BoldInline(children),

            _ => new TextInline(
                ExtractInlineText(emphasis))
        };
    }

    private static MarkdownInline ParseLink(
        MarkdigLinkInline link)
    {
        var children = ParseInlines(
            link);

        if (link.Url is null)
        {
            var text =
                ExtractCoreInlineText(children);

            return new TextInline(
                "[" + text);
        }

        return new HyperlinkInline(
            link.Url,
            children);
    }

    private static MarkdownInline? CreateFallbackInline(
        MarkdigInline inline)
    {
        var text =
            ExtractSourceText(inline);

        return text.Length == 0
            ? null
            : new TextInline(text);
    }

    private static string ExtractSourceText(
        MarkdigInline inline)
    {
        if (inline is MarkdigLiteralInline literal)
        {
            return literal.Content.ToString();
        }

        if (inline is MarkdigContainerInline container)
        {
            var result = new StringBuilder();

            foreach (var child in container)
            {
                result.Append(
                    ExtractSourceText(child));
            }

            return result.ToString();
        }

        return inline.ToString() ?? string.Empty;
    }

    private static string ExtractSourceText(
        MarkdigBlock block,
        string markdown)
    {
        var span = block.Span;

        if (span.Start < 0 ||
            span.End < span.Start ||
            span.Start >= markdown.Length)
        {
            return string.Empty;
        }

        var end =
            Math.Min(
                span.End,
                markdown.Length - 1);

        return markdown[
            span.Start..(end + 1)];
    }

    private static bool ContainsIncompleteLink(
        string source)
    {
        if (string.IsNullOrEmpty(source))
        {
            return false;
        }

        var openingBracket =
            source.LastIndexOf('[');

        if (openingBracket < 0)
        {
            return false;
        }

        var closingBracket =
            source.IndexOf(
                ']',
                openingBracket + 1);

        // There is an '[' without a matching ']'.
        return closingBracket < 0;
    }

    private static string ExtractCoreInlineText(
        IReadOnlyList<MarkdownInline> inlines)
    {
        var result = new StringBuilder();

        foreach (var inline in inlines)
        {
            switch (inline)
            {
                case TextInline text:
                    result.Append(text.Text);
                    break;

                case BoldInline bold:
                    result.Append(
                        ExtractCoreInlineText(
                            bold.Children));
                    break;

                case ItalicInline italic:
                    result.Append(
                        ExtractCoreInlineText(
                            italic.Children));
                    break;

                case HyperlinkInline link:
                    result.Append(
                        ExtractCoreInlineText(
                            link.Children));
                    break;
            }
        }

        return result.ToString();
    }

    private static string ExtractInlineText(
        MarkdigContainerInline container)
    {
        var result = new StringBuilder();

        foreach (var inline in container)
        {
            if (inline is MarkdigLiteralInline literal)
            {
                result.Append(
                    literal.Content);
            }
            else if (inline is MarkdigContainerInline child)
            {
                result.Append(
                    ExtractInlineText(child));
            }
        }

        return result.ToString();
    }
}
