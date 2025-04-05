using BibliotecaAPI.Entidades;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Datos
{
    public class ApplicationDbContext: DbContext // pieza central de Entity Framework Core 
        // donde se haran configuraciones con la base de datos 
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        // especificar las tablas de la bd
        public DbSet<Autor> Autores { get; set; }
        public DbSet<Libro> Libros { get; set; } // nueva tabla
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<AutorLibro> autoresLibros { get; set; }
    }
}
