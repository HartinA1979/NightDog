using NightDog.App.ViewModels;

namespace NightDog.App.Views;

public partial class HistoryPage : ContentPage
{
	public HistoryPage(HistoryViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
