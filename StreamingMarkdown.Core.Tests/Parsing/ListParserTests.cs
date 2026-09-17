using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Core.Models.Inlines;
using StreamingMarkdown.Core.Parsing;

namespace StreamingMarkdown.Core.Tests.Parsing;

public sealed class ListParserTests
{
    private readonly IMarkdownParser _parser =
        new MarkdigMarkdownParser();

    [Fact]
    public void Parse_ShouldCreateUnorderedList()
    {
        // Arrange
        const string markdown = """
            - Temperature: 25°C
            - PH: 7.2
            - Chlorine: Normal
            """;

        // Act
        var document =
            _parser.Parse(markdown);

        // Assert
        Assert.Single(document.Blocks);

        var list =
            Assert.IsType<ListBlock>(
                document.Blocks[0]);

        Assert.False(list.IsOrdered);

        Assert.Equal(
            3,
            list.Items.Count);
    }

    [Fact]
public void Parse_ShouldCreateOrderedList()
{
    // Arrange
    const string markdown = """
        1. Check temperature
        2. Check PH
        3. Check chlorine
        """;

    // Act
    var document =
        _parser.Parse(markdown);

    // Assert
    var list =
        Assert.IsType<ListBlock>(
            document.Blocks[0]);

    Assert.True(list.IsOrdered);

    Assert.Equal(
        3,
        list.Items.Count);
}
}