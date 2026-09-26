using System.Collections;
using StreamingMarkdown.Core.Models.Blocks;

namespace StreamingMarkdown.Core.Models;

public sealed class SuffixMarkdownBlockList
    : IReadOnlyList<MarkdownBlock>
{
    private readonly IReadOnlyList<MarkdownBlock> _source;
    private readonly int _start;

    public SuffixMarkdownBlockList(
        IReadOnlyList<MarkdownBlock> source,
        int start)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegative(start);

        if (start > source.Count)
            throw new ArgumentOutOfRangeException(nameof(start));

        _source = source;
        _start = start;
    }

    public int Count =>
        _source.Count - _start;

    public MarkdownBlock this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);

            if (index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _source[_start + index];
        }
    }

    public IEnumerator<MarkdownBlock> GetEnumerator()
    {
        for (var index = _start;
             index < _source.Count;
             index++)
        {
            yield return _source[index];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}