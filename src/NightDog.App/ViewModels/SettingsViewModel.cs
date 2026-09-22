using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NightDog.App.Services;

namespace NightDog.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
	private readonly IThemeService themeService;

	public SettingsViewModel(IThemeService themeService)
	{
		this.themeService = themeService;

		ThemeOptions =
		[
			new("Dunkel", AppTheme.Dark),
			new("Hell", AppTheme.Light),
			new("System", AppTheme.Unspecified),
		];

		var selected = ThemeOptions.FirstOrDefault(o => o.Value == themeService.SelectedTheme) ?? ThemeOptions[0];
		selected.IsSelected = true;
	}

	public IReadOnlyList<ThemeOption> ThemeOptions { get; }

	public string AppVersion => $"Version {AppInfo.Current.VersionString}";

	[RelayCommand]
	private void SelectTheme(ThemeOption option)
	{
		ThemeOptions.Select(option);
		themeService.Apply(option.Value);
	}
}
