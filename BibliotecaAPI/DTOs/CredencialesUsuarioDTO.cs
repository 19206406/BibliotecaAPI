using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs
{
    public class CredencialesUsuarioDTO // datos para poder hacer login 
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public string Password { get; set; }

    }
}
