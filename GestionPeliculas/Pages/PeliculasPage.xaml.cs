using GestionPeliculas.Service;
using GestionPeliculas.Model;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using System.IO;

namespace GestionPeliculas.Pages
{
    public partial class PeliculasPage : ContentPage
    {
        private readonly PeliculasService _service;
        private readonly IServiceProvider _services;

        public PeliculasPage(PeliculasService service, IServiceProvider services)
        {
            InitializeComponent();
            _service = service;
            _services = services;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarPeliculasAsync();
            await ExportarAutomaticoAsync(); // Exportación automática
        }

        private async Task CargarPeliculasAsync()
        {
            try
            {
                var peliculas = await _service.GetPeliculasAsync();

                var items = new List<PeliculaItem>();
                foreach (var p in peliculas)
                {
                    var item = PeliculaItem.FromPelicula(p);

                    // Cargar imagen automáticamente
                    try
                    {
                        var bytes = await _service.DownloadPosterAsync(p.Id);
                        if (bytes != null && bytes.Length > 0)
                        {
                            var stream = new MemoryStream(bytes);
                            item.PosterImage = ImageSource.FromStream(() => new MemoryStream(bytes));

                            // AUTO GUARDAR EN GALERÍA (requisito)
                            await GuardarEnGaleriaAsync(bytes, $"poster_{p.Id}.png");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error cargando póster {p.Id}: {ex}");
                    }

                    items.Add(item);
                }

                ListaPeliculas.ItemsSource = items;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudieron cargar las películas: {ex.Message}", "OK");
            }
        }

        // AUTO GUARDADO EN GALERÍA (SIN CLICK)
        private async Task GuardarEnGaleriaAsync(byte[] imageBytes, string filename)
        {
            try
            {
#if ANDROID
                var context = Android.App.Application.Context;
                var resolver = context.ContentResolver;

                var values = new Android.Content.ContentValues();
                values.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, filename);
                values.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "image/png");
                values.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, Android.OS.Environment.DirectoryPictures);

                var uri = resolver.Insert(Android.Provider.MediaStore.Images.Media.ExternalContentUri, values);
                using var stream = resolver.OpenOutputStream(uri);
                await stream.WriteAsync(imageBytes, 0, imageBytes.Length);

                System.Diagnostics.Debug.WriteLine($"Póster guardado automáticamente en galería: {filename}");
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error guardando en galería: {ex}");
            }
        }

        // EXPORTACIÓN AUTOMÁTICA (SIN CLICK)
        private async Task ExportarAutomaticoAsync()
        {
            try
            {
                // Exportar JSON automáticamente
                var jsonBytes = await _service.ExportarJsonAsync();
                if (jsonBytes != null)
                {
                    var jsonPath = Path.Combine(FileSystem.AppDataDirectory, "peliculas_automaticas.json");
                    await File.WriteAllBytesAsync(jsonPath, jsonBytes);
                    System.Diagnostics.Debug.WriteLine($"JSON exportado automáticamente: {jsonPath}");
                }

                // Exportar CSV automáticamente
                var csvBytes = await _service.ExportarCsvAsync();
                if (csvBytes != null)
                {
                    var csvPath = Path.Combine(FileSystem.AppDataDirectory, "peliculas_automaticas.csv");
                    await File.WriteAllBytesAsync(csvPath, csvBytes);
                    System.Diagnostics.Debug.WriteLine($"CSV exportado automáticamente: {csvPath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exportación automática: {ex}");
            }
        }

        private async void OnOpcionesClicked(object sender, EventArgs e)
        {
            var opcionesPage = _services.GetService(typeof(OpcionesPage)) as Page;
            if (opcionesPage != null)
                await Navigation.PushAsync(opcionesPage);
        }
    }
}