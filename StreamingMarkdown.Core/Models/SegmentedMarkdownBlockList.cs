using System.Collections;
using StreamingMarkdown.Core.Models.Blocks;

namespace StreamingMarkdown.Core.Models;

public sealed class SegmentedMarkdownBlockList
    : IReadOnlyList<MarkdownBlock>
{
    private readonly Segment[] _segments;
    private readonly int[] _starts;
    private readonly int _count;

    private readonly struct Segment
    {
        public readonly IReadOnlyList<MarkdownBlock> Source;
        public readonly int Start;
        public readonly int Count;

        public Segment(
            IReadOnlyList<MarkdownBlock> source,
            int start,
            int count)
        {
            Source = source;
            Start = start;
            Count = count;
        }
    }

    public SegmentedMarkdownBlockList(
        IReadOnlyList<MarkdownBlock> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        _segments =
        [
            new Segment(
                source,
                0,
                source.Count)
        ];

        _starts = [0];
        _count = source.Count;
    }

    private SegmentedMarkdownBlockList(
        Segment[] segments,
        int[] starts,
        int count)
    {
        _segments = segments;
        _starts = starts;
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

            var segmentIndex =
                FindSegment(index);

            var segment =
                _segments[segmentIndex];

            var localIndex =
                index -
                _starts[segmentIndex];

            return segment.Source[
                segment.Start + localIndex];
        }
    }

    public IEnumerator<MarkdownBlock> GetEnumerator()
    {
        foreach (var segment in _segments)
        {
            for (var i = 0; i < segment.Count; i++)
            {
                yield return
                    segment.Source[
                        segment.Start + i];
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public static SegmentedMarkdownBlockList Create(
        IReadOnlyList<MarkdownBlock> prefix,
        IReadOnlyList<MarkdownBlock> suffix)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        ArgumentNullException.ThrowIfNull(suffix);

        if (prefix.Count == 0)
        {
            return new SegmentedMarkdownBlockList(
                suffix);
        }

        if (suffix.Count == 0)
        {
            return new SegmentedMarkdownBlockList(
                prefix);
        }

        var segments =
            new Segment[2]
            {
                new Segment(
                    prefix,
                    0,
                    prefix.Count),

                new Segment(
                    suffix,
                    0,
                    suffix.Count)
            };

        var starts =
            new int[2]
            {
                0,
                prefix.Count
            };

        return new SegmentedMarkdownBlockList(
            segments,
            starts,
            prefix.Count + suffix.Count);
    }

    private int FindSegment(int index)
    {
        var low = 0;
        var high = _starts.Length - 1;

        while (low <= high)
        {
            var middle =
                low + ((high - low) >> 1);

            if (_starts[middle] <= index)
            {
                low = middle + 1;
            }
            else
            {
                high = middle - 1;
            }
        }

        return high;
    }
}