using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            if (await context.Roles.AnyAsync())
                return;

            // ROLES
            var roles = new List<Rol>
            {
                new Rol { NombreRol = "Diseñador" },
                new Rol { NombreRol = "Desarrollador" },
                new Rol { NombreRol = "Soporte" },
                new Rol { NombreRol = "Analista" },
                new Rol { NombreRol = "Project Manager" }
            };
            context.Roles.AddRange(roles);
            await context.SaveChangesAsync();

            // EMPLEADOS 
            var empleados = new List<Empleado>
            {
                new Empleado { NombreCompleto = "Ana López", RolId = roles.First(r => r.NombreRol == "Diseñador").Id, EstaActivo = true, EstaDisponible = true },
                new Empleado { NombreCompleto = "Carlos Pérez", RolId = roles.First(r => r.NombreRol == "Diseñador").Id, EstaActivo = true, EstaDisponible = true },
                new Empleado { NombreCompleto = "Juan Covarruvias", RolId = roles.First(r => r.NombreRol == "Diseñador").Id, EstaActivo = true, EstaDisponible = true },
                new Empleado { NombreCompleto = "Pedro Cruz", RolId = roles.First(r => r.NombreRol == "Diseñador").Id, EstaActivo = true, EstaDisponible = true },
                new Empleado { NombreCompleto = "Maria Garcia", RolId = roles.First(r => r.NombreRol == "Diseñador").Id, EstaActivo = true, EstaDisponible = true },

                new Empleado { NombreCompleto = "Luis García", RolId = roles.First(r => r.NombreRol == "Desarrollador").Id, EstaActivo = true, EstaDisponible = true },
                new Empleado { NombreCompleto = "María Torres", RolId = roles.First(r => r.NombreRol == "Desarrollador").Id, EstaActivo = true, EstaDisponible = true },
                new Empleado { NombreCompleto = "Cesar Ochoa", RolId = roles.First(r => r.NombreRol == "Desarrollador").Id, EstaActivo = true, EstaDisponible = true },
                new Empleado { NombreCompleto = "Uriel Martinez", RolId = roles.First(r => r.NombreRol == "Desarrollador").Id, EstaActivo = true, EstaDisponible = true },
                new Empleado { NombreCompleto = "Manuel Villeda", RolId = roles.First(r => r.NombreRol == "Desarrollador").Id, EstaActivo = true, EstaDisponible = true },

                new Empleado { NombreCompleto = "Jorge Ruiz", RolId = roles.First(r => r.NombreRol == "Soporte").Id, EstaActivo = true, EstaDisponible = true },
                new Empleado { NombreCompleto = "Elena Díaz", RolId = roles.First(r => r.NombreRol == "Soporte").Id, EstaActivo = true, EstaDisponible = true },
                new Empleado { NombreCompleto = "Guadalupe Torres", RolId = roles.First(r => r.NombreRol == "Soporte").Id, EstaActivo = true, EstaDisponible = true },
                new Empleado { NombreCompleto = "Luis Mendez", RolId = roles.First(r => r.NombreRol == "Soporte").Id, EstaActivo = true, EstaDisponible = true },
                new Empleado { NombreCompleto = "Juan Manuel", RolId = roles.First(r => r.NombreRol == "Soporte").Id, EstaActivo = true, EstaDisponible = true },

                new Empleado { NombreCompleto = "Carlos Hernández", RolId = roles.First(r => r.NombreRol == "Analista").Id, EstaActivo = true, EstaDisponible = true },
                new Empleado { NombreCompleto = "María González", RolId = roles.First(r => r.NombreRol == "Analista").Id, EstaActivo = true, EstaDisponible = true },
                new Empleado { NombreCompleto = "Andrea Morales", RolId = roles.First(r => r.NombreRol == "Analista").Id, EstaActivo = true, EstaDisponible = true },
                new Empleado { NombreCompleto = "Miguel Torres", RolId = roles.First(r => r.NombreRol == "Analista").Id, EstaActivo = true, EstaDisponible = true },
                new Empleado { NombreCompleto = "Juan Castillo", RolId = roles.First(r => r.NombreRol == "Analista").Id, EstaActivo = true, EstaDisponible = true },

                new Empleado { NombreCompleto = "Jorge Torres", RolId = roles.First(r => r.NombreRol == "Project Manager").Id, EstaActivo = true, EstaDisponible = true },
                new Empleado { NombreCompleto = "Paola Castillo", RolId = roles.First(r => r.NombreRol == "Project Manager").Id, EstaActivo = true, EstaDisponible = true },
                new Empleado { NombreCompleto = "Guadalupe Mendez", RolId = roles.First(r => r.NombreRol == "Project Manager").Id, EstaActivo = true, EstaDisponible = true },
                new Empleado { NombreCompleto = "Luis Ochoa", RolId = roles.First(r => r.NombreRol == "Project Manager").Id, EstaActivo = true, EstaDisponible = true },
                new Empleado { NombreCompleto = "Juan Gonzalez", RolId = roles.First(r => r.NombreRol == "Project Manager").Id, EstaActivo = true, EstaDisponible = true }
            };
            context.Empleados.AddRange(empleados);
            await context.SaveChangesAsync();

            // ===== NECESIDADES POR ROL =====
            var necesidades = new List<NecesidadPorRol>
            {
                // Diseñador 1 Laptop, 1 Monitor
                new NecesidadPorRol { RolId = roles.First(r => r.NombreRol == "Diseñador").Id, TipoEquipo = "Laptop", CantidadPorPuesto = 1 },
                new NecesidadPorRol { RolId = roles.First(r => r.NombreRol == "Diseñador").Id, TipoEquipo = "Monitor", CantidadPorPuesto = 1 },

                // Desarrollador 1 Laptop, 2 Monitores
                new NecesidadPorRol { RolId = roles.First(r => r.NombreRol == "Desarrollador").Id, TipoEquipo = "Laptop", CantidadPorPuesto = 1 },
                new NecesidadPorRol { RolId = roles.First(r => r.NombreRol == "Desarrollador").Id, TipoEquipo = "Monitor", CantidadPorPuesto = 2 },

                // Soporte 1 Desktop, 1 Monitor
                new NecesidadPorRol { RolId = roles.First(r => r.NombreRol == "Soporte").Id, TipoEquipo = "Desktop", CantidadPorPuesto = 1 },
                new NecesidadPorRol { RolId = roles.First(r => r.NombreRol == "Soporte").Id, TipoEquipo = "Monitor", CantidadPorPuesto = 1 },

                // Analista 1 Laptop, 2 Monitores
                new NecesidadPorRol { RolId = roles.First(r => r.NombreRol == "Analista").Id, TipoEquipo = "Laptop", CantidadPorPuesto = 1 },
                new NecesidadPorRol { RolId = roles.First(r => r.NombreRol == "Analista").Id, TipoEquipo = "Monitor", CantidadPorPuesto = 2 },

                // Project Manager 1 Laptop
                new NecesidadPorRol { RolId = roles.First(r => r.NombreRol == "Project Manager").Id, TipoEquipo = "Laptop", CantidadPorPuesto = 1 }
            };
            context.NecesidadesPorRol.AddRange(necesidades);
            await context.SaveChangesAsync();

            // EQUIPOS INICIALES 
            var equipos = new List<Equipo>
            {
                new Equipo { TipoEquipo = "Laptop", Modelo = "Dell Latitude 5420", NumeroSerie = "LAP-001", Costo = 20000, Especificaciones = "16GB RAM, 512GB SSD", Estado = "disponible" },
                new Equipo { TipoEquipo = "Laptop", Modelo = "HP EliteBook 840", NumeroSerie = "LAP-002", Costo = 19500, Especificaciones = "16GB RAM, 512GB SSD", Estado = "disponible" },
                new Equipo { TipoEquipo = "Laptop", Modelo = "Lenovo ThinkPad T14", NumeroSerie = "LAP-003", Costo = 21000, Especificaciones = "16GB RAM, 1TB SSD", Estado = "disponible" },
                new Equipo { TipoEquipo = "Laptop", Modelo = "HP EliteBook 652", NumeroSerie = "LAP-004", Costo = 15000, Especificaciones = "32GB RAM, 512GB SSD", Estado = "disponible" },
                new Equipo { TipoEquipo = "Laptop", Modelo = "Lenovo ThinkPad T02", NumeroSerie = "LAP-005", Costo = 25000, Especificaciones = "16GB RAM, 1TB SSD", Estado = "disponible" },

                new Equipo { TipoEquipo = "Desktop", Modelo = "Dell OptiPlex 7090", NumeroSerie = "DESK-001", Costo = 15000, Especificaciones = "16GB RAM, 512GB SSD", Estado = "disponible" },
                new Equipo { TipoEquipo = "Desktop", Modelo = "HP ProDesk 600", NumeroSerie = "DESK-002", Costo = 14500, Especificaciones = "16GB RAM, 512GB SSD", Estado = "disponible" },
                new Equipo { TipoEquipo = "Desktop", Modelo = "HP ProDesk 600", NumeroSerie = "DESK-003", Costo = 18000, Especificaciones = "32GB RAM, 1TB SSD", Estado = "disponible" },

                new Equipo { TipoEquipo = "Monitor", Modelo = "Dell 24\"", NumeroSerie = "MON-001", Costo = 3500, Especificaciones = "24 pulgadas", Estado = "disponible" },
                new Equipo { TipoEquipo = "Monitor", Modelo = "Dell 24\"", NumeroSerie = "MON-002", Costo = 3500, Especificaciones = "24 pulgadas", Estado = "disponible" },
                new Equipo { TipoEquipo = "Monitor", Modelo = "LG 27\"", NumeroSerie = "MON-003", Costo = 4500, Especificaciones = "27 pulgadas", Estado = "disponible" },
                new Equipo { TipoEquipo = "Monitor", Modelo = "LG 27\"", NumeroSerie = "MON-004", Costo = 4500, Especificaciones = "27 pulgadas", Estado = "disponible" }
            };
            context.Equipos.AddRange(equipos);
            await context.SaveChangesAsync();
        }
    }
}
