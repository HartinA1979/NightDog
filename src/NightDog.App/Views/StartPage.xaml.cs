using NightDog.App.ViewModels;

namespace NightDog.App.Views;

public partial class StartPage : ContentPage
{
	public StartPage(StartViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
