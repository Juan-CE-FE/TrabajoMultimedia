using Microsoft.Extensions.Logging;
using GestionPeliculas.Pages;
using GestionPeliculas.Service;
using System.Net.Http.Headers;

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
                client.BaseAddress = new Uri("https://10.0.2.2:7013/");
#else
                client.BaseAddress = new Uri("https://localhost:7013/");
#endif

                // Autenticación básica por defecto
                var credentials = "juan:123";
                var byteArray = System.Text.Encoding.UTF8.GetBytes(credentials);
                var base64Credentials = Convert.ToBase64String(byteArray);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", base64Credentials);

                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
#if DEBUG && ANDROID
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
#else
                return new HttpClientHandler();
#endif
            })
            .AddHttpMessageHandler<LoggingHttpHandler>(); // ✅ AÑADIDO

            // Registrar servicios
            builder.Services.AddSingleton<LoggingHttpHandler>();

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

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }

    // ✅ NUEVO: Handler para logging HTTP
    public class LoggingHttpHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine($"➡️ Request: {request.Method} {request.RequestUri}");

            if (request.Content != null)
            {
                var content = await request.Content.ReadAsStringAsync(cancellationToken);
                System.Diagnostics.Debug.WriteLine($"📦 Body: {content}");
            }

            var response = await base.SendAsync(request, cancellationToken);

            System.Diagnostics.Debug.WriteLine($"⬅️ Response: {(int)response.StatusCode} {response.StatusCode}");

            return response;
        }
    }
}