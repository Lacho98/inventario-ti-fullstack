namespace Backend.DTOs
{
    public class EquipoCreateDto
    {
        public string TipoEquipo { get; set; } = null!;
        public string Modelo { get; set; } = null!;
        public string NumeroSerie { get; set; } = null!;
        public decimal Costo { get; set; }
        public string? Especificaciones { get; set; }
    }
}
