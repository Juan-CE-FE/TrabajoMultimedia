using GestionPeliculas.Service;
using System.IO;

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
                ResultadosFrame.IsVisible = false;
                return;
            }

            // Mostrar el frame de resultados
            ResultadosFrame.IsVisible = true;

            // Asignar valores
            IdLabel.Text = p.Id.ToString();
            TituloLabel.Text = p.Titulo;
            DirectorLabel.Text = p.Director;
            GeneroLabel.Text = p.Genero;
            AnhoLabel.Text = p.AnhoLanzamiento.ToString();
            SinopsisLabel.Text = p.Sinopsis ?? "Sin sinopsis disponible";
            RutaImagenLabel.Text = p.RutaImagen ?? "Sin imagen";

            // Cargar imagen
            try
            {
                var bytes = await _service.DownloadPosterAsync(p.Id);
                if (bytes != null && bytes.Length > 0)
                {
                    ImagenPoster.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
                }
                else
                {
                    ImagenPoster.Source = null;
                }
            }
            catch
            {
                ImagenPoster.Source = null;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
            ResultadosFrame.IsVisible = false;
        }
    }
}