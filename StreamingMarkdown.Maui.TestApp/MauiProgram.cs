using Microsoft.Extensions.Logging;
using StreamingMarkdown.Maui.DependencyInjection;
using StreamingMarkdown.Maui.TestApp.Features.Chat.Services;
using StreamingMarkdown.Maui.TestApp.Features.Chat.ViewModels;
using StreamingMarkdown.Maui.TestApp.Features.Chat.Views;
using StreamingMarkdown.Maui.TestApp.ViewModels;
using StreamingMarkdown.Maui.TestApp.Views;

namespace StreamingMarkdown.Maui.TestApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
			.UseStreamingMarkdown(options =>
			{
				options.FontFamily = "OpenSansRegular";
				options.FontSize = 14;
				options.HeadingFontSize = 20;
			});

		builder.Services.AddTransient<StreamingMarkdownTestPage>();
		builder.Services.AddTransient<StreamingMarkdownTestViewModel>();
		builder.Services.AddSingleton<IChatService, MockChatService>();
		builder.Services.AddTransient<ChatViewModel>();
		builder.Services.AddTransient<ChatPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}