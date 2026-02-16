using GestionPeliculas.Service;

namespace GestionPeliculas.Pages;

public partial class DeletePeliculaPage : ContentPage
{
    private readonly PeliculasService _service;

    public DeletePeliculaPage(PeliculasService service)
    {
        InitializeComponent();
        _service = service;
    }

    private async void OnEliminarClicked(object sender, EventArgs e)
    {
        if (!int.TryParse(IdEntry.Text, out var id))
        {
            await DisplayAlert("Error", "Id inválido", "OK");
            return;
        }

        var confirmar = await DisplayAlert("Eliminar", $"Eliminar película {id}?", "Sí", "No");
        if (!confirmar) return;

        try
        {
            var ok = await _service.EliminarPeliculaAsync(id);
            await DisplayAlert("Eliminar", ok ? "Eliminada" : "Fallo al eliminar", "OK");
            if (ok) await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }


}