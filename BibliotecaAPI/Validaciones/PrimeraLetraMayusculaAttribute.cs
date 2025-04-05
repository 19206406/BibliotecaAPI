using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.Validaciones
{
    public class PrimeraLetraMayusculaAttribute: ValidationAttribute // eredar de esta clase para utilizarla como atributo de validación 
    {
        // crear el lugar para hacer la validación 
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success; 
            }

            var valueString = value.ToString()!; // convierte la cadena en un string 

            var primeraLetra = valueString[0].ToString(); // la primera letra de la cadena 

            if (primeraLetra != primeraLetra.ToUpper())
            {
                return new ValidationResult("La primera letra debe ser mayuscula"); 
            }

            return ValidationResult.Success; 
        }
    }
}
