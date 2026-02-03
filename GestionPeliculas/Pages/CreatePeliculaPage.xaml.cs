using GestionPeliculas.Model;
using GestionPeliculas.Service;

namespace GestionPeliculas.Pages;

public partial class CreatePeliculaPage : ContentPage
{
    private readonly PeliculasService _service;

    public CreatePeliculaPage(PeliculasService service)
    {
        InitializeComponent();
        _service = service;
    }

    private async void OnCrearClicked(object sender, EventArgs e)
    {
        if (!int.TryParse(AnhoEntry.Text, out var anho)) anho = 0;

        var p = new Pelicula
        {
            Titulo = TituloEntry.Text ?? string.Empty,
            Director = DirectorEntry.Text ?? string.Empty,
            Genero = GeneroEntry.Text ?? string.Empty,
            AnhoLanzamiento = anho,
            RutaImagen = RutaImagenEntry.Text
        };

        try
        {
            var ok = await _service.CrearPeliculaAsync(p);
            await DisplayAlert("Crear", ok ? "Creada correctamente" : "Fallo al crear", "OK");
            if (ok) await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}