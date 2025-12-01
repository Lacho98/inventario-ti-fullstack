namespace Backend.Models
{
    public class NecesidadPorRol
    {
        public int Id { get; set; }

        public int RolId { get; set; }
        public Rol Rol { get; set; } = null!;

        public string TipoEquipo { get; set; } = null!;

        public int CantidadPorPuesto { get; set; }
    }
}
