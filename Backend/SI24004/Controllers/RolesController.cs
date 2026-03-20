using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SI24004.Models.PostgreSQL;
using SI24004.Services;
using System.Security.Claims;

namespace SI24004.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly PostgrestContext _context;
        public RolesController(PostgrestContext context)
        {
            _context = context;
        }

        [HttpGet("statuses")]
        public async Task<IActionResult> GetStatuses()
        {
            var statuses = await _context.Statuses.ToListAsync();
            return Ok(statuses);
        }

        [HttpGet("DrawingStatus")]
        public async Task<IActionResult> DrawingStatus()
        {
            var statuses = await _context.Statuses.ToListAsync();
            return Ok(statuses);
        }

        [HttpGet("DrawingDDR")]
        public async Task<IActionResult> DrawingDropdownList()
        {
            // ListItems/Drawings table not in current schema - return empty
            return Ok(new List<object>());
        }

        [HttpGet("SectionDDR")]
        public async Task<IActionResult> SectionDropdownList()
        {
            var sections = await _context.Sections.ToListAsync();
            return Ok(sections);
        }

        [HttpGet("MechineDDR")]
        public async Task<IActionResult> MCDropdownList()
        {
            // RequestMachines table not in current schema - return empty
            return Ok(new List<object>());
        }

        [HttpGet("ShiftDDR")]
        public async Task<IActionResult> ShiftDropdownList()
        {
            var shifts = await _context.Shifts.ToListAsync();
            return Ok(shifts);
        }

        [HttpGet("ObjectiveDDR")]
        public async Task<IActionResult> ObjectiveDropdownList()
        {
            // Objectives table not in current schema - return empty
            return Ok(new List<object>());
        }
    }
}
