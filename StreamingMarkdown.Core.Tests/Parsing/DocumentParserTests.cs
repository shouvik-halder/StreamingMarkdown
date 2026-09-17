using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Core.Models.Inlines;
using StreamingMarkdown.Core.Parsing;

namespace StreamingMarkdown.Core.Tests.Parsing;

public sealed class DocumentParserTests
{
    private readonly IMarkdownParser _parser =
        new MarkdigMarkdownParser();

    [Fact]
    public void Parse_ShouldCreateCompleteDocument()
    {
        // Arrange
        const string markdown = """
            # Pool Status

            Current water condition is **Good**.

            - Temperature: 25°C
            - PH: 7.2

            > Water quality is within normal range.

            Visit [OpenAI](https://openai.com).
            """;

        // Act
        var document =
            _parser.Parse(markdown);

        // Assert
        Assert.Equal(
            5,
            document.Blocks.Count);

        Assert.IsType<HeadingBlock>(
            document.Blocks[0]);

        Assert.IsType<ParagraphBlock>(
            document.Blocks[1]);

        Assert.IsType<ListBlock>(
            document.Blocks[2]);

        Assert.IsType<QuoteBlock>(
            document.Blocks[3]);

        Assert.IsType<ParagraphBlock>(
            document.Blocks[4]);
    }
}