using BibliotecaAPI.Servicios;
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
        private readonly IServicioHash servicioHash;

        public SeguridadController(IDataProtectionProvider dataProtectionProvider, IServicioHash servicioHash)
        {
            protector = dataProtectionProvider.CreateProtector("SeguridadController"); // el string de proposito no es la llave 
            // es parte de la llave 
            protectorLimitadoPorTiempo = protector.ToTimeLimitedDataProtector();
            this.servicioHash = servicioHash; // inicializamos el servicio de hash
        }

        [HttpGet("hash")]
        public ActionResult<string> Hash(string textoPlano)
        {
            var hash1 = servicioHash.Hash(textoPlano); // hacer el hash del texto plano
            var hash2 = servicioHash.Hash(textoPlano); // hacer el hash del texto plano
            var hash3 = servicioHash.Hash(textoPlano, hash2.Sal); // es el mismo que el hash2 sirve para hacer una comparación
            
            var resultado = new {textoPlano, hash1, hash2, hash3 }; // crear un objeto anonimo con los resultados

            return Ok(resultado); // devolver el resultado
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
