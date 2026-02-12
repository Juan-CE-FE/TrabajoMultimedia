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

    private (bool Ok, string Message) ValidateInputs()
    {
        var title = TituloEntry.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title)) return (false, "El título es obligatorio.");
        if (title.Length > 200) return (false, "El título es demasiado largo (máx. 200 caracteres).");

        var director = DirectorEntry.Text ?? string.Empty;
        if (director.Length > 100) return (false, "El nombre del director es demasiado largo (máx. 100 caracteres).");

        if (!int.TryParse(AnhoEntry.Text, out var anho)) anho = 0;
        if (anho != 0 && (anho < 1895 || anho > DateTime.Now.Year))
            return (false, $"Introduce un año válido (1895-{DateTime.Now.Year}).");

        var sinopsis = SinopsisEditor.Text ?? string.Empty;
        if (sinopsis.Length > 5000) return (false, "La sinopsis es demasiado larga.");

        return (true, string.Empty);
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
            SinopsisEditor.Text = p.Sinopsis;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnActualizarClicked(object sender, EventArgs e)
    {
        var (okVal, msg) = ValidateInputs();
        if (!okVal)
        {
            await DisplayAlert("Validación", msg, "OK");
            return;
        }

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
}