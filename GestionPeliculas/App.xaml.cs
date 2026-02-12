using GestionPeliculas.Pages;

namespace GestionPeliculas
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            var login = serviceProvider.GetRequiredService<LoginPage>();
            MainPage = new NavigationPage(login);
        }
    }
}