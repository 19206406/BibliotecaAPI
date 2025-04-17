using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers
{
    [Route("api/seguridad")]
    [ApiController]
    public class SeguridadController: ControllerBase 
    {
        private readonly IDataProtector protector;
        private readonly ITimeLimitedDataProtector protectorLimitadoPorTiempo;

        public SeguridadController(IDataProtectionProvider dataProtectionProvider)
        {
            protector = dataProtectionProvider.CreateProtector("SeguridadController"); // el string de proposito no es la llave 
            // es parte de la llave 
            protectorLimitadoPorTiempo = protector.ToTimeLimitedDataProtector(); 
        }

        [HttpGet("encriptar")]
        public ActionResult<string> Encriptar(string textoPlano)
        {
            var textoEncriptado = protectorLimitadoPorTiempo.Protect(textoPlano, lifetime: TimeSpan.FromSeconds(30)); // encriptar el texto 
            return Ok(textoEncriptado);
        }

        [HttpGet("desencriptar")]
        public ActionResult<string> Desencriptar(string textoEncriptado)
        {
            var textoDesencriptado = protectorLimitadoPorTiempo.Unprotect(textoEncriptado); // desencriptar el texto 
            return Ok(textoDesencriptado);
        }

        [HttpGet("encriptar-limitado-por-tiempo")]
        public ActionResult<string> EncriptarLimitadoPorTiempo(string textoPlano)
        {
            var textoEncriptado = protector.Protect(textoPlano); // encriptar el texto 
            return Ok(textoEncriptado);
        }

        [HttpGet("desencriptar-limitado-por-tiempo")]
        public ActionResult<string> DesencriptarLimitadoPorTiempo(string textoEncriptado)
        {
            var textoDesencriptado = protector.Unprotect(textoEncriptado); // desencriptar el texto 
            return Ok(textoDesencriptado);
        }
    }
}
