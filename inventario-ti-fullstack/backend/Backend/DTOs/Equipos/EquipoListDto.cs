namespace Backend.DTOs.Equipos
{
    public class EquipoListDto
    {
        public int Id { get; set; }
        public string TipoEquipo { get; set; } = null!;
        public string Modelo { get; set; } = null!;
        public string NumeroSerie { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public decimal Costo { get; set; }
        public string? EmpleadoAsignado { get; set; }
        public string? RolEmpleado { get; set; }

    }

}
