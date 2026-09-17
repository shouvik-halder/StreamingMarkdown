using StreamingMarkdown.Core.Models;

namespace StreamingMarkdown.Core.Diffing;

public interface IDocumentDiffEngine
{
    MarkdownDiff Compare(
        MarkdownDocument previous,
        MarkdownDocument current);
}