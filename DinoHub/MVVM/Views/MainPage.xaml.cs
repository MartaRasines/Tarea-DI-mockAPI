using DinoHub.MVVM.ViewModels;

namespace DinoHub
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
            BindingContext = new DinoViewModel();
            NavigationPage.SetHasNavigationBar(this, false);

            //Cargamos la lista de dinosaurios al iniciar la aplicacion
            (BindingContext as DinoViewModel)?.GetAllDinosCommand.Execute(null);
        }

        
    }

}
