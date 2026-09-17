using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Core.Models.Inlines;
using StreamingMarkdown.Core.Parsing;

namespace StreamingMarkdown.Core.Tests.Parsing;

public sealed class HeadingParserTests
{
    private readonly IMarkdownParser _parser =
        new MarkdigMarkdownParser();

    [Fact]
    public void Parse_ShouldCreateHeadingBlock()
    {
        // Arrange
        const string markdown = "# Pool Status";

        // Act
        var document = _parser.Parse(markdown);

        // Assert
        Assert.Single(document.Blocks);

        var heading =
            Assert.IsType<HeadingBlock>(
                document.Blocks[0]);

        Assert.Equal(1, heading.Level);

        Assert.Single(heading.Inlines);

        var text =
            Assert.IsType<TextInline>(
                heading.Inlines[0]);

        Assert.Equal(
            "Pool Status",
            text.Text);
    }


[Theory]
[InlineData("# Heading", 1)]
[InlineData("## Heading", 2)]
[InlineData("### Heading", 3)]
[InlineData("#### Heading", 4)]
[InlineData("##### Heading", 5)]
[InlineData("###### Heading", 6)]
public void Parse_ShouldPreserveHeadingLevel(
    string markdown,
    int expectedLevel)
{
    // Act
    var document = _parser.Parse(markdown);

    // Assert
    var heading =
        Assert.IsType<HeadingBlock>(
            document.Blocks[0]);

    Assert.Equal(
        expectedLevel,
        heading.Level);
}
}