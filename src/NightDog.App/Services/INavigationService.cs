namespace NightDog.App.Services;

/// <summary>
/// Kapselt die Shell-Navigation, damit ViewModels ohne Abhängigkeit von <see cref="Shell"/> testbar bleiben.
/// </summary>
public interface INavigationService
{
	Task GoToAsync(string route, IDictionary<string, object>? parameters = null);

	Task GoBackAsync();
}
