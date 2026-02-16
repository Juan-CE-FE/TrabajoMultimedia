using Microsoft.Extensions.Logging;
using GestionPeliculas.Pages;
using GestionPeliculas.Service;
using Microsoft.Maui.Devices;

namespace GestionPeliculas
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Configurar HttpClient
            builder.Services.AddHttpClient<PeliculasService>((serviceProvider, client) =>
            {
#if ANDROID
                if (DeviceInfo.DeviceType == DeviceType.Virtual)
                {
                    // ✅ EMULADOR: HTTPS en puerto 7013
                    client.BaseAddress = new Uri("https://10.0.2.2:7013/");
                }
                else
                {
                    // ✅ MÓVIL REAL: HTTP en puerto 5240 con tu IP
                    client.BaseAddress = new Uri("http://192.168.128.1:5240/");
                }
#else
                // ✅ WINDOWS: HTTP en localhost
                client.BaseAddress = new Uri("http://localhost:5240/");
#endif

                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
#if DEBUG && ANDROID
                return new HttpClientHandler
                {
                    // Necesario para HTTPS con certificado de desarrollo
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
#else
                return new HttpClientHandler();
#endif
            });

            // Registrar páginas
            builder.Services.AddTransient<MainMenuPage>();
            builder.Services.AddTransient<ListPeliculasPage>();
            builder.Services.AddTransient<CreatePeliculaPage>();
            builder.Services.AddTransient<GetByIdPage>();
            builder.Services.AddTransient<UpdatePeliculaPage>();
            builder.Services.AddTransient<DeletePeliculaPage>();
            builder.Services.AddTransient<OpcionesPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<PeliculasPage>();
            builder.Services.AddTransient<DetallePeliculaPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}