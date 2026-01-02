using InternalSystem_ModelContext;
using InternalSystem_ModelContext.Models.SQLite;
using Microsoft.AspNetCore.Mvc;
using THMY_API.Models;

namespace THMY_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(DatabaseContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<Employee>> Login([FromBody] User user)
        {
            _logger.LogDebug("In PostRequest.");
            Employee employee;

            if (_context.employee.Any(e => e.empId == user.EmployeeID) == false)
            {
                _logger.LogDebug("No employee found.");
                return Forbid("Employee not found.");
            }

            _logger.LogDebug("Employee Found.");
            employee = _context.employee.First(e => e.empId == user.EmployeeID);

            if (employee.status.Equals("Inactive"))
            {
                _logger.LogDebug("Employee status is inactive.");
                return Unauthorized("Employee status is inactive.");
            }

            if (employee.password.Equals(user.Password) == false)
            {
                _logger.LogDebug("Wrong Password.");
                return Unauthorized("Incorrect EmployeeID or Password.");
            }

            _logger.LogDebug("Authenticate done.");
            return Ok(new { EmployeeID = employee.empId, EmployeeName = employee.empName, Department = employee.department });
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<Employee>>> GetAllUsers()
        {
            _logger.LogDebug("In GetRequest.");

            List<Employee> employees = _context.employee.ToList();
            return Ok(employees);
        }
    }
}
