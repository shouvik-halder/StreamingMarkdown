using StreamingMarkdown.Core.Models;

namespace StreamingMarkdown.Core.Diffing;

public interface IDocumentDiffEngine
{
    MarkdownDiff Compare(
        MarkdownDocument previous,
        MarkdownDocument current);

    MarkdownDiff CompareIncremental(
        MarkdownDocument previous,
        MarkdownDocument current,
        int reusedBlockCount);
}