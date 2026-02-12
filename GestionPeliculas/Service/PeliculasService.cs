using GestionPeliculas.Model;
using System.Net.Http;
using System.Net.Http.Json;
using Newtonsoft.Json;
using System.Text;

namespace GestionPeliculas.Service
{
    public class PeliculasService
    {
        private readonly HttpClient _client;

        public PeliculasService(HttpClient client)
        {
            _client = client;
        }

        // ==================== CRUD ====================

        public async Task<List<Pelicula>> GetPeliculasAsync()
        {
            try
            {
                var response = await _client.GetAsync("api/peliculas");
                if (!response.IsSuccessStatusCode)
                    return new List<Pelicula>();

                var json = await response.Content.ReadAsStringAsync();
                var peliculas = JsonConvert.DeserializeObject<List<Pelicula>>(json) ?? new List<Pelicula>();

                await AsegurarUrlsImagenes(peliculas);
                return peliculas;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetPeliculasAsync: {ex}");
                return new List<Pelicula>();
            }
        }

        public async Task<Pelicula?> GetPeliculaAsync(int id)
        {
            try
            {
                var response = await _client.GetAsync($"api/peliculas/{id}");
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                var pelicula = JsonConvert.DeserializeObject<Pelicula>(json);

                if (pelicula != null)
                    await AsegurarUrlsImagenes(new List<Pelicula> { pelicula });

                return pelicula;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> CrearPeliculaAsync(Pelicula pelicula)
        {
            try
            {
                var response = await _client.PostAsJsonAsync("api/peliculas", pelicula);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ActualizarPeliculaAsync(int id, Pelicula pelicula)
        {
            try
            {
                var response = await _client.PutAsJsonAsync($"api/peliculas/{id}", pelicula);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EliminarPeliculaAsync(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"api/peliculas/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ==================== BÚSQUEDAS ====================

        public async Task<List<Pelicula>> SearchByGeneroAsync(string genero)
        {
            try
            {
                var response = await _client.GetAsync($"api/peliculas/genero/{Uri.EscapeDataString(genero)}");
                if (!response.IsSuccessStatusCode) return new List<Pelicula>();

                var json = await response.Content.ReadAsStringAsync();
                var list = JsonConvert.DeserializeObject<List<Pelicula>>(json) ?? new List<Pelicula>();
                await AsegurarUrlsImagenes(list);
                return list;
            }
            catch
            {
                return new List<Pelicula>();
            }
        }

        public async Task<List<Pelicula>> SearchByAnhoAsync(int anho)
        {
            try
            {
                var response = await _client.GetAsync($"api/peliculas/anho/{anho}");
                if (!response.IsSuccessStatusCode) return new List<Pelicula>();

                var json = await response.Content.ReadAsStringAsync();
                var list = JsonConvert.DeserializeObject<List<Pelicula>>(json) ?? new List<Pelicula>();
                await AsegurarUrlsImagenes(list);
                return list;
            }
            catch
            {
                return new List<Pelicula>();
            }
        }

        public async Task<List<Pelicula>> SearchByDirectorAsync(string director)
        {
            try
            {
                var response = await _client.GetAsync($"api/peliculas/director/{Uri.EscapeDataString(director)}");
                if (!response.IsSuccessStatusCode) return new List<Pelicula>();

                var json = await response.Content.ReadAsStringAsync();
                var list = JsonConvert.DeserializeObject<List<Pelicula>>(json) ?? new List<Pelicula>();
                await AsegurarUrlsImagenes(list);
                return list;
            }
            catch
            {
                return new List<Pelicula>();
            }
        }

        // ==================== GESTIÓN DE PÓSTERS ====================

        public async Task<bool> UploadPosterAsync(int id, Stream imageStream, string fileName)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                var streamContent = new StreamContent(imageStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                content.Add(streamContent, "archivo", fileName);

                var response = await _client.PostAsync($"api/peliculas/{id}/imagen", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error UploadPosterAsync: {ex}");
                return false;
            }
        }

        public async Task<byte[]?> DownloadPosterAsync(int id)
        {
            try
            {
                var response = await _client.GetAsync($"api/peliculas/{id}/descargar-poster");
                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error DownloadPosterAsync: {ex}");
                return null;
            }
        }

        // ==================== EXPORTACIONES ====================

        public async Task<byte[]?> ExportarJsonAsync()
        {
            try
            {
                var response = await _client.GetAsync("api/peliculas/exportar/json");
                return response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync() : null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<byte[]?> ExportarCsvAsync()
        {
            try
            {
                var response = await _client.GetAsync("api/peliculas/exportar/csv");
                return response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync() : null;
            }
            catch
            {
                return null;
            }
        }

        // ==================== AUTENTICACIÓN ====================

        public void SetBasicAuth(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _client.DefaultRequestHeaders.Authorization = null;
                return;
            }

            var credentials = $"{username}:{password}";
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64);
        }

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            SetBasicAuth(username, password);
            try
            {
                var response = await _client.GetAsync("api/peliculas");
                if (response.IsSuccessStatusCode)
                    return true;

                _client.DefaultRequestHeaders.Authorization = null;
                return false;
            }
            catch
            {
                _client.DefaultRequestHeaders.Authorization = null;
                return false;
            }
        }

        // ==================== UTILIDADES ====================

        private async Task AsegurarUrlsImagenes(IEnumerable<Pelicula> peliculas)
        {
            if (peliculas == null) return;

            foreach (var p in peliculas)
            {
                if (string.IsNullOrEmpty(p.RutaImagen))
                {
                    var poster = await DownloadPosterAsync(p.Id);
                    if (poster != null)
                    {
                        p.RutaImagen = $"{_client.BaseAddress}api/peliculas/{p.Id}/poster";
                    }
                }
                else if (!Uri.IsWellFormedUriString(p.RutaImagen, UriKind.Absolute))
                {
                    p.RutaImagen = $"{_client.BaseAddress}api/peliculas/{p.Id}/poster";
                }
            }
        }
    }
}