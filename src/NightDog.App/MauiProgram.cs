using CommunityToolkit.Maui;
using LiveChartsCore.SkiaSharpView.Maui;
using Microsoft.Extensions.Logging;
using NightDog.App.Services;
using NightDog.App.ViewModels;
using NightDog.App.Views;

namespace NightDog.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.UseLiveCharts()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("Inter-Light.ttf", "InterLight");
				fonts.AddFont("Inter-Regular.ttf", "InterRegular");
				fonts.AddFont("Inter-Medium.ttf", "InterMedium");
				fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");
				fonts.AddFont("Inter-Bold.ttf", "InterBold");
				fonts.AddFont("Inter-ExtraBold.ttf", "InterExtraBold");
			});

		builder.Services
			.AddServices()
			.AddPages();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	private static IServiceCollection AddServices(this IServiceCollection services)
	{
		services.AddSingleton<IPreferences>(Preferences.Default);
		services.AddSingleton<INavigationService, ShellNavigationService>();
		services.AddSingleton<IThemeService, ThemeService>();
		return services;
	}

	private static IServiceCollection AddPages(this IServiceCollection services)
	{
		services.AddTransient<StartPage, StartViewModel>();
		services.AddTransient<HistoryPage, HistoryViewModel>();
		services.AddTransientWithShellRoute<NightDetailPage, NightDetailViewModel>(Routes.NightDetail);
		services.AddTransient<DiaryPage, DiaryViewModel>();
		services.AddTransient<SettingsPage, SettingsViewModel>();
		return services;
	}
}
