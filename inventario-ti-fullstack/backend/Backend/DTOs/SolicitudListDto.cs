namespace Backend.DTOs.Solicitudes
{
    public class SolicitudListDto
    {
        public int Id { get; set; }
        public string NombreSolicitud { get; set; } = null!;
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = null!;
    }
}
