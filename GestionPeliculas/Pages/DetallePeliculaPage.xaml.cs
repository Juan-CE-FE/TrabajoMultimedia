using GestionPeliculas.Service;
using System.IO;

namespace GestionPeliculas.Pages;

public partial class DetallePeliculaPage : ContentPage
{
    private readonly PeliculasService _service;
    private readonly int _peliculaId;

    public DetallePeliculaPage(PeliculasService service, int id)
    {
        InitializeComponent();
        _service = service;
        _peliculaId = id;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarDetalle();
    }

    private async Task CargarDetalle()
    {
        try
        {
            var p = await _service.GetPeliculaAsync(_peliculaId);

            if (p == null)
            {
                await DisplayAlert("Error", "No se pudo cargar la película", "OK");
                await Navigation.PopAsync();
                return;
            }

            // Asignar valores
            IdLabel.Text = p.Id.ToString();
            TituloLabel.Text = p.Titulo;
            DirectorLabel.Text = p.Director;
            GeneroLabel.Text = p.Genero;
            AnhoLabel.Text = p.AnhoLanzamiento.ToString();
            SinopsisLabel.Text = p.Sinopsis ?? "Sin sinopsis disponible";

            // Mostrar ruta de imagen si existe
            if (!string.IsNullOrEmpty(p.RutaImagen))
            {
                RutaImagenLabel.Text = p.RutaImagen;
                RutaFrame.IsVisible = true;
            }

            // Cargar imagen
            try
            {
                var bytes = await _service.DownloadPosterAsync(p.Id);
                if (bytes != null && bytes.Length > 0)
                {
                    ImagenPoster.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando imagen: {ex}");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}