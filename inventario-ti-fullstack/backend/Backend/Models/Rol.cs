namespace Backend.Models
{
    public class Rol
    {
        public int Id { get; set; }
        public string NombreRol { get; set; } = null!;

        public ICollection<PerfilRequerimiento> PerfilesRequerimientos { get; set; } = new List<PerfilRequerimiento>();
        public ICollection<DetalleSolicitud> DetallesSolicitud { get; set; } = new List<DetalleSolicitud>();
    }
}
