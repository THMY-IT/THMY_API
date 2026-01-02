using InternalSystem_ModelContext;
using InternalSystem_ModelContext.Models.SQLite;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;
using THMY_API.Models;
using THM_Encryption;

namespace THMY_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<UserController> _logger;
        readonly bool _isDebug = Log.IsEnabled(LogEventLevel.Debug);
        private Encryption _encryption;

        public UserController(DatabaseContext context, ILogger<UserController> logger, Encryption encryption)
        {
            _context = context;
            _logger = logger;
            _encryption = encryption;
        }

        [HttpPost]
        public async Task<ActionResult<Employee>> Login([FromBody] User user)
        {
            _logger.LogDebug("In PostRequest.");
            Employee employee;

            if (_context.employee.Any(e => e.empId == user.EmployeeID) == false)
            {
                _logger.LogDebug("No user found.");
                return Unauthorized(new{ Error = "Password or EmployeeID not found" });
            }

            employee = _context.employee.First(e => e.empId == user.EmployeeID);

            if (employee.password.Equals(user.Password) == false)
            {

                _logger.LogDebug("Wrong Password.");
                return Unauthorized(new { Error = "Password or EmployeeID not found" });
            }

            _logger.LogDebug("Authenticate done.");
            return Ok(new { EmployeeID = employee.empId, EmployeeName = employee.empName, Department = employee.department, Email = employee.email });
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<Employee>>> GetAllUsers()
        {
            _logger.LogDebug("In GetRequest.");
            _logger.LogInformation("In GetRequest.");
            if (true) Log.Debug("Someone is stuck debugging...");

            List<Employee> employees = _context.employee.ToList();
            return Ok(employees);
        }
    }
}
