namespace Backend.Models
{
    public class Equipo
    {
        public int Id { get; set; }
        public string TipoEquipo { get; set; } = null!;
        public string Modelo { get; set; } = null!;
        public string NumeroSerie { get; set; } = null!;
        public string Estado { get; set; } = "disponible";
        public decimal Costo { get; set; }
        public string? Especificaciones { get; set; }

        public ICollection<HistorialAsignacion> HistorialAsignaciones { get; set; } = new List<HistorialAsignacion>();
    }
}
