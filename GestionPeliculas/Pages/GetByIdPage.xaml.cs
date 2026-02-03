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
                return;
            }

            TituloLabel.Text = p.Titulo;
            DirectorLabel.Text = p.Director;
            GeneroLabel.Text = p.Genero;
            AnhoLabel.Text = p.AnhoLanzamiento.ToString();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}