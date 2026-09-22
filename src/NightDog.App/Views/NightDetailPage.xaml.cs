using NightDog.App.ViewModels;

namespace NightDog.App.Views;

public partial class NightDetailPage : ContentPage
{
	public NightDetailPage(NightDetailViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
