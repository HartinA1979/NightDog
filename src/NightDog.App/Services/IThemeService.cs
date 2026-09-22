namespace NightDog.App.Services;

public interface IThemeService
{
	/// <summary>Gespeicherte Auswahl; <see cref="AppTheme.Unspecified"/> bedeutet „wie System“.</summary>
	AppTheme SelectedTheme { get; }

	/// <summary>Speichert die Auswahl und wendet sie sofort an.</summary>
	void Apply(AppTheme theme);
}
