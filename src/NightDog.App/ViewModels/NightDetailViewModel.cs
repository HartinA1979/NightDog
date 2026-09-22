using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NightDog.App.Services;

namespace NightDog.App.ViewModels;

public partial class NightDetailViewModel(INavigationService navigation) : ObservableObject, IQueryAttributable
{
	public const string NightIdParameter = "nightId";

	[ObservableProperty]
	public partial string? NightId { get; set; }

	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.TryGetValue(NightIdParameter, out var id))
			NightId = id?.ToString();
	}

	[RelayCommand]
	private Task GoBackAsync() => navigation.GoBackAsync();
}
