using DinoHub.MVVM.ViewModels;

namespace DinoHub.MVVM.Views;

public partial class AddPage : ContentPage
{
	public AddPage()
	{
		InitializeComponent();
		BindingContext = new DinoViewModel();
	}
}