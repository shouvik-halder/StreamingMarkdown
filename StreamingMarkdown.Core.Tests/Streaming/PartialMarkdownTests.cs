
using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Core.Models.Inlines;
using StreamingMarkdown.Core.Parsing;

namespace StreamingMarkdown.Core.Tests.Streaming;

public sealed class PartialMarkdownTests
{
    private readonly IMarkdownParser _parser =
        new MarkdigMarkdownParser();

    [Fact]
    public void Parse_WhenBoldSyntaxIsIncomplete_ShouldKeepText()
    {
        var document =
            _parser.Parse("**Hello");

        var paragraph =
            Assert.IsType<ParagraphBlock>(
                Assert.Single(document.Blocks));

        Assert.Equal(
            2,
            paragraph.Inlines.Count);

        var first =
            Assert.IsType<TextInline>(
                paragraph.Inlines[0]);

        var second =
            Assert.IsType<TextInline>(
                paragraph.Inlines[1]);

        Assert.Equal(
            "**",
            first.Text);

        Assert.Equal(
            "Hello",
            second.Text);
    }

    [Fact]
    public void Parse_WhenBoldSyntaxBecomesComplete_ShouldCreateBoldInline()
    {
        var document =
            _parser.Parse("**Hello**");

        var paragraph =
            Assert.IsType<ParagraphBlock>(
                Assert.Single(document.Blocks));

        var inline =
            Assert.Single(paragraph.Inlines);

        var bold =
            Assert.IsType<BoldInline>(inline);

        var text =
            Assert.IsType<TextInline>(
                Assert.Single(bold.Children));

        Assert.Equal(
            "Hello",
            text.Text);
    }

    [Fact]
    public void Parse_WhenItalicSyntaxIsIncomplete_ShouldKeepText()
    {
        var document =
            _parser.Parse("*Hello");

        var paragraph =
            Assert.IsType<ParagraphBlock>(
                Assert.Single(document.Blocks));

        Assert.Equal(
            2,
            paragraph.Inlines.Count);

        var first =
            Assert.IsType<TextInline>(
                paragraph.Inlines[0]);

        var second =
            Assert.IsType<TextInline>(
                paragraph.Inlines[1]);

        Assert.Equal(
            "*",
            first.Text);

        Assert.Equal(
            "Hello",
            second.Text);
    }

    [Fact]
    public void Parse_WhenLinkSyntaxIsIncomplete_ShouldKeepText()
    {
        var document =
            _parser.Parse("[OpenAI");

        var paragraph =
            Assert.IsType<ParagraphBlock>(
                Assert.Single(document.Blocks));

        var inline =
            Assert.Single(paragraph.Inlines);

        var text =
            Assert.IsType<TextInline>(inline);

        Assert.Equal(
            "[OpenAI",
            text.Text);
    }

    [Fact]
    public void Parse_WhenLinkSyntaxBecomesComplete_ShouldCreateLink()
    {
        var document =
            _parser.Parse(
                "[OpenAI](https://openai.com)");

        var paragraph =
            Assert.IsType<ParagraphBlock>(
                Assert.Single(document.Blocks));

        var inline =
            Assert.Single(paragraph.Inlines);

        var link =
            Assert.IsType<HyperlinkInline>(inline);

        Assert.Equal(
            "https://openai.com",
            link.Url);

        var text =
            Assert.IsType<TextInline>(
                Assert.Single(link.Children));

        Assert.Equal(
            "OpenAI",
            text.Text);
    }
}