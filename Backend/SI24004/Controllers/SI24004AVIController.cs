using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SI24004.Models;
using SI24004.Models.Requests;
using SI24004.Service;

namespace SI24004.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SI24004AVIController : ControllerBase
    {
        private readonly PostgrestContext _context;
        private readonly SI24004AVIService _service;

        public SI24004AVIController(PostgrestContext context,SI24004AVIService service)
        {
            _context = context;
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AviRequest>>> GetAviRequests()
        {
            // ดึงข้อมูลทั้งหมดจากตาราง avi_request
            var aviRequests = await _context.AviRequests.Where(x => x.IsDeleted != true).OrderBy(x => x.RequestCode).ToListAsync();
            return Ok(aviRequests);  // ส่งคืนข้อมูลในรูปแบบ JSON
        }
        // GET: api/avirequest/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AviRequest>> GetAviRequestById(Guid id)
        {
            var aviRequest = await _context.AviRequests.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (aviRequest == null)
            {
                return NotFound();
            }
            return aviRequest;
        }

        // DELETE: api/avirequest/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAviRequest(Guid id)
        {
            var aviRequest = await _context.AviRequests.FindAsync(id);
            if (aviRequest == null)
            {
                return NotFound();
            }
            aviRequest.IsDeleted = true;
            aviRequest.Active = false;
            _context.Entry(aviRequest).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpPost]
        public async Task<IActionResult> AddRequest([FromBody] AviRequestDto requestDto)
        {
            if (requestDto == null)
                return BadRequest("Request data is required.");
            try
            {
                // Map DTO to Entity
                var newRequest = new AviRequest
                {
                    RequestCode = await _service.GenerateRequestCode(),
                    RequestName = requestDto.RequestName,
                    UserId = requestDto.UserId,
                    RequestDescription = requestDto.RequestDescription,
                    AttachmentId = requestDto.AttachmentId,
                    RequestDate = requestDto.RequestDate ?? DateTime.Now,
                    RequestApprove = requestDto.RequestApprove,
                    ApproveDate = null,
                    Active = true,
                    IsDeleted = false,
                };

                // Add to Database
                await _context.AviRequests.AddAsync(newRequest);
                await _context.SaveChangesAsync();

                return Ok(new { Id = newRequest.Id, Message = "Request added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        [HttpPut]
        public async Task<IActionResult> UpdateRequest([FromBody] AviRequestDto requestDto)
        {
            if (requestDto == null || requestDto.Id == Guid.Empty) // ตรวจสอบว่า requestDto มี Id หรือไม่
                return BadRequest("Request data is invalid.");

            try
            {
                // ค้นหา Request ในฐานข้อมูลจาก Id
                var updateRequest = await _context.AviRequests.FindAsync(requestDto.Id);

                if (updateRequest == null) // ถ้าไม่พบรายการ
                    return NotFound($"Request with Id {requestDto.Id} not found.");

                // อัปเดตข้อมูล
                updateRequest.RequestName = requestDto.RequestName;
                updateRequest.UserId = requestDto.UserId;
                updateRequest.RequestDescription = requestDto.RequestDescription;
                updateRequest.AttachmentId = requestDto.AttachmentId;
                updateRequest.RequestDate = requestDto.RequestDate ?? updateRequest.RequestDate;
                updateRequest.RequestApprove = requestDto.RequestApprove;
                updateRequest.ApproveDate = requestDto.RequestApprove ? DateTime.Now : null;
                updateRequest.Active = requestDto.Active;
                updateRequest.IsDeleted = requestDto.IsDeleted;

                // บันทึกการเปลี่ยนแปลง
                _context.AviRequests.Update(updateRequest);
                await _context.SaveChangesAsync();

                return Ok(new { Id = updateRequest.Id, Message = "Request updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while processing your request. Error: {ex.Message}");
            }
        }

    }
}