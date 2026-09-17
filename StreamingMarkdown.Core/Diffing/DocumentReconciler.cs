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
                    heading.Position,
                    heading.Level,
                    heading.Inlines,
                    id),

            ParagraphBlock paragraph =>
                new ParagraphBlock(
                    paragraph.Position,
                    paragraph.Inlines,
                    id),

            ListBlock list =>
                new ListBlock(
                    list.Position,
                    list.IsOrdered,
                    list.Items,
                    id),

            QuoteBlock quote =>
                new QuoteBlock(
                    quote.Position,
                    quote.Blocks,
                    id),

            _ => throw new NotSupportedException(
                $"Unsupported block type: {block.GetType().Name}")
        };
    }
}