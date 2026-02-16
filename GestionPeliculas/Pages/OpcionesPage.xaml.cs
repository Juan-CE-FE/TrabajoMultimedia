using GestionPeliculas.Service;
using Microsoft.Maui.Storage;

namespace GestionPeliculas.Pages;

public partial class OpcionesPage : ContentPage
{
    private readonly PeliculasService _service;
    private readonly IServiceProvider _services;

    public OpcionesPage(PeliculasService service, IServiceProvider services)
    {
        InitializeComponent();
        _service = service;
        _services = services;
    }

    // ========== VOLVER ==========
    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    // ========== EXPORTAR JSON ==========
    private async void OnExportJsonClicked(object sender, EventArgs e)
    {
        await Exportar("json");
    }

    // ========== EXPORTAR CSV ==========
    private async void OnExportCsvClicked(object sender, EventArgs e)
    {
        await Exportar("csv");
    }

    private async Task Exportar(string formato)
    {
        try
        {
            byte[]? data = formato == "json"
                ? await _service.ExportarJsonAsync()
                : await _service.ExportarCsvAsync();

            if (data == null)
            {
                await DisplayAlert("Error", "No se pudo exportar", "OK");
                return;
            }

            var filename = $"peliculas_{DateTime.Now:yyyyMMddHHmmss}.{formato}";

#if ANDROID
            // Pedir permisos de almacenamiento
            var status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permiso", "Se necesita permiso de almacenamiento", "OK");
                return;
            }

            // Guardar en Downloads
            var downloads = Android.OS.Environment.GetExternalStoragePublicDirectory(
                Android.OS.Environment.DirectoryDownloads).AbsolutePath;
            var path = Path.Combine(downloads, filename);

            await File.WriteAllBytesAsync(path, data);

            await DisplayAlert("Exportado correctament",
                $"Archivo guardado en:\nDescargas/{filename}", "OK");
#else
        var path = Path.Combine(FileSystem.AppDataDirectory, filename);
        await File.WriteAllBytesAsync(path, data);
        await DisplayAlert("✅ Exportación exitosa",
            $"Archivo guardado en:\n{path}", "OK");
#endif
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // ========== SUBIR PÓSTER ==========
    private async void OnSubirPosterClicked(object sender, EventArgs e)
    {
        // Validar ID
        if (string.IsNullOrWhiteSpace(SubirIdEntry.Text) ||
            !int.TryParse(SubirIdEntry.Text, out var id))
        {
            await DisplayAlert("Error", "ID inválido", "OK");
            return;
        }

        try
        {
            // Pedir permiso
            var status = await Permissions.RequestAsync<Permissions.Photos>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permiso", "Se necesita acceso a la galería", "OK");
                return;
            }

            // Seleccionar imagen
            var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Selecciona un póster"
            });

            if (result == null) return;

            // Subir imagen
            using var stream = await result.OpenReadAsync();
            var ok = await _service.UploadPosterAsync(id, stream, result.FileName);

            if (ok)
            {
                await DisplayAlert("✅ Éxito", "Póster subido correctamente", "OK");
                SubirIdEntry.Text = string.Empty; // Limpiar campo
            }
            else
            {
                await DisplayAlert("❌ Error", "No se pudo subir el póster", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // ========== DESCARGAR PÓSTER ==========
    private async void OnDescargarPosterClicked(object sender, EventArgs e)
    {
        // Validar ID
        if (string.IsNullOrWhiteSpace(DescargarIdEntry.Text) ||
            !int.TryParse(DescargarIdEntry.Text, out var id))
        {
            await DisplayAlert("Error", "ID inválido", "OK");
            return;
        }

        try
        {
            // Descargar imagen
            var data = await _service.DownloadPosterAsync(id);
            if (data == null)
            {
                await DisplayAlert("Error", "No se pudo descargar el póster", "OK");
                return;
            }

            // Guardar en galería (Android)
            var filename = $"poster_{id}_{DateTime.Now:yyyyMMddHHmmss}.png";

#if ANDROID
            var context = Android.App.Application.Context;
            var resolver = context.ContentResolver;

            var values = new Android.Content.ContentValues();
            values.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, filename);
            values.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "image/png");
            values.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, Android.OS.Environment.DirectoryPictures);

            var uri = resolver.Insert(Android.Provider.MediaStore.Images.Media.ExternalContentUri, values);
            using var stream = resolver.OpenOutputStream(uri);
            await stream.WriteAsync(data, 0, data.Length);

            await DisplayAlert("✅ Descarga completada",
                "Póster guardado en la galería", "OK");
#else
            var path = Path.Combine(FileSystem.AppDataDirectory, filename);
            await File.WriteAllBytesAsync(path, data);
            await DisplayAlert("✅ Descarga completada", 
                $"Póster guardado en:\n{path}", "OK");
#endif

            DescargarIdEntry.Text = string.Empty; // Limpiar campo
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}