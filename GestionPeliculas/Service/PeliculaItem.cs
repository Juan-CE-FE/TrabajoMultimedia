using GestionPeliculas.Model;

namespace GestionPeliculas.Service
{
    public class PeliculaItem
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Director { get; set; } = string.Empty;
        public int AnhoLanzamiento { get; set; }
        public string Genero { get; set; } = string.Empty;
        public string? Sinopsis { get; set; }
        public string? RutaImagen { get; set; }
        public ImageSource? PosterImage { get; set; }

        public static PeliculaItem FromPelicula(Pelicula p)
        {
            return new PeliculaItem
            {
                Id = p.Id,
                Titulo = p.Titulo,
                Director = p.Director,
                AnhoLanzamiento = p.AnhoLanzamiento,
                Genero = p.Genero,
                Sinopsis = p.Sinopsis,
                RutaImagen = p.RutaImagen
            };
        }
    }
}