using NightDog.App.ViewModels;

namespace NightDog.App.Views;

public partial class DiaryPage : ContentPage
{
	public DiaryPage(DiaryViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
