using GestionPeliculas.Pages;
using Microsoft.Maui.Controls;

namespace GestionPeliculas
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            var mainMenu = serviceProvider.GetRequiredService<MainMenuPage>();
            MainPage = new NavigationPage(mainMenu);
        }
    }
}