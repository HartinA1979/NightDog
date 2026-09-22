namespace NightDog.App.Services;

public sealed class ThemeService(IPreferences preferences) : IThemeService
{
	private const string PreferenceKey = "app_theme";

	// Die App wird nachts bedient, daher ist Dunkel der Standard (siehe docs/Designsystem.md).
	private const AppTheme DefaultTheme = AppTheme.Dark;

	public AppTheme SelectedTheme =>
		(AppTheme)preferences.Get(PreferenceKey, (int)DefaultTheme);

	public void Apply(AppTheme theme)
	{
		preferences.Set(PreferenceKey, (int)theme);

		if (Application.Current is { } app)
			app.UserAppTheme = theme;
	}
}
