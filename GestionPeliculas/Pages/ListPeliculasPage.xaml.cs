using GestionPeliculas.Service;
using GestionPeliculas.Model;
using System.IO;

namespace GestionPeliculas.Pages;

public partial class ListPeliculasPage : ContentPage
{
    private readonly PeliculasService _service;

    public ListPeliculasPage(PeliculasService service)
    {
        InitializeComponent();
        _service = service;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPeliculasAsync();
    }

    private async Task LoadPeliculasAsync()
    {
        try
        {
            var peliculas = await _service.GetPeliculasAsync();

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
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar las películas: {ex.Message}", "OK");
        }
    }
}