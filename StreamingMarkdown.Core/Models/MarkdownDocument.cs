using StreamingMarkdown.Core.Models.Blocks;

namespace StreamingMarkdown.Core.Models;

public sealed class MarkdownDocument
{
    public IReadOnlyList<MarkdownBlock> Blocks { get; }

    public MarkdownDocument(IReadOnlyList<MarkdownBlock> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        Blocks = blocks;
    }

    public static MarkdownDocument Empty =>
        new([]);
}