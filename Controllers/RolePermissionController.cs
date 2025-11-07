using Microsoft.AspNetCore.Mvc;
using THMY_API.Models;
using THMY_API.Models.DBContext;
using Microsoft.EntityFrameworkCore;

namespace THMY_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RolePermissionController : ControllerBase
    {
        private readonly APIContext _context;
        private readonly ILogger<RolePermissionController> _logger;

        public RolePermissionController(APIContext context, ILogger<RolePermissionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<RolePermission>>> GetAllRolePermissions()
        {
            _logger.LogDebug("Getting all role-permission associations.");
            var rolePermissions = await _context.RolePermission.ToListAsync();
            return Ok(rolePermissions);
        }

        [HttpGet("role/{roleId}")]
        public async Task<ActionResult<List<RolePermission>>> GetRolePermissionsByRole(int roleId)
        {
            _logger.LogDebug("Getting permissions for role: {RoleId}", roleId);
            var rolePermissions = await _context.RolePermission
                .Where(rp => rp.roleId == roleId)
                .ToListAsync();
            return Ok(rolePermissions);
        }

        [HttpGet("permission/{permissionId}")]
        public async Task<ActionResult<List<RolePermission>>> GetRolePermissionsByPermission(int permissionId)
        {
            _logger.LogDebug("Getting roles for permission: {PermissionId}", permissionId);
            var rolePermissions = await _context.RolePermission
                .Where(rp => rp.permissionId == permissionId)
                .ToListAsync();
            return Ok(rolePermissions);
        }

        [HttpPost]
        public async Task<ActionResult<RolePermission>> CreateRolePermission([FromBody] RolePermission rolePermission)
        {
            _logger.LogDebug("Creating role-permission association: {RoleId} - {PermissionId}", rolePermission.roleId, rolePermission.permissionId);
            
            rolePermission.createdAt = DateTime.UtcNow;
            rolePermission.updatedAt = DateTime.UtcNow;

            if (await _context.RolePermission.AnyAsync(rp => 
                rp.roleId == rolePermission.roleId && 
                rp.permissionId == rolePermission.permissionId && 
                rp.systemId == rolePermission.systemId))
            {
                return BadRequest("This role-permission association already exists.");
            }

            _context.RolePermission.Add(rolePermission);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllRolePermissions), rolePermission);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRolePermission([FromBody] RolePermission rolePermission)
        {
            _logger.LogDebug("Deleting role-permission association: {RoleId} - {PermissionId}", rolePermission.roleId, rolePermission.permissionId);
            
            var entity = await _context.RolePermission
                .FirstOrDefaultAsync(rp => 
                    rp.roleId == rolePermission.roleId && 
                    rp.permissionId == rolePermission.permissionId && 
                    rp.systemId == rolePermission.systemId);
            
            if (entity == null)
            {
                return NotFound();
            }

            _context.RolePermission.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

