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
            RutaImagen = RutaImagenEntry.Text,
            Sinopsis = SinopsisEditor.Text
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

    private async void OnSubirPosterClicked(object sender, EventArgs e)
    {
        var idString = await DisplayPromptAsync("Subir poster", "Id de la pelicula:", initialValue: "");
        if (string.IsNullOrWhiteSpace(idString) || !int.TryParse(idString, out var id)) return;

        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions { Title = "Seleccione imagen" });
            if (result == null) return;

            using var stream = await result.OpenReadAsync();
            var ok = await _service.UploadPosterAsync(id, stream, result.FileName);
            await DisplayAlert("Subir poster", ok ? "Subida correcta" : "Fallo al subir", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}