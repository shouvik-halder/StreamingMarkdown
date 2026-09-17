using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Core.Models.Inlines;
using StreamingMarkdown.Core.Parsing;

namespace StreamingMarkdown.Core.Tests.Parsing;

public sealed class QuoteParserTests
{
    private readonly IMarkdownParser _parser =
        new MarkdigMarkdownParser();

    [Fact]
    public void Parse_ShouldCreateQuoteBlock()
    {
        // Arrange
        const string markdown =
            "> The water condition is good.";

        // Act
        var document =
            _parser.Parse(markdown);

        // Assert
        Assert.Single(document.Blocks);

        var quote =
            Assert.IsType<QuoteBlock>(
                document.Blocks[0]);

        Assert.Single(quote.Blocks);

        var paragraph =
            Assert.IsType<ParagraphBlock>(
                quote.Blocks[0]);

        var text =
            Assert.IsType<TextInline>(
                paragraph.Inlines[0]);

        Assert.Equal(
            "The water condition is good.",
            text.Text);
    }
}