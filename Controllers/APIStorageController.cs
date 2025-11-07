using Microsoft.AspNetCore.Mvc;
using THMY_API.Models;
using THMY_API.Models.DBContext;
using Microsoft.EntityFrameworkCore;

namespace THMY_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class APIStorageController : ControllerBase
    {
        private readonly APIContext _context;
        private readonly ILogger<APIStorageController> _logger;

        public APIStorageController(APIContext context, ILogger<APIStorageController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<APIStorage>>> GetAllAPIStorages()
        {
            _logger.LogDebug("Getting all API storages.");
            var apiStorages = await _context.APIStroageKey.ToListAsync();
            return Ok(apiStorages);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<APIStorage>> GetAPIStorage(int id)
        {
            _logger.LogDebug("Getting API storage with ID: {Id}", id);
            var apiStorage = await _context.APIStroageKey.FindAsync(id);
            
            if (apiStorage == null)
            {
                return NotFound();
            }

            return Ok(apiStorage);
        }

        [HttpPost]
        public async Task<ActionResult<APIStorage>> CreateAPIStorage([FromBody] APIStorage apiStorage)
        {
            _logger.LogDebug("Creating new API storage: {ApplicationName}", apiStorage.applicationName);
            
            apiStorage.createdAt = DateTime.UtcNow;
            apiStorage.updatedAt = DateTime.UtcNow;

            _context.APIStroageKey.Add(apiStorage);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAPIStorage), new { id = apiStorage.id }, apiStorage);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAPIStorage(int id, [FromBody] APIStorage apiStorage)
        {
            _logger.LogDebug("Updating API storage with ID: {Id}", id);

            apiStorage.updatedAt = DateTime.UtcNow;

            if (id != apiStorage.id)
            {
                return BadRequest("API Storage ID mismatch.");
            }

            _context.Entry(apiStorage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.APIStroageKey.AnyAsync(a => a.id == id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAPIStorage(int id)
        {
            _logger.LogDebug("Deleting API storage with ID: {Id}", id);
            
            var apiStorage = await _context.APIStroageKey.FindAsync(id);
            if (apiStorage == null)
            {
                return NotFound();
            }

            _context.APIStroageKey.Remove(apiStorage);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

