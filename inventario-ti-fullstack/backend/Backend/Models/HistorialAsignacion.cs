namespace Backend.Models
{
    public class HistorialAsignacion
    {
        public int Id { get; set; }

        public int EquipoId { get; set; }
        public Equipo Equipo { get; set; } = null!;

        public int EmpleadoId { get; set; }
        public Empleado Empleado { get; set; } = null!;

        public int SolicitudId { get; set; }
        public SolicitudEquipamiento Solicitud { get; set; } = null!;

        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
        public string? Comentario { get; set; }
    }

}
