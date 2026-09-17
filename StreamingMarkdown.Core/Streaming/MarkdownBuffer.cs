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

    public void Clear()
    {
        _buffer.Clear();
    }
}