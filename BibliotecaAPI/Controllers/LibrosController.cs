﻿using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Runtime.InteropServices;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/libros")]
    [Authorize] // Requiere autenticacion para acceder a las acciones
    public class LibrosController: ControllerBase   
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public LibrosController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper; 
        }


        [HttpGet]
        public async Task<IEnumerable<LibroDTO>> Get()
        {
            var libros =  context.Libros.ToListAsync(); 
            var librosDto = mapper.Map<IEnumerable<LibroDTO>>(libros); // se pasan las cosas por aqui 
            return librosDto; 
        }

        [HttpGet("{id:int}", Name = "ObtenerLibro")]
        public async Task<ActionResult<LibroConAutoresDTO>> Get(int id)
        {
            var libro = await context.Libros
                .Include(x => x.Autores)
                    .ThenInclude(x => x.Autor)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (libro is null)
            {
                return NotFound();
            }

            var libroDTO = mapper.Map<LibroConAutoresDTO>(libro);

            return libroDTO;
        }

        [HttpPost]
        public async Task<ActionResult> Post(LibroCreacionDTO libroCreacionDTO) // uso del nuevo DTO de DTOs
        {

            if (libroCreacionDTO.AutoresId is null || libroCreacionDTO.AutoresId.Count == 0)
            {
                ModelState.AddModelError(nameof(libroCreacionDTO.AutoresId),
                    "No se puede crear un libro sin autores");

                return ValidationProblem();
            }

            var autoresIdsExisten = await context.Autores
                .Where(x => libroCreacionDTO.AutoresId.Contains(x.Id))
                .Select(x => x.Id).ToListAsync(); 

            if (autoresIdsExisten.Count != libroCreacionDTO.AutoresId.Count)
            {
                var autoresNoExisten = libroCreacionDTO.AutoresId.Except(autoresIdsExisten);
                var autoresNoExistenString = string.Join(",", autoresNoExisten); // 1,2,3
                var mensajeDeError = $"Los siguientes autores no existen: {autoresNoExisten}";
                ModelState.AddModelError(nameof(libroCreacionDTO.AutoresId), mensajeDeError);
                return ValidationProblem(); 
            }

            var libro = mapper.Map<Libro>(libroCreacionDTO); 
            AsignarOrdenAutores(libro);

            context.Add(libro);
            await context.SaveChangesAsync();

            var libroDTO = mapper.Map<LibroDTO>(libro);
            return CreatedAtRoute("ObtenerLibro", new { id = libro.Id }, libroDTO);
        }

        private void AsignarOrdenAutores(Libro libro)
        {
            if (libro.Autores is null)
            {
                for (int i = 0; i < libro.Autores.Count; i++)
                {
                    libro.Autores[1].Orden = i; 
                }
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, LibroCreacionDTO libroCreacionDTO)
        {
            if (libroCreacionDTO.AutoresId is null || libroCreacionDTO.AutoresId.Count == 0)
            {
                ModelState.AddModelError(nameof(libroCreacionDTO.AutoresId),
                    "No se puede crear un libro sin autores");

                return ValidationProblem();
            }

            var autoresIdsExisten = await context.Autores
                .Where(x => libroCreacionDTO.AutoresId.Contains(x.Id))
                .Select(x => x.Id).ToListAsync();

            if (autoresIdsExisten.Count != libroCreacionDTO.AutoresId.Count)
            {
                var autoresNoExisten = libroCreacionDTO.AutoresId.Except(autoresIdsExisten);
                var autoresNoExistenString = string.Join(",", autoresNoExisten); // 1,2,3
                var mensajeDeError = $"Los siguientes autores no existen: {autoresNoExisten}";
                ModelState.AddModelError(nameof(libroCreacionDTO.AutoresId), mensajeDeError);
                return ValidationProblem();
            }

            var libroDB = await context.Libros
                .Include(x => x.Autores).FirstOrDefaultAsync(x => x.Id == id); 

            if (libroDB is null)
            {
                return NotFound(); 
            }

            libroDB = mapper.Map(libroCreacionDTO, libroDB);
            AsignarOrdenAutores(libroDB); 

            await context.SaveChangesAsync();
            return NoContent(); // estandar 
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var registrosBorrados = await context.Libros.Where(x => x.Id == id).ExecuteDeleteAsync();

            if (registrosBorrados == 0)
            {
                return NotFound();
            }

            return NoContent(); // estandar 
        }
    }
}
