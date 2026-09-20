using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Core.Results;
using StreamingMarkdown.Maui.Rendering;
using StreamingMarkdown.Maui.Styling;

namespace StreamingMarkdown.Maui.Controls;

public sealed class MarkdownView : ContentView
{
    private readonly VerticalStackLayout _layout;
    private readonly MarkdownViewState _state;
    private bool _isApplyingUpdate;

    public static readonly BindableProperty DocumentProperty =
        BindableProperty.Create(
            nameof(Document),
            typeof(MarkdownDocument),
            typeof(MarkdownView),
            MarkdownDocument.Empty,
            propertyChanged:
                OnDocumentChanged);

    public static new readonly BindableProperty StyleProperty =
        BindableProperty.Create(
            nameof(Style),
            typeof(MarkdownStyle),
            typeof(MarkdownView),
            null,
            propertyChanged:
                OnStyleChanged);

    public MarkdownDocument Document
    {
        get =>
            (MarkdownDocument)GetValue(
                DocumentProperty);

        set =>
            SetValue(
                DocumentProperty,
                value);
    }

    public new MarkdownStyle? Style
    {
        get =>
            (MarkdownStyle?)GetValue(
                StyleProperty);

        set =>
            SetValue(
                StyleProperty,
                value);
    }

    public MarkdownView()
    {
        _layout =
            new VerticalStackLayout
            {
                Spacing = 0
            };

        _state =
            new MarkdownViewState();

        Content =
            new ScrollView
            {
                Content = _layout
            };
    }

    private static void OnDocumentChanged(
    BindableObject bindable,
    object oldValue,
    object newValue)
{
    var view =
        (MarkdownView)bindable;

    if (view._isApplyingUpdate)
    {
        return;
    }

    view.RenderDocument(
        (MarkdownDocument)newValue);
}

    private static void OnStyleChanged(
        BindableObject bindable,
        object oldValue,
        object newValue)
    {
        var view =
            (MarkdownView)bindable;

        view.RenderDocument(
            view.Document);
    }

private void RenderDocument(
    MarkdownDocument document)
{
    _layout.Children.Clear();
    _state.Clear();

    var renderer =
        new MarkdownDocumentRenderer(
            Style);

    foreach (var block in document.Blocks)
    {
        var blockView =
            renderer.RenderBlock(
                block);

        if (blockView is null)
        {
            continue;
        }

        _state.SetView(
            block,
            blockView);

        _layout.Children.Add(
            blockView.View);
    }
}
    public void ApplyUpdate(
    MarkdownUpdate update)
{
    ArgumentNullException.ThrowIfNull(update);

    _isApplyingUpdate = true;

    try
    {
        if (!update.Diff.HasChanges)
        {
            SetValue(
                DocumentProperty,
                update.Document);

            return;
        }

        var renderer =
            new MarkdownDocumentRenderer(
                Style);

        foreach (var change in update.Diff.Changes)
{
    switch (change.Type)
    {
        case MarkdownChangeType.Added:
            ApplyAdded(
                change,
                renderer);

            break;

        case MarkdownChangeType.Removed:
            ApplyRemoved(
                change);

            break;

        case MarkdownChangeType.Modified:
            ApplyModified(
                change);

            break;
    }
}

        ReorderViews(
            update.Document);

        SetValue(
            DocumentProperty,
            update.Document);
    }
    finally
    {
        _isApplyingUpdate = false;
    }
}

private void ApplyAdded(
    MarkdownChange change,
    MarkdownDocumentRenderer renderer)
{
    if (change.Current is null)
    {
        return;
    }

    var blockView =
        renderer.RenderBlock(
            change.Current);

    if (blockView is null)
    {
        return;
    }

    _state.SetView(
        change.Current,
        blockView);
}
private void ApplyRemoved(
    MarkdownChange change)
{
    if (change.Previous is null)
    {
        return;
    }

    if (!_state.TryGetView(
            change.Previous.Id,
            out var blockView))
    {
        return;
    }

    _layout.Children.Remove(
        blockView.View);

    _state.RemoveView(
        change.Previous.Id);
}
private void ApplyModified(
    MarkdownChange change)
{
    if (change.Current is null)
    {
        return;
    }

    if (!_state.TryGetView(
            change.Current.Id,
            out var blockView))
    {
        var renderer =
            new MarkdownDocumentRenderer(
                Style);

        ApplyAdded(
            change,
            renderer);

        return;
    }

    blockView.Update(
        change.Current);
}
private void ReorderViews(
    MarkdownDocument document)
{
    for (var targetIndex = 0;
         targetIndex < document.Blocks.Count;
         targetIndex++)
    {
        var block =
            document.Blocks[targetIndex];

        if (!_state.TryGetView(
                block.Id,
                out var blockView))
        {
            continue;
        }

        if (blockView is null)
        {
            continue;
        }

        var view =
            blockView.View;

        var currentIndex =
            _layout.Children.IndexOf(
                view);

        if (currentIndex == targetIndex)
        {
            continue;
        }

        if (currentIndex >= 0)
        {
            _layout.Children.RemoveAt(
                currentIndex);
        }

        _layout.Children.Insert(
            targetIndex,
            view);
    }
}}