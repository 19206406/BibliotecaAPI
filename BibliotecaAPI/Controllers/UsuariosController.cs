using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Validations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    [Authorize] // Requiere autenticacion para acceder a las acciones
    public class UsuariosController: ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<IdentityUser> signInManager;

        public UsuariosController(UserManager<IdentityUser> userManager, IConfiguration configuration, 
            SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
        }

        [HttpPost("registro")]
        [AllowAnonymous] // permite el acceso a esta accion sin autenticacion
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Registrar(
            CredencialesUsuarioDTO credencialesUsuarioDTO)
        {
            var usuario = new IdentityUser // creación del usuario con Identity 
            {
                UserName = credencialesUsuarioDTO.Email,
                Email = credencialesUsuarioDTO.Email,
            };

            var resultado = await userManager.CreateAsync(usuario, credencialesUsuarioDTO.Password); // pasarle el password

            if (resultado.Succeeded)
            {
                var respuestaAutenticacion = await ConstruirToken(credencialesUsuarioDTO); // crear el token
                return respuestaAutenticacion; // devolver el token
            }
            else
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description); // si no se ha creado el usuario, se le devuelve el error
                }

                return ValidationProblem();
            }
        }

        [HttpPost("login")]
        [AllowAnonymous] // permite el acceso a esta accion sin autenticacion
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Login(
            CredencialesUsuarioDTO credencialesUsuarioDTO)
        {
            var usuario = await userManager.FindByEmailAsync(credencialesUsuarioDTO.Email); // buscar el usuario por email

            if (usuario is null)
            {
                return RetornarLoginIncorrecto(); 
            }

            var resultado = await signInManager.CheckPasswordSignInAsync(usuario,
                credencialesUsuarioDTO.Password!, lockoutOnFailure: false); // comprobar si el password es correcto

            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuarioDTO); // retornar el login
            }
            else
            {
                return RetornarLoginIncorrecto(); // retornar el login incorrecto
            }
        }

        private ActionResult RetornarLoginIncorrecto()
        {
            ModelState.AddModelError(string.Empty, "Login incorrecto");
            return ValidationProblem(); 
        }

        // proceso para crear el token y ese tipo de cosas 
        private async Task<RespuestaAutenticacionDTO> ConstruirToken(CredencialesUsuarioDTO credencialesUsuarioDTO) 
        {
            // crear los claims info del usuario 

            var claims = new List<Claim>
            {
                new Claim("email", credencialesUsuarioDTO.Email),
                new Claim("lo que yo quiera", "lo que yo quiera"),
            }; 

            var usuario = await userManager.FindByEmailAsync(credencialesUsuarioDTO.Email); // buscar el usuario por email
            var claimsDB = await userManager.GetClaimsAsync(usuario); // buscar los claims del usuario

            claims.AddRange(claimsDB); // añadir los claims del usuario a los claims

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llavejwt"]!)); // crear la llave simetrica
            var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256); // crear las credenciales de firma

            var expiracion = DateTime.UtcNow.AddYears(1); // crear la expiración del token
            var tokenDeSeguridad = new JwtSecurityToken(issuer: null, audience: null,
                claims: claims, expires: expiracion, signingCredentials: credenciales);

            var token = new JwtSecurityTokenHandler().WriteToken(tokenDeSeguridad); // crear el token

            return new RespuestaAutenticacionDTO
            {
                Token = token,
                Expiracion = expiracion
            }; 
        }
    }
}
