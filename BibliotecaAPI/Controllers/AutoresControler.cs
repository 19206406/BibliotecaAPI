
using AutoMapper;
using Azure;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace BibliotecaAPI.Controllers
{
    [ApiController] 
    [Route("api/autores")]
    [Authorize] // Requiere autenticacion para acceder a las acciones
    public class AutoresControler : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public AutoresControler(ApplicationDbContext context, IMapper mapper) 
        {
            this.context = context;
            this.mapper = mapper; 
        }

        [HttpGet]
        [AllowAnonymous] // cualquier persona puede ejecutar esta acción 
        public async Task<IEnumerable<AutorDTO>> Get() // se cambia el tipo de dato por dto 
        {
            var autores = await context.Autores.ToListAsync();
            var autoresDTO = mapper.Map<IEnumerable<AutorDTO>>(autores); // pasar los autores al dto 
            return autoresDTO; // se retornan los datos 
        }
     
        [HttpGet("{id:int}", Name = "ObtenerAutor")] // crear como una clase de nombre con referencia 
        public async Task<ActionResult<AutorConLibrosDTO>> Get([FromRoute] int id) 
        {
            var autor = await context.Autores
                .Include(x => x.Libros)
                    .ThenInclude(x => x.Libro)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (autor is null)
            {
                return NotFound(); 
            }

            var autorDTO = mapper.Map<AutorConLibrosDTO>(autor);
            return autorDTO; 

        }

        [HttpPost]
        public async Task<ActionResult> Post(AutorCreadorDTO autorCreadorDTO)
        {
            var autor = mapper.Map<Autor>(autorCreadorDTO); 
            context.Add(autor); 
            await context.SaveChangesAsync();
            var autorDTO = mapper.Map<AutorDTO>(autor); 
            return CreatedAtRoute("ObtenerAutor", new {id = autor.Id}, autorDTO); // con esto tenemos una mejor respuesta 

        }

        [HttpPut("{id:int}")] // api/autores/id 
        public async Task<ActionResult> Put(int id, AutorCreadorDTO autorCreadorDTO)
        {
            var autor = mapper.Map<Autor>(autorCreadorDTO); 
            autor.Id = id;
            context.Update(autor); // se esta marcando el elemento para ser actualizado 
            await context.SaveChangesAsync();
            return NoContent(); // estandar en el web api 
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<AutorPatchDTO> patchDoc)
        {
            if (patchDoc is null)
            {
                return BadRequest();
            }

            var autorDB = await context.Autores.FirstOrDefaultAsync(x => x.Id == id); 

            if (autorDB is null)
            {
                return NotFound(); 
            }

            var autorPatchDTO = mapper.Map<AutorPatchDTO>(autorDB);

            patchDoc.ApplyTo(autorPatchDTO, ModelState); 

            var esValiddo = TryValidateModel(autorPatchDTO);

            if (!esValiddo)
            {
                return ValidationProblem(); 
            }

            mapper.Map(autorPatchDTO, autorDB);

            await context.SaveChangesAsync(); 

            return NoContent(); 

        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            // esto solo guarda el total de registros borrados 
            var registrosBorrados = await context.Autores.Where(x => x.Id == id).ExecuteDeleteAsync(); 

            if (registrosBorrados == 0) // si es igual a 0 no hay un elemento con un id de este tipo 
            {
                return NotFound();
            }

            return NoContent();  // Estandar 
        }
    }
}
