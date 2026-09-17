using StreamingMarkdown.Core.Models.Blocks;

namespace StreamingMarkdown.Core.Diffing;

public sealed class MarkdownChange
{
    public MarkdownChangeType Type { get; }

    public int PreviousIndex { get; }

    public int CurrentIndex { get; }

    public MarkdownBlock? Previous { get; }

    public MarkdownBlock? Current { get; }

    public MarkdownChange(
        MarkdownChangeType type,
        int previousIndex,
        int currentIndex,
        MarkdownBlock? previous,
        MarkdownBlock? current)
    {
        Type = type;
        PreviousIndex = previousIndex;
        CurrentIndex = currentIndex;
        Previous = previous;
        Current = current;
    }
}