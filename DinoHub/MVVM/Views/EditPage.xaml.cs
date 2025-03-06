using DinoHub.MVVM.Models;
using DinoHub.MVVM.ViewModels;

namespace DinoHub.MVVM.Views;

public partial class EditPage : ContentPage
{
	public EditPage(Dinosaurio dinosaurio)
	{
		InitializeComponent();
        BindingContext = new DinoViewModel(dinosaurio);
    }
}