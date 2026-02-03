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
    }
}
