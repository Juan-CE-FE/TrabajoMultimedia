using GestionPeliculas.Service;

namespace GestionPeliculas.Pages;

public partial class GetByIdPage : ContentPage
{
    private readonly PeliculasService _service;

    public GetByIdPage(PeliculasService service)
    {
        InitializeComponent();
        _service = service;
    }

    private async void OnObtenerClicked(object sender, EventArgs e)
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
                LimpiarLabels();
                return;
            }

            TituloLabel.Text = $"Título: {p.Titulo}";
            DirectorLabel.Text = $"Director: {p.Director}";
            GeneroLabel.Text = $"Género: {p.Genero}";
            AnhoLabel.Text = $"Año: {p.AnhoLanzamiento}";

            // ✅ AÑADIR MÁS INFORMACIÓN
            var sinopsisLabel = this.FindByName<Label>("SinopsisLabel");
            var rutaImagenLabel = this.FindByName<Label>("RutaImagenLabel");

            if (sinopsisLabel != null)
                sinopsisLabel.Text = $"Sinopsis: {p.Sinopsis ?? "No disponible"}";

            if (rutaImagenLabel != null)
                rutaImagenLabel.Text = $"Imagen: {p.RutaImagen ?? "No disponible"}";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
            LimpiarLabels();
        }
    }

    private void LimpiarLabels()
    {
        TituloLabel.Text = string.Empty;
        DirectorLabel.Text = string.Empty;
        GeneroLabel.Text = string.Empty;
        AnhoLabel.Text = string.Empty;

        var sinopsisLabel = this.FindByName<Label>("SinopsisLabel");
        var rutaImagenLabel = this.FindByName<Label>("RutaImagenLabel");

        if (sinopsisLabel != null)
            sinopsisLabel.Text = string.Empty;

        if (rutaImagenLabel != null)
            rutaImagenLabel.Text = string.Empty;
    }
}