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
        public IActionResult DrawingDropdownList()
        {
            var drawingTypes = new List<object>
            {
                new { Id = "dt-part",    DrawingName = "Part" },
                new { Id = "dt-machine", DrawingName = "Machine" },
                new { Id = "dt-table",   DrawingName = "Table" },
                new { Id = "dt-jig",     DrawingName = "Jig" },
                new { Id = "dt-tool",    DrawingName = "Tool" },
                new { Id = "dt-other",   DrawingName = "Other" },
            };
            return Ok(drawingTypes);
        }

        [HttpGet("SectionDDR")]
        public async Task<IActionResult> SectionDropdownList()
        {
            var sections = await _context.Sections
                .Select(s => new { s.Id, SectionName = s.Name, s.Code })
                .ToListAsync();
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
