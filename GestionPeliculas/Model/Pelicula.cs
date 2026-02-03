using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionPeliculas.Model
{
    public class Pelicula
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Director { get; set; } = string.Empty;
        public int AnhoLanzamiento { get; set; }
        public string Genero { get; set; } = string.Empty;
        public string? Sinopsis { get; set; }
        public string? RutaImagen { get; set; }
    }
}