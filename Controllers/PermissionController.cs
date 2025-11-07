using Microsoft.AspNetCore.Mvc;
using THMY_API.Models;
using THMY_API.Models.DBContext;
using Microsoft.EntityFrameworkCore;

namespace THMY_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PermissionController : ControllerBase
    {
        private readonly APIContext _context;
        private readonly ILogger<PermissionController> _logger;

        public PermissionController(APIContext context, ILogger<PermissionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<Permission>>> GetAllPermissions()
        {
            _logger.LogDebug("Getting all permissions.");
            var permissions = await _context.Permission.ToListAsync();
            return Ok(permissions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Permission>> GetPermission(int id)
        {
            _logger.LogDebug("Getting permission with ID: {PermissionId}", id);
            var permission = await _context.Permission.FindAsync(id);
            
            if (permission == null)
            {
                return NotFound();
            }

            return Ok(permission);
        }

        [HttpGet("system/{systemId}")]
        public async Task<ActionResult<List<Permission>>> GetPermissionsBySystem(int systemId)
        {
            _logger.LogDebug("Getting permissions for system ID: {SystemId}", systemId);
            var permissions = await _context.Permission
                .Where(p => p.systemId == systemId)
                .ToListAsync();
            return Ok(permissions);
        }

        [HttpPost]
        public async Task<ActionResult<Permission>> CreatePermission([FromBody] Permission permission)
        {
            _logger.LogDebug("Creating new permission: {PermissionName}", permission.permissionName);
            
            permission.createdAt = DateTime.UtcNow;
            permission.updatedAt = DateTime.UtcNow;

            if (await _context.Permission.AnyAsync(p => p.permissionId == permission.permissionId))
            {
                return BadRequest("Permission with this ID already exists.");
            }

            _context.Permission.Add(permission);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPermission), new { id = permission.permissionId }, permission);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePermission(int id, [FromBody] Permission permission)
        {
            _logger.LogDebug("Updating permission with ID: {PermissionId}", id);

            permission.updatedAt = DateTime.UtcNow;
            
            if (id != permission.permissionId)
            {
                return BadRequest("Permission ID mismatch.");
            }

            _context.Entry(permission).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Permission.AnyAsync(p => p.permissionId == id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            _logger.LogDebug("Deleting permission with ID: {PermissionId}", id);
            
            var permission = await _context.Permission.FindAsync(id);
            if (permission == null)
            {
                return NotFound();
            }

            _context.Permission.Remove(permission);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
