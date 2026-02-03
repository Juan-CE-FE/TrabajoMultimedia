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
                await DisplayAlert("No encontrado", "No existe la película", "OK");
                return;
            }

            TituloEntry.Text = p.Titulo;
            DirectorEntry.Text = p.Director;
            GeneroEntry.Text = p.Genero;
            AnhoEntry.Text = p.AnhoLanzamiento.ToString();
            RutaImagenEntry.Text = p.RutaImagen;
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
            RutaImagen = RutaImagenEntry.Text
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
}