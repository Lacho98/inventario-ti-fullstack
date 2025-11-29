namespace Backend.DTOs.Solicitudes
{
    public class SolicitudCreateDto
    {
        public string NombreSolicitud { get; set; } = null!;
        public List<RolSolicitadoDto> RolesSolicitados { get; set; } = new();
    }

    public class RolSolicitadoDto
    {
        public int RolId { get; set; }
        public int Cantidad { get; set; }
    }
}
