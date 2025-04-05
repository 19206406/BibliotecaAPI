namespace BibliotecaAPI.DTOs
{
    public class AutorDTO
    {
        // estos son los datos que quiero que el usuario vea del autor
        public int Id {  get; set; }
        public required string NombreCompleto { get; set; }
    }
}
