namespace Backend.Models
{
    public class SolicitudEquipamiento
    {
        public int Id { get; set; }
        public string NombreSolicitud { get; set; } = null!;
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public string Estado { get; set; } = "pendiente";

        public int? CreadoPorId { get; set; }
        public Empleado? CreadoPor { get; set; }

        public ICollection<DetalleSolicitud> Detalles { get; set; } = new List<DetalleSolicitud>();
    }
}
