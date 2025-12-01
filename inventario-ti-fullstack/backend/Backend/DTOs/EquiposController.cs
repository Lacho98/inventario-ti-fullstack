using Backend.Data;
using Backend.DTOs;
using Backend.DTOs.Equipos;     
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquiposController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EquiposController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/equipos?estado=disponible&tipoEquipo=Laptop
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EquipoListDto>>> GetEquipos(
            [FromQuery] string? estado,
            [FromQuery] string? tipoEquipo)
        {
            var query = _context.Equipos
                .Include(e => e.EmpleadoAsignado)
                    .ThenInclude(emp => emp.Rol)   
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(e => e.Estado == estado);
            }

            if (!string.IsNullOrWhiteSpace(tipoEquipo))
            {
                query = query.Where(e => e.TipoEquipo == tipoEquipo);
            }

            var lista = await query
                .Select(e => new EquipoListDto
                {
                    Id = e.Id,
                    TipoEquipo = e.TipoEquipo,
                    Modelo = e.Modelo,
                    NumeroSerie = e.NumeroSerie,
                    Estado = e.Estado,
                    Costo = e.Costo,

                    EmpleadoAsignado = e.EmpleadoAsignado != null
                        ? e.EmpleadoAsignado.NombreCompleto
                        : null,

                    RolEmpleado = e.EmpleadoAsignado != null
                        ? e.EmpleadoAsignado.Rol.NombreRol
                        : null
                })

                .ToListAsync();

            return Ok(lista);
        }



        // GET: api/equipos/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Equipo>> GetEquipo(int id)
        {
            var equipo = await _context.Equipos.FindAsync(id);

            if (equipo == null)
                return NotFound(new { mensaje = "Equipo no encontrado" });

            return Ok(equipo);
        }

        // POST: api/equipos
        [HttpPost]
        public async Task<ActionResult<Equipo>> CrearEquipo([FromBody] EquipoCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Costo <= 0)
                return BadRequest(new { mensaje = "El costo debe ser mayor a cero." });

            var equipo = new Equipo
            {
                TipoEquipo = dto.TipoEquipo,
                Modelo = dto.Modelo,
                NumeroSerie = dto.NumeroSerie,
                Costo = dto.Costo,
                Especificaciones = dto.Especificaciones,
                Estado = "disponible" 
            };

            _context.Equipos.Add(equipo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEquipo),
                new { id = equipo.Id },
                equipo
            );
        }

        [HttpGet("tipos")]
        public async Task<ActionResult<IEnumerable<string>>> GetTiposEquipos()
        {
            var tipos = await _context.NecesidadesPorRol
                .Select(n => n.TipoEquipo)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            return Ok(tipos);
        }

    }
}
