using BibliotecaAPI;
using BibliotecaAPI.Datos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// configuración del auto maper 
builder.Services.AddAutoMapper(typeof(Program)); // indicamos los mapeos se van hacer dentro de este proyecto 

builder.Services.AddControllers().AddNewtonsoftJson(); // para hacer uso del patch 

builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
    opciones.UseSqlServer("name=DefaultConnection"));

builder.Services.AddIdentityCore<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders(); // para manejar la autenticación y autorización de usuarios

builder.Services.AddScoped<UserManager<IdentityUser>>(); // para manejar la autenticación y autorización de usuarios
builder.Services.AddScoped<SignInManager<IdentityUser>>(); // para manejar la autenticación y autorización de usuarios
builder.Services.AddHttpContextAccessor(); // para tener el contexto en todas partes

builder.Services.AddAuthentication().AddJwtBearer(opciones =>
{
    opciones.MapInboundClaims = false; // para que no se repitan los claims

    opciones.TokenValidationParameters = new TokenValidationParameters // configuraciones del token y la firma
    {
        ValidateIssuer = false, // para validar el emisor
        ValidateAudience = false, // para validar el receptor
        ValidateLifetime = true, // para validar la fecha de expiración
        ValidateIssuerSigningKey = true, // para validar la clave de firma
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["llavejwt"])), // clave de firma
        ClockSkew = TimeSpan.Zero // para que no haya diferencia de tiempo entre el servidor y el cliente
    };

}); 

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
