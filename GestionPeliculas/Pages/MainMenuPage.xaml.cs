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

    // CRUD
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

    // BÚSQUEDAS
    private async void OnBuscarGeneroClicked(object sender, EventArgs e)
    {
        var termino = GeneroEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(termino))
        {
            await DisplayAlert("Error", "Introduce un género", "OK");
            return;
        }

        var page = new ListPeliculasPage(_service, "genero", termino);
        await Navigation.PushAsync(page);
        GeneroEntry.Text = string.Empty;
    }

    private async void OnBuscarAnhoClicked(object sender, EventArgs e)
    {
        var termino = AnhoEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(termino))
        {
            await DisplayAlert("Error", "Introduce un año", "OK");
            return;
        }

        if (!int.TryParse(termino, out _))
        {
            await DisplayAlert("Error", "El año debe ser un número", "OK");
            return;
        }

        var page = new ListPeliculasPage(_service, "anho", termino);
        await Navigation.PushAsync(page);
        AnhoEntry.Text = string.Empty;
    }

    private async void OnBuscarDirectorClicked(object sender, EventArgs e)
    {
        var termino = DirectorEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(termino))
        {
            await DisplayAlert("Error", "Introduce un director", "OK");
            return;
        }

        var page = new ListPeliculasPage(_service, "director", termino);
        await Navigation.PushAsync(page);
        DirectorEntry.Text = string.Empty;
    }

    // OPCIONES AVANZADAS
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