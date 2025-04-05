using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Entidades
{
    [PrimaryKey(nameof(AutorId), nameof(LibroId))] // llave primaria compuesta entre AutorId y LibroId 
    public class AutorLibro
    {
        public int AutorId { get; set; }
        public int LibroId { get; set; }
        public int Orden {  get; set; }
        public Autor? Autor { get; set; }
        public Libro? Libro { get; set; }
    }
}
