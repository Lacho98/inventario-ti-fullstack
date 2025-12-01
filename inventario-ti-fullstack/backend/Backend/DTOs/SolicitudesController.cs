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
        [HttpPost]
        public async Task<ActionResult<SolicitudDetalleDto>> CrearSolicitud([FromBody] SolicitudCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(dto.NombreSolicitud))
                return BadRequest(new { mensaje = "El nombre de la solicitud es obligatorio." });

            if (dto.RolesSolicitados == null || dto.RolesSolicitados.Count == 0)
                return BadRequest(new { mensaje = "Debes especificar al menos un rol solicitado." });

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
            var solicitud = await _context.SolicitudesEquipamiento
                .Include(s => s.Detalles)
                    .ThenInclude(d => d.Rol)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
                return NotFound(new { mensaje = "Solicitud no encontrada" });

            if (solicitud.Detalles == null || !solicitud.Detalles.Any())
                return BadRequest(new { mensaje = "La solicitud no tiene detalles de roles." });

            var rolesIds = solicitud.Detalles
                .Select(d => d.RolId)
                .Distinct()
                .ToList();

            var necesidades = await _context.NecesidadesPorRol
                .Include(n => n.Rol)
                .Where(n => rolesIds.Contains(n.RolId))
                .ToListAsync();

            if (!necesidades.Any())
                return BadRequest(new { mensaje = "No hay necesidades configuradas para los roles de esta solicitud." });

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

            foreach (var detalle in solicitud.Detalles)
            {
                var rol = detalle.Rol;
                var necesidadesRol = necesidades
                    .Where(n => n.RolId == detalle.RolId)
                    .ToList();

                if (!necesidadesRol.Any())
                {
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

                for (int puesto = 1; puesto <= detalle.CantidadPuestos; puesto++)
                {
                    var asignacionRol = new AsignacionRolDto
                    {
                        RolId = rol.Id,
                        Rol = rol.NombreRol,
                        PuestoNumero = puesto
                    };

                    foreach (var necesidad in necesidadesRol)
                    {
                        var tipo = necesidad.TipoEquipo;
                        int cantidadNecesaria = necesidad.CantidadPorPuesto;

                        for (int i = 0; i < cantidadNecesaria; i++)
                        {
                            if (equiposPorTipo.ContainsKey(tipo) && equiposPorTipo[tipo].Any())
                            {
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


        // PUT: api/solicitudes/{id}/estado
        [HttpPut("{id:int}/estado")]
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarEstadoSolicitudDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Estado))
                return BadRequest(new { mensaje = "El estado es obligatorio." });

            var nuevoEstado = dto.Estado.Trim().ToLowerInvariant();

            var solicitud = await _context.SolicitudesEquipamiento
                .Include(s => s.Detalles)
                    .ThenInclude(d => d.Rol)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
                return NotFound(new { mensaje = "Solicitud no encontrada" });

            if (nuevoEstado == "resuelta")
            {
                var rolesIds = solicitud.Detalles
                    .Select(d => d.RolId)
                    .Distinct()
                    .ToList();

                var necesidades = await _context.NecesidadesPorRol
                    .Where(n => rolesIds.Contains(n.RolId))
                    .ToListAsync();

                var equiposDisponibles = await _context.Equipos
                    .Where(e => e.Estado == "disponible")
                    .OrderBy(e => e.Costo)
                    .ToListAsync();

                var equiposPorTipo = equiposDisponibles
                    .GroupBy(e => e.TipoEquipo)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var empleadosDisponibles = await _context.Empleados
                    .Where(emp => emp.EstaActivo && emp.EstaDisponible && rolesIds.Contains(emp.RolId))
                    .OrderBy(emp => emp.Id)
                    .ToListAsync();

                var empleadosPorRol = empleadosDisponibles
                    .GroupBy(emp => emp.RolId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var detalle in solicitud.Detalles)
                {
                    var rol = detalle.Rol;
                    var necesidadesRol = necesidades
                        .Where(n => n.RolId == detalle.RolId)
                        .ToList();

                    if (!necesidadesRol.Any())
                        continue;

                    if (!empleadosPorRol.TryGetValue(detalle.RolId, out var listaEmpleadosRol) ||
                        listaEmpleadosRol.Count == 0)
                    {
                        continue;
                    }

                    for (int puesto = 1; puesto <= detalle.CantidadPuestos; puesto++)
                    {
                        if (listaEmpleadosRol.Count == 0)
                            break;

                        var empleado = listaEmpleadosRol.First();
                        listaEmpleadosRol.RemoveAt(0);
                        empleado.EstaDisponible = false;

                        foreach (var necesidad in necesidadesRol)
                        {
                            var tipo = necesidad.TipoEquipo;
                            int cantidadNecesaria = necesidad.CantidadPorPuesto;

                            for (int i = 0; i < cantidadNecesaria; i++)
                            {
                                if (equiposPorTipo.ContainsKey(tipo) && equiposPorTipo[tipo].Any())
                                {
                                    var equipo = equiposPorTipo[tipo].First();
                                    equiposPorTipo[tipo].RemoveAt(0);

                                    equipo.Estado = "asignado";
                                    equipo.EmpleadoAsignadoId = empleado.Id;

                                    _context.HistorialAsignaciones.Add(new HistorialAsignacion
                                    {
                                        EquipoId = equipo.Id,
                                        EmpleadoId = empleado.Id,
                                        SolicitudId = solicitud.Id,
                                        Comentario = $"Asignado a {empleado.NombreCompleto} ({rol.NombreRol})."
                                    });
                                }
                            }
                        }
                    }
                }
            }


            solicitud.Estado = nuevoEstado;
            await _context.SaveChangesAsync();

            return NoContent();
        }



    }
}


