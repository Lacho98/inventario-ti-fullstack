namespace Backend.Models
{
    public class DetalleSolicitud
    {
        public int Id { get; set; }

        public int SolicitudId { get; set; }
        public SolicitudEquipamiento Solicitud { get; set; } = null!;

        public int RolId { get; set; }
        public Rol Rol { get; set; } = null!;

        public int CantidadPuestos { get; set; }
    }
}
