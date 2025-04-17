namespace BibliotecaAPI.DTOs
{
    public class UsuarioDTO
    {
        public required string Email { get; set; } // email del usuario
        public DateTime FechaNacimiento { get; set; } // fecha de nacimiento del usuario
    }
}
