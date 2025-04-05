using BibliotecaAPI;
using BibliotecaAPI.Datos;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// configuración del auto maper 
builder.Services.AddAutoMapper(typeof(Program)); // indicamos los mapeos se van hacer dentro de este proyecto 

var diccionarioConfiguraciones = new Dictionary<string, string>
{
    {"quien_soy", "un diccionario en memoria" }
};

builder.Configuration.AddInMemoryCollection(diccionarioConfiguraciones!);

builder.Services.AddOptions<PersonaOpciones>()
    .Bind(builder.Configuration.GetSection(PersonaOpciones.Seccion)) // traer la info de la sección desde la clase de la configuración
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<TarifaOpciones>()
    .Bind(builder.Configuration.GetSection(TarifaOpciones.Seccion))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// singlenton 
builder.Services.AddSingleton<PagosProcesamiento>(); 

builder.Services.AddControllers().AddNewtonsoftJson(); // para hacer uso del patch 

builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
    opciones.UseSqlServer("name=DefaultConnection"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// middleware creados desde dos clases auxiliares 

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
