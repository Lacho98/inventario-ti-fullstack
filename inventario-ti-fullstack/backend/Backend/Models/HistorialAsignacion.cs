namespace Backend.Models
{
    public class HistorialAsignacion
    {
        public int Id { get; set; }

        public int EquipoId { get; set; }
        public Equipo Equipo { get; set; } = null!;

        public int? EmpleadoId { get; set; }
        public Empleado? Empleado { get; set; }

        public string Accion { get; set; } = null!;
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public string Responsable { get; set; } = null!;
    }
}
