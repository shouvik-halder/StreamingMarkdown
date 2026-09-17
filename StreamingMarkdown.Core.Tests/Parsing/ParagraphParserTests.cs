using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Core.Models.Inlines;
using StreamingMarkdown.Core.Parsing;

namespace StreamingMarkdown.Core.Tests.Parsing;

public sealed class ParagraphParserTests
{
    private readonly IMarkdownParser _parser =
        new MarkdigMarkdownParser();

    [Fact]
    public void Parse_ShouldCreateParagraphBlock()
    {
        // Arrange
        const string markdown =
            "Current water condition is good.";

        // Act
        var document =
            _parser.Parse(markdown);

        // Assert
        Assert.Single(document.Blocks);

        var paragraph =
            Assert.IsType<ParagraphBlock>(
                document.Blocks[0]);

        Assert.Single(paragraph.Inlines);

        var text =
            Assert.IsType<TextInline>(
                paragraph.Inlines[0]);

        Assert.Equal(
            "Current water condition is good.",
            text.Text);
    }
}