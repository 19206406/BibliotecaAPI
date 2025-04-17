using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Validations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    public class UsuariosController: ControllerBase
    {
        private readonly UserManager<Usuario> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<Usuario> signInManager;
        private readonly IServiciosUsuarios serviciosUsuarios;
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public UsuariosController(UserManager<Usuario> userManager, IConfiguration configuration, 
            SignInManager<Usuario> signInManager, IServiciosUsuarios serviciosUsuarios, ApplicationDbContext context, IMapper mapper)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.serviciosUsuarios = serviciosUsuarios;
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "esadmin")] // requiere autenticacion para acceder a esta acción
        public async Task<IEnumerable<UsuarioDTO>> Get()
        {
            var usuarios = await context.Users.ToListAsync(); // obtener todos los usuarios de la base de datos
            var usuariosDTO = mapper.Map<IEnumerable<UsuarioDTO>>(usuarios); // mapear los usuarios a usuariosDTO

            return usuariosDTO; // retornar los usuariosDTO
        }

        [HttpPost("registro")]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Registrar(
            CredencialesUsuarioDTO credencialesUsuarioDTO)
        {
            var usuario = new Usuario // creación del usuario con Identity 
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

        [HttpPut]
        [Authorize] // requiere autenticacion para acceder a esta acción
        public async Task<ActionResult> Put(ActualizarUsuarioDTO actualizarUsuarioDTO)
        {
            var usuario = await serviciosUsuarios.ObtenerUsuario(); 

            if (usuario is null)
            {
                return NotFound(); // si no existe el usuario, retornar not found
            }

            usuario.FechaNacimiento = actualizarUsuarioDTO.FechaNacimiento; 

            await userManager.UpdateAsync(usuario); // actualizar el usuario
            return NoContent(); // retornar no content 
        }

        [HttpGet("renovar-token")]
        [Authorize] // requiere autenticacion para acceder a esta acción
        public async Task<ActionResult<RespuestaAutenticacionDTO>> RenovarToken()
        {
            var usuario = await serviciosUsuarios.ObtenerUsuario(); // obtener el usuario

            if (usuario is null)
            {
                return NotFound(); // si no existe el usuario, retornar not found
            }

            var credencialesUsuarioDTO = new CredencialesUsuarioDTO
            {
                Email = usuario.Email!
            };

            var respuestaAutenticacion = await ConstruirToken(credencialesUsuarioDTO); // crear el token

            return respuestaAutenticacion; // retornar el token
        }

        [HttpPost("hacer-admin")]
        [Authorize(Policy = "isadmin")]
        public async Task<ActionResult> HacerAdmin(EditarClaimDTO editarClaimDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarClaimDTO.Email); // buscar el usuario por email
            if (usuario is null)
            {
                return NotFound(); // si no existe el usuario, retornar not found
            }
            await userManager.AddClaimAsync(usuario, new Claim("esadmin", "true")); // añadir el claim al usuario

            return NoContent(); // retornar no content 
        }

        [HttpPost("remover-admin")]
        //[Authorize(Policy = "isadmin")] // se puede quitar la autorizacion para que cualquier persona pueda quitar el admin por el momento 
        public async Task<ActionResult> RemoverAdmin(EditarClaimDTO editarClaimDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarClaimDTO.Email); // buscar el usuario por email
            if (usuario is null)
            {
                return NotFound(); // si no existe el usuario, retornar not found
            }
            await userManager.RemoveClaimAsync(usuario, new Claim("esadmin", "true")); // añadir el claim al usuario

            return NoContent(); // retornar no content 
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
