using BibliotecaAPI.Entidades;

namespace BibliotecaAPI.DTOs
{
    public class AutorConLibrosDTO: AutorDTO
    { 
        public List<Libro> libros { get; set; } = []; // sintaxis nueva de C# 
    }
}
