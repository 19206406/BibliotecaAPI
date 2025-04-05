using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/configuraciones")]
    public class ConfiguracionesController: ControllerBase
    {
        private readonly IConfiguration configuration;
        private PagosProcesamiento pagosProcesamiento;
        private readonly IConfigurationSection seccion_01;
        private readonly IConfigurationSection seccion_02;
        private readonly PersonaOpciones _opcionesPersona;

        public ConfiguracionesController(IConfiguration configuration, 
            IOptionsSnapshot<PersonaOpciones> opcionesPersona, PagosProcesamiento pagosProcesamiento) // mejor opción para actualizar el elemento 
            // bueno para controladores entre otras cosas 
        {
            this.configuration = configuration;
            this.pagosProcesamiento = pagosProcesamiento;
            seccion_01 = configuration.GetSection("seccion_1"); // traer el contexto de una seccion de configuración 
            seccion_02 = configuration.GetSection("seccion_2"); // traer el contexto de una seccion de configuración 
            this._opcionesPersona = opcionesPersona.Value; // nuevo campo de la clase 
        }

        [HttpGet("options-monitor")]
        public ActionResult GetTarifas()
        {
            return Ok(pagosProcesamiento.ObtenerTarifas()); 
        }

        [HttpGet("seccion_1_opciones")]
        public ActionResult GetSeccion1Opciones() // consumir la clase 
        {
            return Ok(_opcionesPersona);
        }

        [HttpGet("proveedores")]
        public ActionResult GetProveedor()
        {
            var valor = configuration.GetValue<string>("quien_soy");
            return Ok(new { valor }); 
        }

        // traer todos 
        [HttpGet("obtenertodos")]
        public ActionResult GetObtenerTodos()
        {
            var hijos = configuration.GetChildren().Select(x => $"{x.Key}: {x.Value}");
            return Ok(new { hijos });
        }

        [HttpGet("seccion_01")]
        public ActionResult GetSeccion01()
        {
            var nombre = seccion_01.GetValue<String>("nombre");
            var edad = seccion_01.GetValue<String>("edad");

            return Ok(new { nombre, edad }); 
        }

        [HttpGet("seccion_02")]
        public ActionResult GetSeccion02()
        {
            var nombre = seccion_02.GetValue<String>("nombre");
            var edad = seccion_02.GetValue<String>("edad");

            return Ok(new { nombre, edad });
        }

        [HttpGet]
        public ActionResult<String> Get()
        {
            var opcion1 = configuration["apellido"]; // opcion una para obtener el usuario 

            var opcion2 = configuration.GetValue<String>("apellido")!; // segunda opción para tomar el valor 

            return opcion2; 
        }

        [HttpGet("secciones")]
        public ActionResult<String> GetSeccion()
        {
            var opcion1 = configuration["ConnectionStrings:DefaultConnection"]; // ingresar dentro de algo uno

            var opcion2 = configuration.GetValue<string>("ConnectionStrings:DefaultConnection"); // ingresar segunda opción 

            var seccion = configuration.GetSection("ConnectionStrings"); // configuración preliminar para tercera opción 
            var opcion3 = seccion["DefaultConnection"]; 

            return opcion3;
        }
    }
}
