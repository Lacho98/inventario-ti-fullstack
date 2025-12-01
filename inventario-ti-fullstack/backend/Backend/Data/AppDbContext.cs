using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Equipo> Equipos { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<PerfilRequerimiento> PerfilesRequerimientos { get; set; }
        public DbSet<SolicitudEquipamiento> SolicitudesEquipamiento { get; set; }
        public DbSet<DetalleSolicitud> DetallesSolicitud { get; set; }
        public DbSet<HistorialAsignacion> HistorialAsignaciones { get; set; }
        public DbSet<NecesidadPorRol> NecesidadesPorRol { get; set; }

    }
}
