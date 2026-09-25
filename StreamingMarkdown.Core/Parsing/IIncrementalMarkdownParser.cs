using StreamingMarkdown.Core.Models;

namespace StreamingMarkdown.Core.Parsing;

public interface IIncrementalMarkdownParser
{
    IncrementalParseResult Parse(
        string markdown,
        MarkdownDocument previousDocument);

    IncrementalParseResult ParseSuffix(
        string suffix,
        MarkdownDocument previousDocument,
        int reparseStart);
}