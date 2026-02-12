using GestionPeliculas.Pages;
using GestionPeliculas.Service;

namespace GestionPeliculas.Pages;

public partial class MainMenuPage : ContentPage
{
    private readonly PeliculasService _service;
    private readonly IServiceProvider _services;

    public MainMenuPage(PeliculasService service, IServiceProvider services)
    {
        InitializeComponent();
        _service = service;
        _services = services;
    }

    private async void OnVerTodasClicked(object sender, EventArgs e)
    {
        var page = _services.GetService(typeof(ListPeliculasPage)) as Page;
        if (page != null) await Navigation.PushAsync(page);
    }

    private async void OnCrearClicked(object sender, EventArgs e)
    {
        var page = _services.GetService(typeof(CreatePeliculaPage)) as Page;
        if (page != null) await Navigation.PushAsync(page);
    }

    private async void OnObtenerByIdClicked(object sender, EventArgs e)
    {
        var page = _services.GetService(typeof(GetByIdPage)) as Page;
        if (page != null) await Navigation.PushAsync(page);
    }

    private async void OnActualizarClicked(object sender, EventArgs e)
    {
        var page = _services.GetService(typeof(UpdatePeliculaPage)) as Page;
        if (page != null) await Navigation.PushAsync(page);
    }

    private async void OnEliminarClicked(object sender, EventArgs e)
    {
        var page = _services.GetService(typeof(DeletePeliculaPage)) as Page;
        if (page != null) await Navigation.PushAsync(page);
    }

    private async void OnOpcionesClicked(object sender, EventArgs e)
    {
        var page = _services.GetService(typeof(OpcionesPage)) as Page;
        if (page != null)
        {
            await Navigation.PushAsync(page);
        }
        else
        {
            await DisplayAlert("Error", "No se pudo abrir OpcionesPage", "OK");
        }
    }
}