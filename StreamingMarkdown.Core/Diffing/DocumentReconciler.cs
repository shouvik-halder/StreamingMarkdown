using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Models.Blocks;

namespace StreamingMarkdown.Core.Diffing;

public sealed class DocumentReconciler : IDocumentReconciler
{
    public MarkdownDocument Reconcile(
        MarkdownDocument previous,
        MarkdownDocument current)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(current);

        var result = new List<MarkdownBlock>();

        var usedPreviousIndexes = new HashSet<int>();

        for (var currentIndex = 0;
             currentIndex < current.Blocks.Count;
             currentIndex++)
        {
            var currentBlock =
                current.Blocks[currentIndex];

            // 1. Prefer the same position.
            if (currentIndex < previous.Blocks.Count &&
                !usedPreviousIndexes.Contains(currentIndex) &&
                MarkdownBlockComparer.AreEquivalent(
                    previous.Blocks[currentIndex],
                    currentBlock))
            {
                usedPreviousIndexes.Add(currentIndex);

                result.Add(
                    RecreateWithId(
                        currentBlock,
                        previous.Blocks[currentIndex].Id));

                continue;
            }

            // 2. Look anywhere in the previous document
            // for the exact same logical block.
            var matchingPreviousIndex =
                FindMatchingPreviousBlock(
                    previous.Blocks,
                    currentBlock,
                    usedPreviousIndexes);

            if (matchingPreviousIndex >= 0)
            {
                usedPreviousIndexes.Add(
                    matchingPreviousIndex);

                result.Add(
                    RecreateWithId(
                        currentBlock,
                        previous.Blocks[matchingPreviousIndex].Id));

                continue;
            }

            // 3. If the next current block matches the current
            // previous position, this block is an insertion.
            if (currentIndex + 1 < current.Blocks.Count &&
                currentIndex < previous.Blocks.Count &&
                !usedPreviousIndexes.Contains(currentIndex) &&
                MarkdownBlockComparer.AreEquivalent(
                    previous.Blocks[currentIndex],
                    current.Blocks[currentIndex + 1]))
            {
                result.Add(currentBlock);

                continue;
            }

            // 4. No existing block matches.
            //
            // If there is an unused previous block at this
            // position, treat this as modified content and
            // preserve its identity.
            if (currentIndex < previous.Blocks.Count &&
                !usedPreviousIndexes.Contains(currentIndex))
            {
                usedPreviousIndexes.Add(currentIndex);

                result.Add(
                    RecreateWithId(
                        currentBlock,
                        previous.Blocks[currentIndex].Id));

                continue;
            }

            // 5. Completely new block.
            result.Add(currentBlock);
        }

        return new MarkdownDocument(result);
    }

    public MarkdownDocument ReconcileIncremental(
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

    var result =
        new List<MarkdownBlock>(
            current.Blocks.Count);

    // Reuse the stable prefix while preserving the
// identity of the corresponding previous blocks.
for (var i = 0;
     i < reusedBlockCount;
     i++)
{
    result.Add(
        RecreateWithId(
            current.Blocks[i],
            previous.Blocks[i].Id));
}

    // Only reconcile the affected suffix.
    var previousSuffix =
        previous.Blocks
            .Skip(reusedBlockCount)
            .ToList();

    for (var currentIndex = reusedBlockCount;
         currentIndex < current.Blocks.Count;
         currentIndex++)
    {
        var currentBlock =
            current.Blocks[currentIndex];

        var localCurrentIndex =
            currentIndex - reusedBlockCount;

        var matchingPreviousIndex =
            FindMatchingPreviousBlock(
                previousSuffix,
                currentBlock,
                new HashSet<int>());

        if (matchingPreviousIndex >= 0)
        {
            var previousBlock =
                previousSuffix[matchingPreviousIndex];

            result.Add(
                RecreateWithId(
                    currentBlock,
                    previousBlock.Id));

            continue;
        }

        // If there is a previous block at the same local
        // position, treat this as modified content and
        // preserve its identity.
        if (localCurrentIndex < previousSuffix.Count)
        {
            result.Add(
                RecreateWithId(
                    currentBlock,
                    previousSuffix[localCurrentIndex].Id));

            continue;
        }

        result.Add(currentBlock);
    }

    return new MarkdownDocument(result);
}
    private static int FindMatchingPreviousBlock(
        IReadOnlyList<MarkdownBlock> previousBlocks,
        MarkdownBlock currentBlock,
        HashSet<int> usedPreviousIndexes)
    {
        for (var i = 0; i < previousBlocks.Count; i++)
        {
            if (usedPreviousIndexes.Contains(i))
            {
                continue;
            }

            if (MarkdownBlockComparer.AreEquivalent(
                    previousBlocks[i],
                    currentBlock))
            {
                return i;
            }
        }

        return -1;
    }

    private static MarkdownBlock RecreateWithId(
        MarkdownBlock block,
        Guid id)
    {
        return block switch
        {
            HeadingBlock heading =>
                new HeadingBlock(
    block.Position,
    block.SourceStart,
    block.SourceEnd,
    heading.Level,
    heading.Inlines,
    id),

            ParagraphBlock paragraph =>
                new ParagraphBlock(
    block.Position,
    block.SourceStart,
    block.SourceEnd,
    paragraph.Inlines,
    id),

            ListBlock list =>
                new ListBlock(
    block.Position,
    block.SourceStart,
    block.SourceEnd,
    list.IsOrdered,
    list.Items,
    id),

            QuoteBlock quote =>
                new QuoteBlock(
    block.Position,
    block.SourceStart,
    block.SourceEnd,
    quote.Blocks,
    id),

            _ => throw new NotSupportedException(
                $"Unsupported block type: {block.GetType().Name}")
        };
    }
}