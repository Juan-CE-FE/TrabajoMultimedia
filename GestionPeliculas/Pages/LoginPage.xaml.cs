using GestionPeliculas.Service;

namespace GestionPeliculas.Pages;

public partial class LoginPage : ContentPage
{
    private readonly PeliculasService _service;

    public LoginPage(PeliculasService service)
    {
        InitializeComponent();
        _service = service;
    }

    private void OnLoginClicked(object sender, EventArgs e)
    {
        var user = UsernameEntry.Text ?? string.Empty;
        var pass = PasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            DisplayAlert("Error", "Usuario o contraseña vacíos", "OK");
            return;
        }

        _service.SetBasicAuth(user, pass);
        // Volver atrás
        Navigation.PopAsync();
    }
}