using StreamingMarkdown.Core.Models.Blocks;

namespace StreamingMarkdown.Maui.Rendering.Views;

internal interface IMarkdownBlockView
{
    Guid BlockId { get; }

    View View { get; }

    void Update(MarkdownBlock block);
}