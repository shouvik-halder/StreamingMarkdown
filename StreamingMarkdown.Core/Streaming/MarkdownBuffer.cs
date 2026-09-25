using System.Text;

namespace StreamingMarkdown.Core.Streaming;

public sealed class MarkdownBuffer
{
    private readonly StringBuilder _buffer = new();

    public string Content => _buffer.ToString();

    public int Length => _buffer.Length;

    public void Append(string chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);

        _buffer.Append(chunk);
    }

    public string GetSuffix(int start)
{
    ArgumentOutOfRangeException.ThrowIfNegative(start);

    if (start > _buffer.Length)
    {
        throw new ArgumentOutOfRangeException(
            nameof(start));
    }

    if (start == _buffer.Length)
        return string.Empty;

    return _buffer.ToString(
        start,
        _buffer.Length - start);
}

    public void Clear()
    {
        _buffer.Clear();
    }
}