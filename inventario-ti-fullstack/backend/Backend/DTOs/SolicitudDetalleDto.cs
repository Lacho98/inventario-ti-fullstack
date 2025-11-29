namespace Backend.DTOs.Solicitudes
{
    public class SolicitudDetalleDto
    {
        public int Id { get; set; }
        public string NombreSolicitud { get; set; } = null!;
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = null!;

        public List<RolPuestoDetalleDto> Roles { get; set; } = new();
    }

    public class RolPuestoDetalleDto
    {
        public int RolId { get; set; }
        public string NombreRol { get; set; } = null!;
        public int CantidadPuestos { get; set; }
    }
}
