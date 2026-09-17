using StreamingMarkdown.Core.Models.Blocks;

namespace StreamingMarkdown.Maui.Rendering;

internal sealed class MarkdownViewState
{
    private readonly Dictionary<Guid, View> _views = new();

    public bool TryGetView(
        Guid blockId,
        out View? view)
    {
        return _views.TryGetValue(
            blockId,
            out view);
    }

    public void SetView(
        MarkdownBlock block,
        View view)
    {
        ArgumentNullException.ThrowIfNull(block);
        ArgumentNullException.ThrowIfNull(view);

        _views[block.Id] = view;
    }

    public bool RemoveView(
        Guid blockId)
    {
        return _views.Remove(blockId);
    }

    public void Clear()
    {
        _views.Clear();
    }
}