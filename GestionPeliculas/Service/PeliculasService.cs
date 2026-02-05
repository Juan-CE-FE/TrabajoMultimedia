using GestionPeliculas.Model;
using System.Net.Http;
using System.Net.Http.Json;
using Newtonsoft.Json;

namespace GestionPeliculas.Service
{
    public class PeliculasService
    {
        private readonly HttpClient client;

        public PeliculasService(HttpClient client)
        {
            this.client = client ?? new HttpClient();
        }

        // GET: api/peliculas
        public async Task<List<Pelicula>> GetPeliculasAsync()
        {
            try
            {
                // Instrumented HTTP call for debugging
                var requestUri = "api/peliculas";
                System.Diagnostics.Debug.WriteLine($"Llamando a: {client.BaseAddress}{requestUri}");

                var response = await client.GetAsync(requestUri);
                var body = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"HTTP {(int)response.StatusCode} - {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Respuesta cuerpo: {body}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Error {(int)response.StatusCode}: {response.ReasonPhrase}. Body: {body}");
                }

                // Intentar deserializar el JSON recibido
                try
                {
                    var peliculas = JsonConvert.DeserializeObject<List<Pelicula>>(body);
                    return peliculas ?? new List<Pelicula>();
                }
                catch (Exception jsonEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deserializando JSON: {jsonEx}");
                    throw new Exception("Respuesta de la API no contiene JSON válido de películas.", jsonEx);
                }
            }
            catch (TaskCanceledException tex)
            {
                System.Diagnostics.Debug.WriteLine($"Timeout o cancelado: {tex}");
                throw;
            }
            catch (HttpRequestException hex)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP EXCEPTION: {hex}");
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPCIÓN: {ex}");
                throw;
            }
        }

        // GET: api/peliculas/{id}
        public async Task<Pelicula?> GetPeliculaAsync(int id)
        {
            return await client.GetFromJsonAsync<Pelicula>($"api/peliculas/{id}");
        }

        // POST: api/peliculas
        public async Task<bool> CrearPeliculaAsync(Pelicula pelicula)
        {
            var response = await client.PostAsJsonAsync("api/peliculas", pelicula);
            return response.IsSuccessStatusCode;
        }

        // PUT: api/peliculas/{id}
        public async Task<bool> ActualizarPeliculaAsync(int id, Pelicula pelicula)
        {
            var response = await client.PutAsJsonAsync($"api/peliculas/{id}", pelicula);
            return response.IsSuccessStatusCode;
        }

        // DELETE: api/peliculas/{id}
        public async Task<bool> EliminarPeliculaAsync(int id)
        {
            var response = await client.DeleteAsync($"api/peliculas/{id}");
            return response.IsSuccessStatusCode;
        }

        // SEARCH: api/peliculas/search?genero=&anho=&director=
        public async Task<List<Pelicula>> SearchPeliculasAsync(string? genero = null, int? anho = null, string? director = null)
        {
            var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrWhiteSpace(genero)) query["genero"] = genero;
            if (anho.HasValue) query["anho"] = anho.Value.ToString();
            if (!string.IsNullOrWhiteSpace(director)) query["director"] = director;

            var requestUri = "api/peliculas/search" + (query.Count > 0 ? "?" + query.ToString() : string.Empty);
            var response = await client.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode) return new List<Pelicula>();
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Pelicula>>(body) ?? new List<Pelicula>();
        }

        // EXPORT: api/peliculas/export?format=json|csv
        public async Task<byte[]?> ExportPeliculasAsync(string format = "json")
        {
            var requestUri = $"api/peliculas/export?format={format}";
            var response = await client.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadAsByteArrayAsync();
        }

        // Upload poster: POST api/peliculas/{id}/poster
        public async Task<bool> UploadPosterAsync(int id, Stream imageStream, string fileName)
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(imageStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content.Add(streamContent, "poster", fileName);

            var response = await client.PostAsync($"api/peliculas/{id}/poster", content);
            return response.IsSuccessStatusCode;
        }

        // Download poster: GET api/peliculas/{id}/poster
        public async Task<byte[]?> DownloadPosterAsync(int id)
        {
            var response = await client.GetAsync($"api/peliculas/{id}/poster");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadAsByteArrayAsync();
        }

        // Allow setting basic auth at runtime from the app (login screen)
        public void SetBasicAuth(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                client.DefaultRequestHeaders.Authorization = null;
                return;
            }

            var credentials = $"{username}:{password}";
            var byteArray = System.Text.Encoding.UTF8.GetBytes(credentials);
            var base64Credentials = Convert.ToBase64String(byteArray);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64Credentials);
        }
    }
}
