using System.Collections;

using StreamingMarkdown.Core.Models.Blocks;

namespace StreamingMarkdown.Core.Models;

public sealed class PrefixMarkdownBlockList
    : IReadOnlyList<MarkdownBlock>
{
    private readonly IReadOnlyList<MarkdownBlock> _source;
    private readonly int _count;

    public PrefixMarkdownBlockList(
        IReadOnlyList<MarkdownBlock> source,
        int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        if (count > source.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count));
        }

        _source = source;
        _count = count;
    }

    public int Count => _count;

    public MarkdownBlock this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);

            if (index >= _count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index));
            }

            return _source[index];
        }
    }

    public IEnumerator<MarkdownBlock> GetEnumerator()
    {
        for (var index = 0; index < _count; index++)
            yield return _source[index];
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}