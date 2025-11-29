namespace Backend.Models
{
    public class Empleado
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = null!;
        public string? RolActual { get; set; }

        public ICollection<HistorialAsignacion> HistorialAsignaciones { get; set; } = new List<HistorialAsignacion>();
    }
}
