using GestionPeliculas.Model;
using GestionPeliculas.Service;

namespace GestionPeliculas.Pages;

public partial class UpdatePeliculaPage : ContentPage
{
    private readonly PeliculasService _service;

    public UpdatePeliculaPage(PeliculasService service)
    {
        InitializeComponent();
        _service = service;
    }

    private async void OnCargarClicked(object sender, EventArgs e)
    {
        if (!int.TryParse(IdEntry.Text, out var id))
        {
            await DisplayAlert("Error", "Id inválido", "OK");
            return;
        }

        try
        {
            var p = await _service.GetPeliculaAsync(id);
            if (p is null)
            {
                await DisplayAlert("No encontrado", "No existe la pelicula", "OK");
                return;
            }

            TituloEntry.Text = p.Titulo;
            DirectorEntry.Text = p.Director;
            GeneroEntry.Text = p.Genero;
            AnhoEntry.Text = p.AnhoLanzamiento.ToString();
            RutaImagenEntry.Text = p.RutaImagen;
            SinopsisEditor.Text = p.Sinopsis;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnActualizarClicked(object sender, EventArgs e)
    {
        if (!int.TryParse(IdEntry.Text, out var id))
        {
            await DisplayAlert("Error", "Id inválido", "OK");
            return;
        }

        if (!int.TryParse(AnhoEntry.Text, out var anho)) anho = 0;

        var p = new Pelicula
        {
            Id = id,
            Titulo = TituloEntry.Text ?? string.Empty,
            Director = DirectorEntry.Text ?? string.Empty,
            Genero = GeneroEntry.Text ?? string.Empty,
            AnhoLanzamiento = anho,
            RutaImagen = RutaImagenEntry.Text,
            Sinopsis = SinopsisEditor.Text
        };

        try
        {
            var ok = await _service.ActualizarPeliculaAsync(id, p);
            await DisplayAlert("Actualizar", ok ? "Actualizada" : "Fallo al actualizar", "OK");
            if (ok) await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnSubirPosterClicked(object sender, EventArgs e)
    {
        if (!int.TryParse(IdEntry.Text, out var id))
        {
            var idString = await DisplayPromptAsync("Subir poster", "Id de la pelicula:", initialValue: "");
            if (string.IsNullOrWhiteSpace(idString) || !int.TryParse(idString, out id)) return;
        }

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