using NightDog.App.Services;

namespace NightDog.App;

public partial class App : Application
{
	public App(IThemeService themeService)
	{
		InitializeComponent();
		UserAppTheme = themeService.SelectedTheme;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}
