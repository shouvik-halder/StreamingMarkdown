using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Core.Models.Inlines;
using StreamingMarkdown.Core.Parsing;

namespace StreamingMarkdown.Core.Tests.Parsing;

public sealed class InlineParserTests
{
    private readonly IMarkdownParser _parser =
        new MarkdigMarkdownParser();

    [Fact]
    public void Parse_ShouldCreateBoldInline()
    {
        // Arrange
        const string markdown =
            "Water condition is **Good**.";

        // Act
        var document =
            _parser.Parse(markdown);

        // Assert
        var paragraph =
            Assert.IsType<ParagraphBlock>(
                document.Blocks[0]);

        Assert.Equal(
            3,
            paragraph.Inlines.Count);

        Assert.IsType<TextInline>(
            paragraph.Inlines[0]);

        var bold =
            Assert.IsType<BoldInline>(
                paragraph.Inlines[1]);

        Assert.Single(bold.Children);

        var text =
            Assert.IsType<TextInline>(
                bold.Children[0]);

        Assert.Equal(
            "Good",
            text.Text);
    }
    [Fact]
    public void Parse_ShouldCreateItalicInline()
    {
        // Arrange
        const string markdown =
            "Water is *clean*.";

        // Act
        var document =
            _parser.Parse(markdown);

        // Assert
        var paragraph =
            Assert.IsType<ParagraphBlock>(
                document.Blocks[0]);

        var italic =
            Assert.IsType<ItalicInline>(
                paragraph.Inlines[1]);

        Assert.Single(italic.Children);

        var text =
            Assert.IsType<TextInline>(
                italic.Children[0]);

        Assert.Equal(
            "clean",
            text.Text);
    }
    [Fact]
    public void Parse_ShouldCreateHyperlinkInline()
    {
        // Arrange
        const string markdown =
            "Visit [OpenAI](https://openai.com).";

        // Act
        var document =
            _parser.Parse(markdown);

        // Assert
        var paragraph =
            Assert.IsType<ParagraphBlock>(
                document.Blocks[0]);

        var link =
            Assert.IsType<HyperlinkInline>(
                paragraph.Inlines[1]);

        Assert.Equal(
            "https://openai.com",
            link.Url);

        Assert.Single(link.Children);

        var text =
            Assert.IsType<TextInline>(
                link.Children[0]);

        Assert.Equal(
            "OpenAI",
            text.Text);
    }
    [Fact]
    public void Parse_ShouldSupportNestedInlineElements()
    {
        // Arrange
        const string markdown =
            "This is **very *important***.";

        // Act
        var document =
            _parser.Parse(markdown);

        // Assert
        var paragraph =
            Assert.IsType<ParagraphBlock>(
                document.Blocks[0]);

        var bold =
            Assert.IsType<BoldInline>(
                paragraph.Inlines[1]);

        Assert.Equal(
            2,
            bold.Children.Count);

        var text =
            Assert.IsType<TextInline>(
                bold.Children[0]);

        Assert.Equal(
            "very ",
            text.Text);

        var italic =
            Assert.IsType<ItalicInline>(
                bold.Children[1]);

        var italicText =
            Assert.IsType<TextInline>(
                italic.Children[0]);

        Assert.Equal(
            "important",
            italicText.Text);
    }
}