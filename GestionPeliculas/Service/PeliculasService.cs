using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.Http.Json;
using Newtonsoft.Json;
using GestionPeliculas.Model;
using Microsoft.Maui.Storage;

namespace GestionPeliculas.Service
{
    public class PeliculasService
    {
        private readonly HttpClient _client;

        public PeliculasService(HttpClient client)
        {
            _client = client;

            // RESTAURAR AUTENTICACIÓN GUARDADA
            RestaurarAutenticacion();
        }

        private void RestaurarAutenticacion()
        {
            try
            {
                var authHeader = Preferences.Get("AuthHeader", "");
                if (!string.IsNullOrEmpty(authHeader))
                {
                    _client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);
                    System.Diagnostics.Debug.WriteLine("🔐 Autenticación restaurada desde Preferences");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restaurando auth: {ex.Message}");
            }
        }

        // ==================== AUTENTICACIÓN ====================

        public void SetBasicAuth(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _client.DefaultRequestHeaders.Authorization = null;
                Preferences.Remove("AuthHeader");
                return;
            }

            var credentials = $"{username}:{password}";
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64);

            // GUARDAR EN PREFERENCES
            Preferences.Set("AuthHeader", base64);

            System.Diagnostics.Debug.WriteLine($"🔐 Auth establecida para: {username}");
        }

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            SetBasicAuth(username, password);

            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 Verificando credenciales: {username}");

                var response = await _client.GetAsync("api/peliculas");

                System.Diagnostics.Debug.WriteLine($"📡 Login response: {(int)response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Login exitoso - credenciales válidas");
                    return true;
                }

                System.Diagnostics.Debug.WriteLine($"❌ Login falló: {response.StatusCode}");
                _client.DefaultRequestHeaders.Authorization = null;
                Preferences.Remove("AuthHeader");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚨 Error en login: {ex.Message}");
                _client.DefaultRequestHeaders.Authorization = null;
                Preferences.Remove("AuthHeader");
                return false;
            }
        }

        // ==================== CRUD ====================

        public async Task<List<Pelicula>> GetPeliculasAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=========================================");
                System.Diagnostics.Debug.WriteLine($"🔍 GetPeliculasAsync");
                System.Diagnostics.Debug.WriteLine($"🌐 URL: {_client.BaseAddress}api/peliculas");
                System.Diagnostics.Debug.WriteLine($"🔐 Auth Header: {_client.DefaultRequestHeaders.Authorization?.ToString() ?? "NO AUTH"}");

                var response = await _client.GetAsync("api/peliculas");

                System.Diagnostics.Debug.WriteLine($"📡 Código de respuesta: {(int)response.StatusCode} {response.StatusCode}");

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"📦 Contenido: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error: {response.StatusCode} - {content}");

                    // SI ES 401, INTENTAR RESTAURAR AUTENTICACIÓN
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        System.Diagnostics.Debug.WriteLine("🔐 401 detectado - intentando restaurar auth...");
                        RestaurarAutenticacion();

                        // REINTENTAR
                        response = await _client.GetAsync("api/peliculas");
                        content = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            System.Diagnostics.Debug.WriteLine("✅ Restauración exitosa");
                        }
                        else
                        {
                            return new List<Pelicula>();
                        }
                    }
                    else
                    {
                        return new List<Pelicula>();
                    }
                }

                var peliculas = JsonConvert.DeserializeObject<List<Pelicula>>(content) ?? new List<Pelicula>();
                System.Diagnostics.Debug.WriteLine($"✅ {peliculas.Count} películas recibidas");

                await AsegurarUrlsImagenes(peliculas);
                return peliculas;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚨 Error: {ex.Message}");
                return new List<Pelicula>();
            }
        }

        public async Task<Pelicula?> GetPeliculaAsync(int id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=========================================");
                System.Diagnostics.Debug.WriteLine($"🔍 GetPeliculaAsync ID: {id}");
                System.Diagnostics.Debug.WriteLine($"🌐 URL: {_client.BaseAddress}api/peliculas/{id}");
                System.Diagnostics.Debug.WriteLine($"🔐 Auth Header: {_client.DefaultRequestHeaders.Authorization?.ToString() ?? "NO AUTH"}");

                var response = await _client.GetAsync($"api/peliculas/{id}");

                System.Diagnostics.Debug.WriteLine($"📡 Código de respuesta: {(int)response.StatusCode} {response.StatusCode}");

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"📦 Contenido: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error: {response.StatusCode} - {content}");

                    // SI ES 401, INTENTAR RESTAURAR AUTENTICACIÓN
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        System.Diagnostics.Debug.WriteLine("🔐 401 detectado - intentando restaurar auth...");
                        RestaurarAutenticacion();

                        // REINTENTAR
                        response = await _client.GetAsync($"api/peliculas/{id}");
                        content = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            System.Diagnostics.Debug.WriteLine("✅ Restauración exitosa");
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }

                var pelicula = JsonConvert.DeserializeObject<Pelicula>(content);

                if (pelicula != null)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Película encontrada: {pelicula.Titulo}");
                    await AsegurarUrlsImagenes(new List<Pelicula> { pelicula });
                }

                return pelicula;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚨 Error: {ex.Message}");
                return null;
            }
        }

        // RESTO DE MÉTODOS (Crear, Actualizar, Eliminar, Búsquedas, Posters, Exportaciones)
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