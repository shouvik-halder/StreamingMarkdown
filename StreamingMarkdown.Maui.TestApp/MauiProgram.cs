using Microsoft.Extensions.Logging;
using StreamingMarkdown.Maui.DependencyInjection;
using StreamingMarkdown.Maui.TestApp.Features.Chat.Services;
using StreamingMarkdown.Maui.TestApp.Features.Chat.ViewModels;
using StreamingMarkdown.Maui.TestApp.Features.Chat.Views;
using StreamingMarkdown.Maui.TestApp.ViewModels;
using StreamingMarkdown.Maui.TestApp.Views;
using StreamingMarkdown.Maui.Styling;
using CommunityToolkit.Maui;
using Microsoft.Maui.Handlers;

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
				fonts.AddFont("Roboto-Bold.ttf", "RobotoBold");
				fonts.AddFont("Roboto-Italic.ttf", "RobotoItalic");
				fonts.AddFont("Roboto-Light.ttf", "RobotoLight");
				fonts.AddFont("Roboto-Medium.ttf", "RobotoMedium");
				fonts.AddFont("Roboto-Regular.ttf", "RobotoRegular");
				fonts.AddFont("Roboto-SemiBold.ttf", "RobotoSemiBold");
				fonts.AddFont("Roboto-BoldItalic.ttf", "RobotoBoldItalic");
			})
			.UseMauiCommunityToolkit()
			.UseStreamingMarkdown(options =>
			{
				options.FontFamily="RobotoRegular";
				options.FontFaces= new MarkdownFontFamilySet
				{
					Regular="RobotoRegular",
					Bold="RobotoBold",
					Italic="RobotoItalic",
					BoldItalic="RobotoBoldItalic",
				};
				options.LineHeight = 1;
			});

		builder.Services.AddTransient<StreamingMarkdownTestPage>();
		builder.Services.AddTransient<StreamingMarkdownTestViewModel>();
		builder.Services.AddSingleton<IChatService, MockChatService>();
		builder.Services.AddSingleton<IChatMessageActionService,LocalChatMessageActionService>();
		builder.Services.AddTransient<ChatViewModel>();
		builder.Services.AddTransient<ChatPage>();
		builder.Services.ConfigureMauiHandlers(handlers =>
		{
#if ANDROID
			EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
			{
				handler.PlatformView.BackgroundTintList=Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
			});
#elif IOS || MACCATALYST
            EntryHandler.Mapper.AppendToMapping("NoBorder", (handler, view) =>
            {
                handler.PlatformView.BorderStyle =
                    UIKit.UITextBorderStyle.None;
            });
#endif
		});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}