using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Datos
{
    public class ApplicationDbContext: IdentityDbContext //nueva forma de manejar la base de datos con Identity  
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // código para hacer las migraciones
        }

        // especificar las tablas de la bd
        public DbSet<Autor> Autores { get; set; }
        public DbSet<Libro> Libros { get; set; } // nueva tabla
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<AutorLibro> autoresLibros { get; set; }
    }
}
