using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NightDog.App.Services;

namespace NightDog.App.ViewModels;

public partial class HistoryViewModel(INavigationService navigation) : ObservableObject
{
	[RelayCommand]
	private Task OpenSampleNightAsync() =>
		navigation.GoToAsync(Routes.NightDetail, new Dictionary<string, object>
		{
			[NightDetailViewModel.NightIdParameter] = "beispiel",
		});
}
