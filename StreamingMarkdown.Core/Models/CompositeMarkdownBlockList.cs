using System.Collections;
using StreamingMarkdown.Core.Models.Blocks;

namespace StreamingMarkdown.Core.Models;

public sealed class CompositeMarkdownBlockList
    : IReadOnlyList<MarkdownBlock>
{
    private readonly IReadOnlyList<MarkdownBlock> _prefix;
    private readonly IReadOnlyList<MarkdownBlock> _suffix;

    public CompositeMarkdownBlockList(
        IReadOnlyList<MarkdownBlock> prefix,
        IReadOnlyList<MarkdownBlock> suffix)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        ArgumentNullException.ThrowIfNull(suffix);

        _prefix = prefix;
        _suffix = suffix;
    }

    public int Count =>
        _prefix.Count + _suffix.Count;

    public MarkdownBlock this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);

            if (index < _prefix.Count)
                return _prefix[index];

            var suffixIndex =
                index - _prefix.Count;

            if (suffixIndex >= _suffix.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index));
            }

            return _suffix[suffixIndex];
        }
    }

    public IEnumerator<MarkdownBlock> GetEnumerator()
    {
        foreach (var block in _prefix)
            yield return block;

        foreach (var block in _suffix)
            yield return block;
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public static IReadOnlyList<MarkdownBlock> TakePrefix(
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

        if (count == 0)
            return Array.Empty<MarkdownBlock>();

        if (count == source.Count)
            return source;

        if (source is CompositeMarkdownBlockList composite)
        {
            if (count <= composite._prefix.Count)
            {
                return new PrefixMarkdownBlockList(
                    composite._prefix,
                    count);
            }

            var remainingCount =
                count - composite._prefix.Count;

            var suffixPrefix =
                TakePrefix(
                    composite._suffix,
                    remainingCount);

            return new CompositeMarkdownBlockList(
                composite._prefix,
                suffixPrefix);
        }

        return new PrefixMarkdownBlockList(
            source,
            count);
    }
}