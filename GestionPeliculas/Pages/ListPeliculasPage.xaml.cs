using GestionPeliculas.Service;

namespace GestionPeliculas.Pages;

public partial class ListPeliculasPage : ContentPage
{
    private readonly PeliculasService _service;

    public ListPeliculasPage(PeliculasService service)
    {
        InitializeComponent();
        _service = service;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadPeliculasAsync();
    }

    private async Task LoadPeliculasAsync()
    {
        try
        {
            var peliculas = await _service.GetPeliculasAsync();
            ListaPeliculas.ItemsSource = peliculas;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}