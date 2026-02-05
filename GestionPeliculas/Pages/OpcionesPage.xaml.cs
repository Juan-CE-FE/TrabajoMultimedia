using GestionPeliculas.Model;
using GestionPeliculas.Service;

namespace GestionPeliculas.Pages;

public partial class OpcionesPage : ContentPage
{
    private readonly PeliculasService _service;

    public OpcionesPage(PeliculasService service)
    {
        InitializeComponent();
        _service = service;
    }

    public void ShowListOnly()
    {
        ResultsList.IsVisible = true;
        CreateSection.IsVisible = false;
        ManageByIdSection.IsVisible = false;
    }

    public void ShowCreateOnly()
    {
        ResultsList.IsVisible = false;
        CreateSection.IsVisible = true;
        ManageByIdSection.IsVisible = false;
    }

    public void ShowManageById()
    {
        ResultsList.IsVisible = false;
        CreateSection.IsVisible = false;
        ManageByIdSection.IsVisible = true;
    }

    private async void OnListarClicked(object sender, EventArgs e)
    {
        try
        {
            var list = await _service.GetPeliculasAsync();
            ResultsList.ItemsSource = list;
            ShowListOnly();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void OnShowCreate(object sender, EventArgs e) => ShowCreateOnly();
    private void OnShowManageById(object sender, EventArgs e) => ShowManageById();

    private async void OnCrearClicked(object sender, EventArgs e)
    {
        if (!int.TryParse(Crear_Anho.Text, out var anho)) anho = 0;

        var p = new Pelicula
        {
            Titulo = Crear_Titulo.Text ?? string.Empty,
            Director = Crear_Director.Text ?? string.Empty,
            Genero = Crear_Genero.Text ?? string.Empty,
            AnhoLanzamiento = anho,
            RutaImagen = Crear_RutaImagen.Text
        };

        try
        {
            var ok = await _service.CrearPeliculaAsync(p);
            await DisplayAlert("Crear", ok ? "Creada correctamente" : "Fallo al crear", "OK");
            if (ok) await RefreshList();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
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

            // Rellenar campos de edición
            Editar_Titulo.Text = p.Titulo;
            Editar_Director.Text = p.Director;
            Editar_Genero.Text = p.Genero;
            Editar_Anho.Text = p.AnhoLanzamiento.ToString();
            Editar_RutaImagen.Text = p.RutaImagen;
            ShowManageById();
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

        if (!int.TryParse(Editar_Anho.Text, out var anho)) anho = 0;

        var p = new Pelicula
        {
            Id = id,
            Titulo = Editar_Titulo.Text ?? string.Empty,
            Director = Editar_Director.Text ?? string.Empty,
            Genero = Editar_Genero.Text ?? string.Empty,
            AnhoLanzamiento = anho,
            RutaImagen = Editar_RutaImagen.Text
        };

        try
        {
            var ok = await _service.ActualizarPeliculaAsync(id, p);
            await DisplayAlert("Actualizar", ok ? "Actualizada" : "Fallo al actualizar", "OK");
            if (ok) await RefreshList();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
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
            if (ok) await RefreshList();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task RefreshList()
    {
        try
        {
            ResultsList.ItemsSource = await _service.GetPeliculasAsync();
        }
        catch { /* Ignorar refresco en fallo */ }
    }

    private async void OnBuscarClicked(object sender, EventArgs e)
    {
        try
        {
            string? genero = Buscar_Genero.Text;
            int? anho = null;
            if (int.TryParse(Buscar_Anho.Text, out var parsed)) anho = parsed;
            string? director = Buscar_Director.Text;

            var results = await _service.SearchPeliculasAsync(genero, anho, director);
            ResultsList.ItemsSource = results;
            ShowListOnly();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnExportJsonClicked(object sender, EventArgs e)
    {
        await ExportAndSave("json");
    }

    private async void OnExportCsvClicked(object sender, EventArgs e)
    {
        await ExportAndSave("csv");
    }

    private async Task ExportAndSave(string format)
    {
        try
        {
            var data = await _service.ExportPeliculasAsync(format);
            if (data == null)
            {
                await DisplayAlert("Error", "No se pudo exportar.", "OK");
                return;
            }

            // Guardar en almacenamiento local del dispositivo (carpeta Descargas o equivalente)
#if ANDROID
            var filename = $"peliculas_export_{DateTime.Now:yyyyMMddHHmmss}.{format}";
            var path = System.IO.Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath, filename);
            System.IO.File.WriteAllBytes(path, data);
            await DisplayAlert("Exportar", $"Archivo guardado en: {path}", "OK");
#elif IOS || MACCATALYST
            var filename = $"peliculas_export_{DateTime.Now:yyyyMMddHHmmss}.{format}";
            var path = System.IO.Path.Combine(FileSystem.AppDataDirectory, filename);
            System.IO.File.WriteAllBytes(path, data);
            await DisplayAlert("Exportar", $"Archivo guardado en: {path}", "OK");
#else
            var filename = $"peliculas_export_{DateTime.Now:yyyyMMddHHmmss}.{format}";
            var path = System.IO.Path.Combine(FileSystem.AppDataDirectory, filename);
            System.IO.File.WriteAllBytes(path, data);
            await DisplayAlert("Exportar", $"Archivo guardado en: {path}", "OK");
#endif
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnSubirPosterClicked(object sender, EventArgs e)
    {
        // Pedir id y seleccionar imagen desde galería
        if (!int.TryParse(Crear_Anho.Text, out var possibleId))
        {
            // No tenemos id; solicitar al usuario
        }

        var idString = await DisplayPromptAsync("Subir póster", "Id de la película:", initialValue: "");
        if (string.IsNullOrWhiteSpace(idString) || !int.TryParse(idString, out var id)) return;

        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions { Title = "Seleccione imagen" });
            if (result == null) return;

            using var stream = await result.OpenReadAsync();
            var ok = await _service.UploadPosterAsync(id, stream, result.FileName);
            await DisplayAlert("Subir póster", ok ? "Subida correcta" : "Fallo al subir", "OK");
            if (ok) await RefreshList();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnDescargarPosterClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int id)
        {
            try
            {
                var data = await _service.DownloadPosterAsync(id);
                if (data == null)
                {
                    await DisplayAlert("Error", "No se pudo descargar el póster", "OK");
                    return;
                }

#if ANDROID
                // Guardar en galería (Android)
                var filename = $"poster_{id}_{DateTime.Now:yyyyMMddHHmmss}.jpg";
                var picturesDir = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath;
                var path = System.IO.Path.Combine(picturesDir, filename);
                System.IO.File.WriteAllBytes(path, data);

                // Insertar en la galería
                var mediaScanIntent = new Android.Content.Intent(Android.Content.Intent.ActionMediaScannerScanFile);
                var contentUri = Android.Net.Uri.FromFile(new Java.IO.File(path));
                mediaScanIntent.SetData(contentUri);
                Android.App.Application.Context.SendBroadcast(mediaScanIntent);

                await DisplayAlert("Descargar", $"Póster guardado en galería: {path}", "OK");
#elif IOS || MACCATALYST
                // En iOS/MacCatalyst guardar en app data (necesita permisos más avanzados para Photos)
                var filename = $"poster_{id}_{DateTime.Now:yyyyMMddHHmmss}.jpg";
                var path = System.IO.Path.Combine(FileSystem.AppDataDirectory, filename);
                System.IO.File.WriteAllBytes(path, data);
                await DisplayAlert("Descargar", $"Póster guardado: {path}", "OK");
#else
                var filename = $"poster_{id}_{DateTime.Now:yyyyMMddHHmmss}.jpg";
                var path = System.IO.Path.Combine(FileSystem.AppDataDirectory, filename);
                System.IO.File.WriteAllBytes(path, data);
                await DisplayAlert("Descargar", $"Póster guardado: {path}", "OK");
#endif
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }

    private async void OnLogin(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage(_service));
    }
}
