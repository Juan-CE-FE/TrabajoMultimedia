using GestionPeliculas.Service;

namespace GestionPeliculas.Pages;

public partial class LoginPage : ContentPage
{
    private readonly PeliculasService _service;
    private readonly IServiceProvider _services;

    public LoginPage(PeliculasService service, IServiceProvider services)
    {
        InitializeComponent();
        _service = service;
        _services = services;

        NavigationPage.SetHasBackButton(this, false);
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var user = UsernameEntry.Text ?? string.Empty;
        var pass = PasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            await DisplayAlert("Error", "Usuario o contraseña vacíos", "OK");
            return;
        }

        try
        {
            var ok = await _service.AuthenticateAsync(user, pass);
            if (!ok)
            {
                await DisplayAlert("Acceso denegado", "Credenciales incorrectas", "OK");
                return;
            }

            // Navegar a MainMenuPage
            var main = _services.GetService(typeof(MainMenuPage)) as Page;
            if (main != null)
            {
                await Navigation.PushAsync(main);
                // Eliminar LoginPage de la pila de navegación
                Navigation.RemovePage(this);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}