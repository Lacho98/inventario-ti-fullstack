namespace Backend.Models
{
    public class Empleado
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = null!;

        public int RolId { get; set; }
        public Rol Rol { get; set; } = null!;

        public bool EstaActivo { get; set; } = true;
        public bool EstaDisponible { get; set; } = true;

        public ICollection<HistorialAsignacion> HistorialAsignaciones { get; set; } = new List<HistorialAsignacion>();
    }

}
