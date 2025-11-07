using Microsoft.AspNetCore.Mvc;
using THMY_API.Models;
using THMY_API.Models.DBContext;
using Microsoft.EntityFrameworkCore;

namespace THMY_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmpRoleController : ControllerBase
    {
        private readonly APIContext _context;
        private readonly ILogger<EmpRoleController> _logger;

        public EmpRoleController(APIContext context, ILogger<EmpRoleController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<EmpRole>>> GetAllEmpRoles()
        {
            _logger.LogDebug("Getting all employee-role associations.");
            var empRoles = await _context.EmpRole.ToListAsync();
            return Ok(empRoles);
        }

        [HttpGet("employee/{empId}")]
        public async Task<ActionResult<List<EmpRole>>> GetEmpRolesByEmployee(string empId)
        {
            _logger.LogDebug("Getting roles for employee: {EmpId}", empId);
            var empRoles = await _context.EmpRole
                .Where(er => er.empId == empId)
                .ToListAsync();
            return Ok(empRoles);
        }

        [HttpGet("role/{roleId}")]
        public async Task<ActionResult<List<EmpRole>>> GetEmpRolesByRole(int roleId)
        {
            _logger.LogDebug("Getting employees for role: {RoleId}", roleId);
            var empRoles = await _context.EmpRole
                .Where(er => er.roleId == roleId)
                .ToListAsync();
            return Ok(empRoles);
        }

        [HttpPost]
        public async Task<ActionResult<EmpRole>> CreateEmpRole([FromBody] EmpRole empRole)
        {
            _logger.LogDebug("Creating employee-role association: {EmpId} - {RoleId}", empRole.empId, empRole.roleId);
            
            empRole.createdAt = DateTime.UtcNow;
            empRole.updatedAt = DateTime.UtcNow;

            if (await _context.EmpRole.AnyAsync(er => 
                er.empId == empRole.empId && 
                er.roleId == empRole.roleId && 
                er.systemId == empRole.systemId))
            {
                return BadRequest("This employee-role association already exists.");
            }

            _context.EmpRole.Add(empRole);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllEmpRoles), empRole);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteEmpRole([FromBody] EmpRole empRole)
        {
            _logger.LogDebug("Deleting employee-role association: {EmpId} - {RoleId}", empRole.empId, empRole.roleId);
            
            var entity = await _context.EmpRole
                .FirstOrDefaultAsync(er => 
                    er.empId == empRole.empId && 
                    er.roleId == empRole.roleId && 
                    er.systemId == empRole.systemId);
            
            if (entity == null)
            {
                return NotFound();
            }

            _context.EmpRole.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

