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

    public static readonly BindableProperty StyleProperty =
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

    public MarkdownStyle? Style
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
            var view =
                renderer.RenderBlock(block);

            if (view is null)
            {
                continue;
            }

            _state.SetView(
                block,
                view);

            _layout.Children.Add(
                view);
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
                        change,
                        renderer);

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

        var view =
            renderer.RenderBlock(
                change.Current);

        if (view is null)
        {
            return;
        }

        _state.SetView(
            change.Current,
            view);
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
                out var view))
        {
            return;
        }

        if (view is not null)
        {
            _layout.Children.Remove(
                view);
        }

        _state.RemoveView(
            change.Previous.Id);
    }

    private void ApplyModified(
        MarkdownChange change,
        MarkdownDocumentRenderer renderer)
    {
        if (change.Current is null)
        {
            return;
        }

        if (!_state.TryGetView(
                change.Current.Id,
                out var existingView))
        {
            ApplyAdded(
                change,
                renderer);

            return;
        }

        var newView =
            renderer.RenderBlock(
                change.Current);

        if (newView is null)
        {
            return;
        }

        var index =
            _layout.Children.IndexOf(
                existingView!);

        if (index >= 0)
        {
            _layout.Children[
                index] = newView;
        }

        _state.SetView(
            change.Current,
            newView);
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
                    out var view))
            {
                continue;
            }

            if (view is null)
            {
                continue;
            }

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
    }
}