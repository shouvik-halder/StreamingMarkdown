using StreamingMarkdown.Maui.TestApp.ViewModels;

namespace StreamingMarkdown.Maui.TestApp.Views;

public partial class StreamingMarkdownTestPage : ContentPage
{
    public StreamingMarkdownTestPage(
        StreamingMarkdownTestViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}