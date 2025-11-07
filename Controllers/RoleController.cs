using Microsoft.AspNetCore.Mvc;
using THMY_API.Models;
using THMY_API.Models.DBContext;
using Microsoft.EntityFrameworkCore;

namespace THMY_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly APIContext _context;
        private readonly ILogger<RoleController> _logger;

        public RoleController(APIContext context, ILogger<RoleController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<Role>>> GetAllRoles()
        {
            _logger.LogDebug("Getting all roles.");
            var roles = await _context.Role.ToListAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRole(int id)
        {
            _logger.LogDebug("Getting role with ID: {RoleId}", id);
            var role = await _context.Role.FindAsync(id);
            
            if (role == null)
            {
                return NotFound();
            }

            return Ok(role);
        }

        [HttpPost]
        public async Task<ActionResult<Role>> CreateRole([FromBody] Role role)
        {
            _logger.LogDebug("Creating new role: {RoleName}", role.roleName);
            
            role.createdAt = DateTime.UtcNow;
            role.updatedAt = DateTime.UtcNow;

            if (await _context.Role.AnyAsync(r => r.roleId == role.roleId))
            {
                return BadRequest("Role with this ID already exists.");
            }

            _context.Role.Add(role);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRole), new { id = role.roleId }, role);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] Role role)
        {
            _logger.LogDebug("Updating role with ID: {RoleId}", id);

            role.updatedAt = DateTime.UtcNow;
            
            if (id != role.roleId)
            {
                return BadRequest("Role ID mismatch.");
            }

            _context.Entry(role).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Role.AnyAsync(r => r.roleId == id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            _logger.LogDebug("Deleting role with ID: {RoleId}", id);
            
            var role = await _context.Role.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            _context.Role.Remove(role);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

