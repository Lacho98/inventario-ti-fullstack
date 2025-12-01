namespace Backend.DTOs.Propuestas
{
    public class PropuestaOptimaDto
    {
        public int SolicitudId { get; set; }
        public List<AsignacionRolDto> Asignaciones { get; set; } = new();
        public decimal CostoTotalEstimado { get; set; }
        public List<FaltanteDto> Faltantes { get; set; } = new();
        public string Mensaje { get; set; } = string.Empty;
    }

    public class AsignacionRolDto
    {
        public int RolId { get; set; }
        public string Rol { get; set; } = string.Empty;
        public int PuestoNumero { get; set; }   
        public List<EquipoAsignadoDto> Equipos { get; set; } = new();
    }

    public class EquipoAsignadoDto
    {
        public int EquipoId { get; set; }
        public string TipoEquipo { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public decimal Costo { get; set; }
    }

    public class FaltanteDto
    {
        public int RolId { get; set; }
        public string Rol { get; set; } = string.Empty;
        public string TipoEquipo { get; set; } = string.Empty;
        public int CantidadFaltante { get; set; }
    }
}
