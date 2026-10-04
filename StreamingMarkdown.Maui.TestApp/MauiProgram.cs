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
#if IOS || MACCATALYST
#else
				fonts.AddFont("Roboto-Bold.ttf", "RobotoBold");
				fonts.AddFont("Roboto-Italic.ttf", "RobotoItalic");
				fonts.AddFont("Roboto-Light.ttf", "RobotoLight");
				fonts.AddFont("Roboto-Medium.ttf", "RobotoMedium");
				fonts.AddFont("Roboto-Regular.ttf", "RobotoRegular");
				fonts.AddFont("Roboto-SemiBold.ttf", "RobotoSemiBold");
#endif
			})
			.UseStreamingMarkdown(options =>
			{
				options.LineHeight = 1;
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