using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Models.Blocks;

namespace StreamingMarkdown.Core.Diffing;

public sealed class DocumentDiffEngine : IDocumentDiffEngine
{
    public MarkdownDiff Compare(
        MarkdownDocument previous,
        MarkdownDocument current)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(current);

        var previousBlocks = previous.Blocks;
        var currentBlocks = current.Blocks;

        if (previousBlocks.Count == 0 &&
            currentBlocks.Count == 0)
        {
            return MarkdownDiff.Empty;
        }

        var lcs = BuildLcsTable(
            previousBlocks,
            currentBlocks);

        var changes = new List<MarkdownChange>();

        var previousIndex = previousBlocks.Count;
        var currentIndex = currentBlocks.Count;

        while (previousIndex > 0 ||
               currentIndex > 0)
        {
            // Same block -> unchanged
            if (previousIndex > 0 &&
                currentIndex > 0 &&
                MarkdownBlockComparer.AreEquivalent(
                    previousBlocks[previousIndex - 1],
                    currentBlocks[currentIndex - 1]))
            {
                previousIndex--;
                currentIndex--;

                continue;
            }

            // If both documents contain the same number
            // of blocks, a mismatch represents a modification.
            if (previousBlocks.Count == currentBlocks.Count &&
                previousIndex > 0 &&
                currentIndex > 0)
            {
                changes.Add(
                    new MarkdownChange(
                        MarkdownChangeType.Modified,
                        previousIndex - 1,
                        currentIndex - 1,
                        previousBlocks[previousIndex - 1],
                        currentBlocks[currentIndex - 1]));

                previousIndex--;
                currentIndex--;

                continue;
            }

            // Addition
            if (currentIndex > 0 &&
                (previousIndex == 0 ||
                 lcs[previousIndex, currentIndex - 1] >=
                 lcs[previousIndex - 1, currentIndex]))
            {
                changes.Add(
                    new MarkdownChange(
                        MarkdownChangeType.Added,
                        -1,
                        currentIndex - 1,
                        null,
                        currentBlocks[currentIndex - 1]));

                currentIndex--;

                continue;
            }

            // Removal
            if (previousIndex > 0)
            {
                changes.Add(
                    new MarkdownChange(
                        MarkdownChangeType.Removed,
                        previousIndex - 1,
                        -1,
                        previousBlocks[previousIndex - 1],
                        null));

                previousIndex--;

                continue;
            }
        }

        changes.Reverse();

        return new MarkdownDiff(changes);
    }

    public MarkdownDiff CompareIncremental(
    MarkdownDocument previous,
    MarkdownDocument current,
    int reusedBlockCount)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(current);

        ArgumentOutOfRangeException.ThrowIfNegative(
            reusedBlockCount);

        if (reusedBlockCount > previous.Blocks.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(reusedBlockCount));
        }

        if (reusedBlockCount > current.Blocks.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(reusedBlockCount));
        }

        var previousSuffix =
    new SuffixMarkdownBlockList(
        previous.Blocks,
        reusedBlockCount);

var currentSuffix =
    new SuffixMarkdownBlockList(
        current.Blocks,
        reusedBlockCount);

        if (previousSuffix.Count == 0 &&
            currentSuffix.Count == 0)
        {
            return MarkdownDiff.Empty;
        }

        var suffixDiff =
            Compare(
                new MarkdownDocument(previousSuffix),
                new MarkdownDocument(currentSuffix));

        if (!suffixDiff.HasChanges)
        {
            return MarkdownDiff.Empty;
        }

        var changes =
            new List<MarkdownChange>(
                suffixDiff.Changes.Count);

        foreach (var change in suffixDiff.Changes)
        {
            var previousIndex =
                change.PreviousIndex >= 0
                    ? change.PreviousIndex + reusedBlockCount
                    : -1;

            var currentIndex =
                change.CurrentIndex >= 0
                    ? change.CurrentIndex + reusedBlockCount
                    : -1;

            changes.Add(
                new MarkdownChange(
                    change.Type,
                    previousIndex,
                    currentIndex,
                    change.Previous,
                    change.Current));
        }

        return new MarkdownDiff(changes);
    }

    private static int[,] BuildLcsTable(
        IReadOnlyList<MarkdownBlock> previous,
        IReadOnlyList<MarkdownBlock> current)
    {
        var table = new int[
            previous.Count + 1,
            current.Count + 1];

        for (var i = 1; i <= previous.Count; i++)
        {
            for (var j = 1; j <= current.Count; j++)
            {
                if (MarkdownBlockComparer.AreEquivalent(
                        previous[i - 1],
                        current[j - 1]))
                {
                    table[i, j] =
                        table[i - 1, j - 1] + 1;
                }
                else
                {
                    table[i, j] =
                        Math.Max(
                            table[i - 1, j],
                            table[i, j - 1]);
                }
            }
        }

        return table;
    }
}