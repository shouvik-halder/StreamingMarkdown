using StreamingMarkdown.Core.Models;

namespace StreamingMarkdown.Core.Parsing;

public interface IMarkdownParser
{
    MarkdownDocument Parse(string markdown);
}