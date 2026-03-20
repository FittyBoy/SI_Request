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
            var statusINA = await _context.ListItems
                    .Where(x => x.ListItemCode == "LI02")
                    .FirstOrDefaultAsync();

            if (statusINA == null)
            {
                return NotFound("Status drawing not found.");
            }
            var statuses = await _context.Statuses.Where(x => x.StatusTypeId == statusINA.Id).ToListAsync();
            return Ok(statuses);
        }

        [HttpGet("DrawingStatus")]
        public async Task<IActionResult> DrawingStatus()
        {
            var statusDrawing = await _context.ListItems
                    .Where(x => x.ListItemCode == "LI04")
                    .FirstOrDefaultAsync();

            if (statusDrawing == null)
            {
                return NotFound("Status drawing not found.");
            }
            var statuses = await _context.Statuses.Where(x => x.StatusTypeId == statusDrawing.Id).OrderByDescending(x => x.Ordinal).ToListAsync();
            return Ok(statuses);
        }

        [HttpGet("DrawingDDR")]
        public async Task<IActionResult> DrawingDropdownList()
        {
            var statusDrawing = await _context.ListItems
                    .Where(x => x.ListItemCode == "LI03")
                    .FirstOrDefaultAsync();

            if (statusDrawing == null)
            {
                return NotFound("Status drawing not found.");
            }

            var statuses = await _context.Drawings
                .Where(x => x.ListItemId == statusDrawing.Id)
                .ToListAsync();

            return Ok(statuses);
        }

        [HttpGet("SectionDDR")]
        public async Task<IActionResult> SectionDropdownList()
        {
            var SectionDDR = await _context.ListItems
                    .Where(x => x.ListItemCode == "LI01")
                    .FirstOrDefaultAsync();

            if (SectionDDR == null)
            {
                return NotFound("Status drawing not found.");
            }

            var statuses = await _context.Sections
                .ToListAsync();

            return Ok(statuses);
        }
        [HttpGet("MechineDDR")]
        public async Task<IActionResult> MCDropdownList()
        {
            var SectionDDR = await _context.ListItems
                    .Where(x => x.ListItemCode == "LI08")
                    .FirstOrDefaultAsync();

            if (SectionDDR == null)
            {
                return NotFound("Status drawing not found.");
            }

            var statuses = await _context.RequestMachines
                .Where(x => x.ListItemId == SectionDDR.Id)
                .ToListAsync();

            return Ok(statuses);
        }
        [HttpGet("ShiftDDR")]
        public async Task<IActionResult> ShiftDropdownList()
        {
            var ShiftDDR = await _context.ListItems
                .Where(x => x.ListItemCode == "LI09")
                .FirstOrDefaultAsync();

            if(ShiftDDR == null)
            {
                return NotFound("Data not found.");
            }

            var shift = await _context.Shifts
                .Where(x => x.ListItemId != ShiftDDR.Id)
                .ToListAsync();

            return Ok(shift);
        }
        [HttpGet("ObjectiveDDR")]
        public async Task<IActionResult> ObjectiveDropdownList()
        {
            var ShiftDDR = await _context.ListItems
                .Where(x => x.ListItemCode == "LI15")
                .FirstOrDefaultAsync();

            if (ShiftDDR == null)
            {
                return NotFound("Data not found.");
            }

            var shift = await _context.Objectives
                .Where(x => x.ListItemId != ShiftDDR.Id)
                .ToListAsync();

            return Ok(shift);
        }
    }
}
