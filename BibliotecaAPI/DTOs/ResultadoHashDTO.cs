namespace BibliotecaAPI.DTOs
{
    public class ResultadoHashDTO
    {
        public required string Hash { get; set; }
        public required byte[] Sal { get; set; } // las sales se almacenan como byte[]
    }
}
