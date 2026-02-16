using GestionPeliculas.Service;
using GestionPeliculas.Model;
using System.IO;

namespace GestionPeliculas.Pages;

public partial class ListPeliculasPage : ContentPage
{
    private readonly PeliculasService _service;
    private string _tipoBusqueda;
    private string _terminoBusqueda;

    public ListPeliculasPage(PeliculasService service)
    {
        InitializeComponent();
        _service = service;
    }

    public ListPeliculasPage(PeliculasService service, string tipoBusqueda, string termino)
    {
        InitializeComponent();
        _service = service;
        _tipoBusqueda = tipoBusqueda;
        _terminoBusqueda = termino;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!string.IsNullOrEmpty(_tipoBusqueda))
            await CargarResultadosBusqueda();
        else
            await LoadPeliculasAsync();
    }

    private async Task LoadPeliculasAsync()
    {
        try
        {
            Title = "Todas las películas";
            var peliculas = await _service.GetPeliculasAsync();
            await MostrarPeliculas(peliculas);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar las películas: {ex.Message}", "OK");
        }
    }

    private async Task CargarResultadosBusqueda()
    {
        try
        {
            List<Pelicula> resultados = new();

            switch (_tipoBusqueda)
            {
                case "genero":
                    Title = $"Género: {_terminoBusqueda}";
                    resultados = await _service.SearchByGeneroAsync(_terminoBusqueda);
                    break;
                case "anho":
                    if (int.TryParse(_terminoBusqueda, out int anho))
                    {
                        Title = $"Año: {anho}";
                        resultados = await _service.SearchByAnhoAsync(anho);
                    }
                    break;
                case "director":
                    Title = $"Director: {_terminoBusqueda}";
                    resultados = await _service.SearchByDirectorAsync(_terminoBusqueda);
                    break;
            }

            await MostrarPeliculas(resultados);

            if (!resultados.Any())
            {
                await DisplayAlert("Sin resultados", $"No se encontraron películas para {_tipoBusqueda}: {_terminoBusqueda}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error en búsqueda: {ex.Message}", "OK");
        }
    }

    private async Task MostrarPeliculas(List<Pelicula> peliculas)
    {
        var items = new List<PeliculaItem>();

        foreach (var p in peliculas)
        {
            var item = PeliculaItem.FromPelicula(p);

            try
            {
                var bytes = await _service.DownloadPosterAsync(p.Id);
                if (bytes != null && bytes.Length > 0)
                {
                    item.PosterImage = ImageSource.FromStream(() => new MemoryStream(bytes));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando imagen {p.Id}: {ex}");
            }

            items.Add(item);
        }

        ListaPeliculas.ItemsSource = items;
    }

    //NUEVO: Navegar a detalle de película
    private async void OnVerInfoClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button?.CommandParameter is int id)
        {
            var detallePage = new DetallePeliculaPage(_service, id);
            await Navigation.PushAsync(detallePage);
        }
    }
}