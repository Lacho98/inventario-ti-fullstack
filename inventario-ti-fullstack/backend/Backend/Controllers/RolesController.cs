using Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetRoles()
        {
            var roles = await _context.Roles
                .OrderBy(r => r.NombreRol)
                .Select(r => new
                {
                    r.Id,
                    r.NombreRol
                })
                .ToListAsync();

            return Ok(roles);
        }
    }
}
