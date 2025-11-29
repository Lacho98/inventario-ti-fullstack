using Backend.Data;
using Backend.DTOs.Solicitudes;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.DTOs.Propuestas;


namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SolicitudesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SolicitudesController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/solicitudes
        // Crea una nueva solicitud de equipamiento
        [HttpPost]
        public async Task<ActionResult<SolicitudDetalleDto>> CrearSolicitud([FromBody] SolicitudCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(dto.NombreSolicitud))
                return BadRequest(new { mensaje = "El nombre de la solicitud es obligatorio." });

            if (dto.RolesSolicitados == null || dto.RolesSolicitados.Count == 0)
                return BadRequest(new { mensaje = "Debes especificar al menos un rol solicitado." });

            // Validar que los roles existan
            var rolesIds = dto.RolesSolicitados.Select(r => r.RolId).Distinct().ToList();
            var rolesExistentes = await _context.Roles
                .Where(r => rolesIds.Contains(r.Id))
                .ToListAsync();

            if (rolesExistentes.Count != rolesIds.Count)
            {
                var idsNoEncontrados = rolesIds.Except(rolesExistentes.Select(r => r.Id)).ToList();
                return BadRequest(new
                {
                    mensaje = "Algunos roles no existen.",
                    rolesNoEncontrados = idsNoEncontrados
                });
            }

            var solicitud = new SolicitudEquipamiento
            {
                NombreSolicitud = dto.NombreSolicitud,
                Fecha = DateTime.UtcNow,
                Estado = "pendiente",
                // CreadoPorId = null // si luego agregas autenticación, se puede llenar
            };

            foreach (var rolSolicitado in dto.RolesSolicitados)
            {
                solicitud.Detalles.Add(new DetalleSolicitud
                {
                    RolId = rolSolicitado.RolId,
                    CantidadPuestos = rolSolicitado.Cantidad
                });
            }

            _context.SolicitudesEquipamiento.Add(solicitud);
            await _context.SaveChangesAsync();

            // Mapear a DTO de detalle para la respuesta
            var resultado = new SolicitudDetalleDto
            {
                Id = solicitud.Id,
                NombreSolicitud = solicitud.NombreSolicitud,
                Fecha = solicitud.Fecha,
                Estado = solicitud.Estado,
                Roles = solicitud.Detalles.Select(d => new RolPuestoDetalleDto
                {
                    RolId = d.RolId,
                    NombreRol = rolesExistentes.First(r => r.Id == d.RolId).NombreRol,
                    CantidadPuestos = d.CantidadPuestos
                }).ToList()
            };

            return CreatedAtAction(nameof(GetSolicitudPorId), new { id = solicitud.Id }, resultado);
        }

        // GET: api/solicitudes
        // Lista todas las solicitudes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SolicitudListDto>>> GetSolicitudes()
        {
            var solicitudes = await _context.SolicitudesEquipamiento
                .OrderByDescending(s => s.Fecha)
                .Select(s => new SolicitudListDto
                {
                    Id = s.Id,
                    NombreSolicitud = s.NombreSolicitud,
                    Fecha = s.Fecha,
                    Estado = s.Estado
                })
                .ToListAsync();

            return Ok(solicitudes);
        }

        // GET: api/solicitudes/{id}
        // Detalle de una solicitud (con roles y cantidades)
        [HttpGet("{id:int}")]
        public async Task<ActionResult<SolicitudDetalleDto>> GetSolicitudPorId(int id)
        {
            var solicitud = await _context.SolicitudesEquipamiento
                .Include(s => s.Detalles)
                    .ThenInclude(d => d.Rol)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
                return NotFound(new { mensaje = "Solicitud no encontrada" });

            var dto = new SolicitudDetalleDto
            {
                Id = solicitud.Id,
                NombreSolicitud = solicitud.NombreSolicitud,
                Fecha = solicitud.Fecha,
                Estado = solicitud.Estado,
                Roles = solicitud.Detalles.Select(d => new RolPuestoDetalleDto
                {
                    RolId = d.RolId,
                    NombreRol = d.Rol.NombreRol,
                    CantidadPuestos = d.CantidadPuestos
                }).ToList()
            };

            return Ok(dto);
        }

        // GET: api/solicitudes/{id}/propuesta-optima
        [HttpGet("{id:int}/propuesta-optima")]
        public async Task<ActionResult<PropuestaOptimaDto>> ObtenerPropuestaOptima(int id)
        {
            // 1. Cargar la solicitud con sus detalles y roles
            var solicitud = await _context.SolicitudesEquipamiento
                .Include(s => s.Detalles)
                    .ThenInclude(d => d.Rol)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
                return NotFound(new { mensaje = "Solicitud no encontrada" });

            if (solicitud.Detalles == null || !solicitud.Detalles.Any())
                return BadRequest(new { mensaje = "La solicitud no tiene detalles de roles." });

            var rolesIds = solicitud.Detalles.Select(d => d.RolId).Distinct().ToList();

            // 2. Cargar perfiles de requerimientos para esos roles
            var perfiles = await _context.PerfilesRequerimientos
                .Include(p => p.Rol)
                .Where(p => rolesIds.Contains(p.RolId))
                .ToListAsync();

            if (!perfiles.Any())
                return BadRequest(new { mensaje = "No hay perfiles de requerimientos configurados para los roles de esta solicitud." });

            // 3. Cargar equipos disponibles y agruparlos por tipo, ordenando por costo (estrategia de menor costo)
            var equiposDisponibles = await _context.Equipos
                .Where(e => e.Estado == "disponible")
                .OrderBy(e => e.Costo)
                .ToListAsync();

            var equiposPorTipo = equiposDisponibles
                .GroupBy(e => e.TipoEquipo)
                .ToDictionary(g => g.Key, g => g.ToList());

            var propuesta = new PropuestaOptimaDto
            {
                SolicitudId = solicitud.Id
            };

            var faltantesDict = new Dictionary<(int rolId, string tipoEquipo), FaltanteDto>();
            decimal costoTotal = 0m;

            // 4. Recorrer cada detalle de la solicitud (rol + cantidad de puestos)
            foreach (var detalle in solicitud.Detalles)
            {
                var rol = detalle.Rol;
                var perfilesRol = perfiles.Where(p => p.RolId == detalle.RolId).ToList();

                if (!perfilesRol.Any())
                {
                    // Si no hay perfil para este rol, lo marcamos como faltante genérico
                    var key = (detalle.RolId, "SIN_PERFIL");
                    if (!faltantesDict.ContainsKey(key))
                    {
                        faltantesDict[key] = new FaltanteDto
                        {
                            RolId = detalle.RolId,
                            Rol = rol.NombreRol,
                            TipoEquipo = "SIN_PERFIL",
                            CantidadFaltante = 0
                        };
                    }

                    faltantesDict[key].CantidadFaltante += detalle.CantidadPuestos;
                    continue;
                }

                // Para cada puesto (ej. 3 diseñadores => puesto 1, 2, 3)
                for (int puesto = 1; puesto <= detalle.CantidadPuestos; puesto++)
                {
                    var asignacionRol = new AsignacionRolDto
                    {
                        RolId = rol.Id,
                        Rol = rol.NombreRol,
                        PuestoNumero = puesto
                    };

                    // Para cada tipo de equipo requerido por el rol
                    foreach (var perfilReq in perfilesRol)
                    {
                        var tipo = perfilReq.TipoEquipo;
                        int cantidadNecesaria = perfilReq.CantidadRequerida;

                        for (int i = 0; i < cantidadNecesaria; i++)
                        {
                            if (equiposPorTipo.ContainsKey(tipo) && equiposPorTipo[tipo].Any())
                            {
                                // Tomar el equipo más barato disponible de ese tipo
                                var equipo = equiposPorTipo[tipo].First();
                                equiposPorTipo[tipo].RemoveAt(0);

                                asignacionRol.Equipos.Add(new EquipoAsignadoDto
                                {
                                    EquipoId = equipo.Id,
                                    TipoEquipo = equipo.TipoEquipo,
                                    Modelo = equipo.Modelo,
                                    Costo = equipo.Costo
                                });

                                costoTotal += equipo.Costo;
                            }
                            else
                            {
                                // No hay suficientes equipos de este tipo
                                var key = (rol.Id, tipo);
                                if (!faltantesDict.ContainsKey(key))
                                {
                                    faltantesDict[key] = new FaltanteDto
                                    {
                                        RolId = rol.Id,
                                        Rol = rol.NombreRol,
                                        TipoEquipo = tipo,
                                        CantidadFaltante = 0
                                    };
                                }

                                faltantesDict[key].CantidadFaltante++;
                            }
                        }
                    }

                    propuesta.Asignaciones.Add(asignacionRol);
                }
            }

            propuesta.CostoTotalEstimado = costoTotal;
            propuesta.Faltantes = faltantesDict.Values.ToList();

            if (!propuesta.Faltantes.Any())
            {
                propuesta.Mensaje = "Propuesta generada correctamente. La solicitud se puede cubrir con el inventario disponible.";
            }
            else
            {
                propuesta.Mensaje = "Propuesta generada parcialmente. No hay inventario suficiente para cubrir todos los requerimientos.";
            }

            return Ok(propuesta);
        }

    }
}


