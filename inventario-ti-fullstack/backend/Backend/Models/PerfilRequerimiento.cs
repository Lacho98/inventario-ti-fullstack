namespace Backend.Models
{
    public class PerfilRequerimiento
    {
        public int Id { get; set; }

        public int RolId { get; set; }
        public Rol Rol { get; set; } = null!;

        public string TipoEquipo { get; set; } = null!;
        public int CantidadRequerida { get; set; }
    }
}
