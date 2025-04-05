using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.Entidades
{
    public class Comentario
    {
        public Guid Id { get; set; } // numero asta muy grandes
        [Required]
        public required string Cuerpo { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public int LibroId { get; set; }
        public Libro? Libro { get; set; }
    }
}
