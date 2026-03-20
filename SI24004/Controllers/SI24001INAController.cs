using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SI24004.Models.DTOs;
using SI24004.Models.PostgreSQL;
using SI24004.Services;
using Microsoft.AspNetCore.Identity.Data;
using SI24004.Enums;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Linq;

namespace SI24004.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SI24001INAController : ControllerBase
    {
        private readonly PostgrestContext _context;
        private readonly SI24004AVIService _service;

        public SI24001INAController(PostgrestContext context, SI24004AVIService service)
        {
            _context = context;
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetInaRequests()
        {
            try
            {
                // Get all requests
                var requests = await _context.InaRequests
                    .Where(x => x.IsDeleted != true)
                    .Include(x => x.Status)
                    .OrderBy(x => x.RequestCode)
                    .ThenBy(x => x.RequestDate)
                    .ToListAsync();

                // Get all attachments for these requests
                List<Guid> requestIds = requests.Select(r => r.Id).ToList();
                var attachments = await _context.Attachments
                                .Where(a => a.RequestId != null &&
                                    requestIds.Contains(a.RequestId.Value) &&
                                    a.IsDeleted == false)
                                .ToListAsync();

                // Get all lots for these requests
                var lots = await _context.LotRequests
                    .Where(l => requestIds.Contains(l.RequestId) && l.IsDeleted == false)
                    .OrderBy(l => l.LotNo)
                    .ToListAsync();

                // Group attachments by RequestId and Category
                var attachmentsByRequest = attachments.GroupBy(a => a.RequestId).ToDictionary(g => g.Key, g => g.ToList());
                var lotsByRequest = lots.GroupBy(l => l.RequestId).ToDictionary(g => g.Key, g => g.ToList());

                var result = requests.Select(x => new
                {
                    Id = x.Id,
                    RequestCode = x.RequestCode,
                    // ลบ RequestName ออก
                    RequestPurpose = x.RequestPurpose, // ย้าย Purpose มาช่องแรก
                    RequestDescription = x.RequestDescription,
                    StatusId = x.StatusId,
                    AttachmentId = x.AttachmentId,
                    Active = x.Active,
                    IsDeleted = x.IsDeleted,
                    UserId = x.UserId,
                    RequestDate = x.RequestDate,
                    RequestMachine = x.RequestMachine,
                    RequestProduct = x.RequestProduct,
                    RequestMass = x.RequestMass,
                    RequestTest = x.RequestTest,
                    RequestComment1 = x.RequestComment1,
                    RequestComment2 = x.RequestComment2,
                    RequestComment3 = x.RequestComment3,
                    Recipe = x.Recipe,
                    OtherPrograms = x.OtherPrograms,
                    RequestBy = x.RequestBy,
                    RequestProcess = x.RequestProcess,
                    RequestStartDate = x.RequestStartDate,
                    RequestFinishDate = x.RequestFinishDate,
                    FlTgDeleted = x.FlTgDeleted,
                    FlMcDeleted = x.FlMcDeleted,
                    FlCheckMass = x.FlCheckMass,
                    FlDeletedOther = x.FlDeletedOther,
                    CtCopyRp = x.CtCopyRp,
                    CtRpDeleted = x.CtRpDeleted,
                    CtBookCheck = x.CtBookCheck,
                    CtDeletedOther = x.CtDeletedOther,
                    FlTgDeletedComment = x.FlTgDeletedComment,
                    FlMcDeletedComment = x.FlMcDeletedComment,
                    FlCheckMassComment = x.FlCheckMassComment,
                    FlDeletedOtherComment = x.FlDeletedOtherComment,
                    CtCopyRpComment = x.CtCopyRpComment,
                    CtRpDeletedComment = x.CtRpDeletedComment,
                    CtBookCheckComment = x.CtBookCheckComment,
                    CtDeletedOtherComment = x.CtDeletedOtherComment,
                    RequestMcNo = x.RequestMcNo,
                    RequestBook = x.RequestBook,
                    RequestInstallDate = x.RequestInstallDate,
                    RequestClearDate = x.RequestClearDate,
                    Status = x.Status != null ? new
                    {
                        Id = x.Status.Id,
                        StatusName = x.Status.StatusName,
                        Ordinal = x.Status.Ordinal
                    } : null,

                    // เพิ่ม LotNumbers ในการตอบกลับ
                    LotNumbers = lotsByRequest.TryGetValue(x.Id, out var lots)
                        ? lots.Select(l => l.LotNo).ToList()
                        : new List<string>(),

                    Attachments = new
                    {
                        Recipe = attachmentsByRequest.TryGetValue(x.Id, out var attachments)
                            ? attachments.Where(a => a.Category?.ToLower() == "recipe")
                                        .Select(a => new { a.Id, a.AttachmentName, a.AttachementPath, a.AttachementType, a.Category, a.UploadDate, a.AttachmentSize, a.AttachmentFileLocation })
                                        .ToList<object>()
                            : new List<object>(),

                        // Other Programs - รองรับหลายตัว
                        OtherPrograms = attachmentsByRequest.TryGetValue(x.Id, out var attachments2)
                            ? attachments2.Where(a => a.Category?.ToLower() == "otherprograms")
                                         .Select(a => new { a.Id, a.AttachmentName, a.AttachementPath, a.AttachementType, a.Category, a.UploadDate, a.AttachmentSize, a.AttachmentFileLocation })
                                         .ToList<object>()
                            : new List<object>(),

                        General = attachmentsByRequest.TryGetValue(x.Id, out var attachments3)
                            ? attachments3.Where(a => a.Category?.ToLower() == "general")
                                         .Select(a => new { a.Id, a.AttachmentName, a.AttachementPath, a.AttachementType, a.Category, a.UploadDate, a.AttachmentSize, a.AttachmentFileLocation })
                                         .ToList<object>()
                            : new List<object>()
                    }
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving requests: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetInaRequest(Guid id)
        {
            try
            {
                var inaRequest = await _context.InaRequests
                    .Where(x => x.Id == id && x.IsDeleted != true)
                    .Include(x => x.Status)
                    .FirstOrDefaultAsync();

                if (inaRequest == null)
                    return NotFound(new { message = "Request not found" });

                // Get attachments for this request
                var attachments = await _context.Attachments
                    .Where(a => a.RequestId == id && a.IsDeleted == false)
                    .ToListAsync();

                // Get lots for this request
                var lots = await _context.LotRequests
                    .Where(l => l.RequestId == id && l.IsDeleted == false)
                    .OrderBy(l => l.LotNo)
                    .ToListAsync();

                var result = new
                {
                    Id = inaRequest.Id,
                    RequestCode = inaRequest.RequestCode,
                    // ลบ RequestName ออก
                    RequestPurpose = inaRequest.RequestPurpose, // ย้าย Purpose มาช่องแรก
                    RequestDescription = inaRequest.RequestDescription,
                    StatusId = inaRequest.StatusId,
                    AttachmentId = inaRequest.AttachmentId,
                    Active = inaRequest.Active,
                    IsDeleted = inaRequest.IsDeleted,
                    UserId = inaRequest.UserId,
                    RequestDate = inaRequest.RequestDate,
                    RequestMachine = inaRequest.RequestMachine,
                    RequestProduct = inaRequest.RequestProduct,
                    RequestMass = inaRequest.RequestMass,
                    RequestTest = inaRequest.RequestTest,
                    RequestComment1 = inaRequest.RequestComment1,
                    RequestComment2 = inaRequest.RequestComment2,
                    RequestComment3 = inaRequest.RequestComment3,
                    Recipe = inaRequest.Recipe,
                    OtherPrograms = inaRequest.OtherPrograms,
                    RequestBy = inaRequest.RequestBy,
                    RequestProcess = inaRequest.RequestProcess,
                    RequestStartDate = inaRequest.RequestStartDate,
                    RequestFinishDate = inaRequest.RequestFinishDate,
                    FlTgDeleted = inaRequest.FlTgDeleted,
                    FlMcDeleted = inaRequest.FlMcDeleted,
                    FlCheckMass = inaRequest.FlCheckMass,
                    FlDeletedOther = inaRequest.FlDeletedOther,
                    CtCopyRp = inaRequest.CtCopyRp,
                    CtRpDeleted = inaRequest.CtRpDeleted,
                    CtBookCheck = inaRequest.CtBookCheck,
                    CtDeletedOther = inaRequest.CtDeletedOther,
                    FlTgDeletedComment = inaRequest.FlTgDeletedComment,
                    FlMcDeletedComment = inaRequest.FlMcDeletedComment,
                    FlCheckMassComment = inaRequest.FlCheckMassComment,
                    FlDeletedOtherComment = inaRequest.FlDeletedOtherComment,
                    CtCopyRpComment = inaRequest.CtCopyRpComment,
                    CtRpDeletedComment = inaRequest.CtRpDeletedComment,
                    CtBookCheckComment = inaRequest.CtBookCheckComment,
                    CtDeletedOtherComment = inaRequest.CtDeletedOtherComment,
                    RequestMcNo = inaRequest.RequestMcNo,
                    RequestBook = inaRequest.RequestBook,
                    RequestInstallDate = inaRequest.RequestInstallDate,
                    Status = inaRequest.Status != null ? new
                    {
                        Id = inaRequest.Status.Id,
                        StatusName = inaRequest.Status.StatusName,
                        Ordinal = inaRequest.Status.Ordinal
                    } : null,

                    // เพิ่ม LotNumbers ในการตอบกลับ
                    LotNumbers = lots.Select(l => l.LotNo).ToList(),

                    Attachments = new
                    {
                        Recipe = attachments
                            .Where(a => a.Category != null && a.Category.ToLower() == "recipe")
                            .Select(a => new
                            {
                                a.Id,
                                a.AttachmentName,
                                a.AttachementPath,
                                a.AttachementType,
                                a.Category,
                                a.UploadDate,
                                a.AttachmentSize,
                                a.AttachmentFileLocation
                            }).ToList(),

                        // Other Programs - รองรับหลายตัว
                        OtherPrograms = attachments
                            .Where(a => a.Category != null && a.Category.ToLower() == "otherprograms")
                            .Select(a => new
                            {
                                a.Id,
                                a.AttachmentName,
                                a.AttachementPath,
                                a.AttachementType,
                                a.Category,
                                a.UploadDate,
                                a.AttachmentSize,
                                a.AttachmentFileLocation
                            }).ToList(),

                        General = attachments
                            .Where(a => a.Category != null && a.Category.ToLower() == "general")
                            .Select(a => new
                            {
                                a.Id,
                                a.AttachmentName,
                                a.AttachementPath,
                                a.AttachementType,
                                a.Category,
                                a.UploadDate,
                                a.AttachmentSize,
                                a.AttachmentFileLocation
                            }).ToList()
                    },

                    Lots = lots.Select(l => new
                    {
                        l.Id,
                        l.LotNo,
                        l.RequestId
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving request: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInaRequest(Guid id)
        {
            try
            {
                var inaRequest = await _context.InaRequests.FindAsync(id);
                if (inaRequest == null)
                {
                    return NotFound(new { message = "Request not found" });
                }

                inaRequest.IsDeleted = true;
                inaRequest.Active = false;
                _context.Entry(inaRequest).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Request has been soft deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting request: {ex.Message}");
            }
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddRequest(
                    [FromForm] InaRequestDto requestDto,
                    [FromForm] List<AttachmentDto> attachments,
                    [FromForm] List<LotInaDto> lots,
                    [FromForm] List<string> otherPrograms) // เพิ่มรองรับ other programs array
        {
            // Validation
            if (requestDto == null)
                return BadRequest("Request data is required.");

            // ลบการ validate RequestName เพราะไม่มีแล้ว
            if (string.IsNullOrWhiteSpace(requestDto.RequestPurpose))
                return BadRequest("Request purpose is required.");

            if (requestDto.UserId == Guid.Empty)
                return BadRequest("User ID is required.");

            // Validate lots - ใช้ validation แบบ required เฉยๆ
            if (lots == null || !lots.Any())
                return BadRequest("At least one lot is required.");

            if (lots.Count > 10)
                return BadRequest("Maximum 10 lots allowed per request.");

            // Validate lot numbers - แค่ check ว่าไม่ว่าง
            var lotNumbers = lots.Where(l => !string.IsNullOrWhiteSpace(l.LotNo)).Select(l => l.LotNo.Trim()).ToList();
            if (lotNumbers.Count != lotNumbers.Distinct().Count())
                return BadRequest("Duplicate lot numbers are not allowed.");

            if (lotNumbers.Count == 0)
                return BadRequest("At least one valid lot number is required.");

            try
            {
                var newRequestId = requestDto.Id ?? Guid.NewGuid();

                // Get default status
                var defaultStatus = await _context.Statuses
                    .Where(s => s.Ordinal == 2 && s.StatusTypeId == Guid.Parse("7a747f9a-3d80-47bf-9157-2341938c71e6"))
                    .FirstOrDefaultAsync();

                if (defaultStatus == null)
                    return BadRequest("Default status not found.");

                // Validate user exists
                var userExists = await _context.Users.AnyAsync(u => u.Id == requestDto.UserId);
                if (!userExists)
                    return BadRequest("User not found.");

                Guid? firstAttachmentId = null;
                Guid? recipeAttachmentId = null;
                var otherProgramsAttachmentIds = new List<Guid>(); // เปลี่ยนเป็น list

                // Handle attachments - ปรับปรุงให้รองรับ other programs หลายตัว
                if (attachments != null && attachments.Any())
                {
                    foreach (var attachment in attachments.Where(a => !string.IsNullOrWhiteSpace(a.AttachmentName) && !string.IsNullOrWhiteSpace(a.AttachementPath)))
                    {
                        var attachmentId = Guid.NewGuid();
                        var newAttachment = new Models.Attachment
                        {
                            Id = attachmentId,
                            AttachmentName = attachment.AttachmentName,
                            AttachementPath = attachment.AttachementPath,
                            AttachementType = attachment.AttachementType ?? "",
                            UploadDate = DateTime.UtcNow,
                            AttachementFileData = attachment.AttachementFileData,
                            IsDeleted = false,
                            RequestId = newRequestId,
                            Category = attachment.Category,
                            AttachmentSize = !string.IsNullOrEmpty(attachment.AttachmentSize) ? attachment.AttachmentSize : null,
                            AttachmentFileLocation = !string.IsNullOrEmpty(attachment.AttachmentFileLocation) ? attachment.AttachmentFileLocation : null 
                        };

                        await _context.Attachments.AddAsync(newAttachment);

                        // Set specific attachment IDs based on category
                        if (attachment.Category?.ToLower() == "recipe")
                        {
                            recipeAttachmentId = attachmentId;
                        }
                        else if (attachment.Category?.ToLower() == "otherprograms")
                        {
                            otherProgramsAttachmentIds.Add(attachmentId); // เก็บ multiple IDs
                        }

                        // Set first attachment ID if not set
                        firstAttachmentId ??= attachmentId;
                    }
                }

                // Create new request
                var newRequest = new InaRequest
                {
                    Id = newRequestId,
                    RequestCode = await _service.GenerateRequestCodeIna(),
                    // ลบ RequestName ออก
                    RequestPurpose = requestDto.RequestPurpose, // ย้าย Purpose มาช่องแรก
                    UserId = requestDto.UserId,
                    RequestDescription = requestDto.RequestDescription ?? "", // อนุญาตให้ blank ได้
                    StatusId = defaultStatus.Id,
                    RequestDate = requestDto.RequestDate ?? DateTime.UtcNow,
                    RequestMachine = requestDto.RequestMachine,
                    RequestProduct = requestDto.RequestProduct ?? "",
                    RequestMass = requestDto.RequestMass ?? false,
                    RequestTest = requestDto.RequestTest ?? true,
                    RequestComment1 = requestDto.RequestComment1,
                    RequestComment2 = requestDto.RequestComment2,
                    RequestComment3 = requestDto.RequestComment3,
                    Recipe = recipeAttachmentId,
                    // OtherPrograms สามารถเก็บ array ได้ หรือจะเก็บเป็น JSON string
                    OtherPrograms = null,
                    RequestBy = requestDto.RequestBy,
                    RequestProcess = requestDto.RequestProcess,
                    RequestStartDate = requestDto.RequestStartDate?.ToUniversalTime(),
                    RequestFinishDate = requestDto.RequestFinishDate?.ToUniversalTime(),
                    Active = true,
                    IsDeleted = false,
                    AttachmentId = firstAttachmentId,
                    FlTgDeleted = requestDto.FlTgDeleted ?? false,
                    FlMcDeleted = requestDto.FlMcDeleted ?? false,
                    FlCheckMass = requestDto.FlCheckMass ?? false,
                    FlDeletedOther = requestDto.FlDeletedOther ?? false,
                    CtCopyRp = requestDto.CtCopyRp ?? false,
                    CtRpDeleted = requestDto.CtRpDeleted ?? false,
                    CtBookCheck = requestDto.CtBookCheck ?? false,
                    CtDeletedOther = requestDto.CtDeletedOther ?? false,
                    FlTgDeletedComment = requestDto.FlTgDeletedComment ?? string.Empty,
                    FlMcDeletedComment = requestDto.FlMcDeletedComment ?? string.Empty,
                    FlCheckMassComment = requestDto.FlCheckMassComment ?? string.Empty,
                    FlDeletedOtherComment = requestDto.FlDeletedOtherComment ?? string.Empty,
                    CtCopyRpComment = requestDto.CtCopyRpComment ?? string.Empty,
                    CtRpDeletedComment = requestDto.CtRpDeletedComment ?? string.Empty,
                    CtBookCheckComment = requestDto.CtBookCheckComment ?? string.Empty,
                    CtDeletedOtherComment = requestDto.CtDeletedOtherComment ?? string.Empty,
                };

                await _context.InaRequests.AddAsync(newRequest);

                // Handle lots
                var createdLots = new List<object>();
                foreach (var lot in lots.Where(l => !string.IsNullOrWhiteSpace(l.LotNo)))
                {
                    var lotId = Guid.NewGuid();
                    var newLot = new LotRequest
                    {
                        Id = lotId,
                        LotNo = lot.LotNo.Trim(),
                        RequestId = newRequestId,
                        IsDeleted = false
                    };

                    await _context.LotRequests.AddAsync(newLot);
                    createdLots.Add(new { Id = lotId, LotNo = newLot.LotNo });
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Id = newRequest.Id,
                    RequestCode = newRequest.RequestCode,
                    AttachmentId = firstAttachmentId,
                    RecipeId = recipeAttachmentId,
                    OtherProgramsIds = otherProgramsAttachmentIds, // ส่งกลับ multiple IDs
                    Lots = createdLots,
                    Message = "Request added successfully"
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        [HttpPut]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateRequest(
            [FromForm] InaRequestDto requestDto,
            [FromForm] List<AttachmentDto> attachments,
            [FromForm] List<LotInaDto> lots,
            [FromForm] List<string> otherPrograms, // เพิ่มรองรับ other programs array
            [FromForm] bool isApproved = false)
        {
            if (requestDto == null || requestDto.Id == Guid.Empty)
                return BadRequest("Request data is invalid.");

            // Validate lots if provided
            if (lots != null && lots.Any())
            {
                if (lots.Count > 10)
                    return BadRequest("Maximum 10 lots allowed per request.");

                var lotNumbers = lots.Where(l => !string.IsNullOrWhiteSpace(l.LotNo)).Select(l => l.LotNo.Trim()).ToList();
                if (lotNumbers.Count != lotNumbers.Distinct().Count())
                    return BadRequest("Duplicate lot numbers are not allowed.");
            }

            try
            {
                var updateRequest = await _context.InaRequests
                    .Include(r => r.Status)
                    .FirstOrDefaultAsync(r => r.Id == requestDto.Id);

                if (updateRequest == null)
                    return NotFound($"Request with Id {requestDto.Id} not found.");

                var currentStatus = updateRequest.Status;

                // Update Status only if approved
                if (isApproved && currentStatus != null)
                {
                    var nextStatus = await _context.Statuses
                        .Where(s => s.Ordinal > currentStatus.Ordinal)
                        .OrderBy(s => s.Ordinal)
                        .FirstOrDefaultAsync();

                    if (nextStatus != null)
                    {
                        updateRequest.StatusId = nextStatus.Id;
                    }
                }
                else
                {
                    updateRequest.StatusId = requestDto.StatusId;
                }

                // Update Request data - ลบ RequestName และใช้ Purpose แทน
                updateRequest.RequestPurpose = requestDto.RequestPurpose ?? updateRequest.RequestPurpose;
                updateRequest.UserId = requestDto.UserId != Guid.Empty ? requestDto.UserId : updateRequest.UserId;
                updateRequest.RequestDescription = requestDto.RequestDescription ?? updateRequest.RequestDescription; // อนุญาตให้ blank
                updateRequest.RequestDate = requestDto.RequestDate?.ToUniversalTime() ?? updateRequest.RequestDate?.ToUniversalTime();
                updateRequest.RequestMachine = requestDto.RequestMachine ?? updateRequest.RequestMachine;
                updateRequest.RequestProduct = requestDto.RequestProduct ?? updateRequest.RequestProduct;
                updateRequest.RequestMass = requestDto.RequestMass ?? updateRequest.RequestMass;
                updateRequest.RequestTest = requestDto.RequestTest ?? updateRequest.RequestTest;
                updateRequest.Recipe = requestDto.Recipe ?? updateRequest.Recipe;
                updateRequest.RequestBy = requestDto.RequestBy ?? updateRequest.RequestBy;
                updateRequest.RequestProcess = requestDto.RequestProcess ?? updateRequest.RequestProcess;
                updateRequest.RequestStartDate = requestDto.RequestStartDate?.ToUniversalTime() ?? updateRequest.RequestStartDate?.ToUniversalTime();
                updateRequest.RequestFinishDate = requestDto.RequestFinishDate?.ToUniversalTime() ?? updateRequest.RequestFinishDate?.ToUniversalTime();
                updateRequest.Active = requestDto.Active;
                updateRequest.IsDeleted = requestDto.IsDeleted ?? updateRequest.IsDeleted;

                // Update Boolean Flags
                updateRequest.FlTgDeleted = requestDto.FlTgDeleted ?? false;
                updateRequest.FlMcDeleted = requestDto.FlMcDeleted ?? false;
                updateRequest.FlCheckMass = requestDto.FlCheckMass ?? false;
                updateRequest.FlDeletedOther = requestDto.FlDeletedOther ?? false;
                updateRequest.CtCopyRp = requestDto.CtCopyRp ?? false;
                updateRequest.CtRpDeleted = requestDto.CtRpDeleted ?? false;
                updateRequest.CtBookCheck = requestDto.CtBookCheck ?? false;
                updateRequest.CtDeletedOther = requestDto.CtDeletedOther ?? false;

                // Update Comment Fields
                updateRequest.FlTgDeletedComment = requestDto.FlTgDeletedComment ?? string.Empty;
                updateRequest.FlMcDeletedComment = requestDto.FlMcDeletedComment ?? string.Empty;
                updateRequest.FlCheckMassComment = requestDto.FlCheckMassComment ?? string.Empty;
                updateRequest.FlDeletedOtherComment = requestDto.FlDeletedOtherComment ?? string.Empty;
                updateRequest.CtCopyRpComment = requestDto.CtCopyRpComment ?? string.Empty;
                updateRequest.CtRpDeletedComment = requestDto.CtRpDeletedComment ?? string.Empty;
                updateRequest.CtBookCheckComment = requestDto.CtBookCheckComment ?? string.Empty;
                updateRequest.CtDeletedOtherComment = requestDto.CtDeletedOtherComment ?? string.Empty;

                updateRequest.RequestBook = requestDto.RequestBook ?? string.Empty;
                updateRequest.RequestMcNo = requestDto.RequestMcNo ?? string.Empty;

                // Handle Comments based on status
                if (currentStatus != null)
                {
                    if (currentStatus.Ordinal >= 2)
                    {
                        updateRequest.RequestComment1 = requestDto.RequestComment1 ?? updateRequest.RequestComment1;
                    }
                    if (currentStatus.Ordinal >= 3)
                    {
                        updateRequest.RequestInstallDate = requestDto.RequestInstallDate ?? updateRequest.RequestInstallDate;
                        updateRequest.RequestComment2 = requestDto.RequestComment2 ?? updateRequest.RequestComment2;
                    }
                    if (currentStatus.Ordinal >= 4)
                    {
                        updateRequest.RequestInstallDate = requestDto.RequestInstallDate ?? updateRequest.RequestInstallDate;
                        updateRequest.RequestClearDate = requestDto.RequestClearDate ?? updateRequest.RequestClearDate;
                        updateRequest.RequestComment3 = requestDto.RequestComment3 ?? updateRequest.RequestComment3;
                    }
                }

                // Handle attachments - ปรับปรุงให้รองรับ other programs หลายตัว
                var otherProgramsIds = new List<Guid>();

                if (attachments != null && attachments.Any())
                {
                    // Get existing attachments for this request
                    var existingAttachments = await _context.Attachments
                        .Where(a => a.RequestId == updateRequest.Id && a.IsDeleted == false)
                        .ToListAsync();

                    // Process each attachment from the request
                    foreach (var attachment in attachments.Where(a => !string.IsNullOrWhiteSpace(a.AttachmentName)))
                    {
                        if (attachment.Id != null && attachment.Id != Guid.Empty)
                        {
                            // Update existing attachment
                            var existingAttachment = existingAttachments
                                .FirstOrDefault(ea => ea.Id == attachment.Id);

                            if (existingAttachment != null)
                            {
                                if (attachment.IsDeleted == true)
                                {
                                    // Soft delete attachment
                                    existingAttachment.IsDeleted = true;
                                }
                                else
                                {
                                    // Update existing attachment
                                    existingAttachment.AttachmentName = attachment.AttachmentName;
                                    existingAttachment.AttachementType = attachment.AttachementType ?? existingAttachment.AttachementType;
                                    existingAttachment.Category = attachment.Category ?? existingAttachment.Category;
                                    existingAttachment.AttachmentSize = attachment.AttachmentSize ?? existingAttachment.AttachmentSize;

                                    // Track other programs IDs
                                    if (attachment.Category?.ToLower() == "otherprograms")
                                    {
                                        otherProgramsIds.Add(existingAttachment.Id);
                                    }

                                    // Only update file data if new data is provided
                                    if (attachment.AttachementFileData != null)
                                        existingAttachment.AttachementFileData = attachment.AttachementFileData;

                                    // Only update path if new path is provided
                                    if (!string.IsNullOrWhiteSpace(attachment.AttachementPath))
                                        existingAttachment.AttachementPath = attachment.AttachementPath;

                                    if (!string.IsNullOrWhiteSpace(attachment.AttachmentFileLocation))
                                        existingAttachment.AttachmentFileLocation = attachment.AttachmentFileLocation;
                                }

                                _context.Attachments.Update(existingAttachment);
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(attachment.AttachementPath))
                        {
                            // Add new attachment only if it doesn't already exist
                            var isDuplicate = existingAttachments.Any(ea =>
                                ea.AttachmentName == attachment.AttachmentName &&
                                ea.Category == attachment.Category);

                            if (!isDuplicate)
                            {
                                var newAttachmentId = Guid.NewGuid();
                                var newAttachment = new Models.Attachment
                                {
                                    Id = newAttachmentId,
                                    AttachmentName = attachment.AttachmentName,
                                    AttachementPath = attachment.AttachementPath,
                                    AttachementType = attachment.AttachementType ?? "",
                                    UploadDate = DateTime.UtcNow,
                                    AttachementFileData = attachment.AttachementFileData,
                                    IsDeleted = false,
                                    RequestId = updateRequest.Id,
                                    Category = attachment.Category,
                                    AttachmentSize = !string.IsNullOrEmpty(attachment.AttachmentSize) ? attachment.AttachmentSize : null,
                                    AttachmentFileLocation = !string.IsNullOrEmpty(attachment.AttachmentFileLocation) ? attachment.AttachmentFileLocation : null
                                };

                                await _context.Attachments.AddAsync(newAttachment);

                                // Track other programs IDs
                                if (attachment.Category?.ToLower() == "otherprograms")
                                {
                                    otherProgramsIds.Add(newAttachmentId);
                                }
                            }
                        }
                    }
                }

                // Update OtherPrograms field with multiple IDs
                if (otherProgramsIds.Any())
                {
                    updateRequest.OtherPrograms = null;
                }

                // Handle lots - Complete Replace Strategy
                var updatedLots = new List<object>();
                if (lots != null && lots.Any())
                {
                    // Get existing lots for this request
                    var existingLots = await _context.LotRequests
                        .Where(l => l.RequestId == updateRequest.Id && l.IsDeleted == false)
                        .ToListAsync();

                    // Get lot numbers from request (clean data) - แค่ check ว่าไม่ว่าง
                    var requestLotNumbers = lots.Where(l => !string.IsNullOrWhiteSpace(l.LotNo))
                        .Select(l => l.LotNo.Trim())
                        .Distinct()
                        .ToList();

                    // Step 1: Soft delete all existing lots that are not in the new request
                    var lotsToDelete = existingLots.Where(el => !requestLotNumbers.Contains(el.LotNo)).ToList();
                    foreach (var lotToDelete in lotsToDelete)
                    {
                        lotToDelete.IsDeleted = true;
                        _context.LotRequests.Update(lotToDelete);
                    }

                    // Step 2: Process each lot from the request
                    foreach (var requestLotNumber in requestLotNumbers)
                    {
                        // Check if lot already exists
                        var existingLot = existingLots.FirstOrDefault(el => el.LotNo == requestLotNumber);

                        if (existingLot != null)
                        {
                            // Lot already exists, just add to response (no need to update)
                            updatedLots.Add(new { Id = existingLot.Id, LotNo = existingLot.LotNo });
                        }
                        else
                        {
                            // Add new lot
                            var lotId = Guid.NewGuid();
                            var newLot = new LotRequest
                            {
                                Id = lotId,
                                LotNo = requestLotNumber,
                                RequestId = updateRequest.Id,
                                IsDeleted = false
                            };

                            await _context.LotRequests.AddAsync(newLot);
                            updatedLots.Add(new { Id = lotId, LotNo = newLot.LotNo });
                        }
                    }
                }

                _context.InaRequests.Update(updateRequest);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Id = updateRequest.Id,
                    Lots = updatedLots,
                    OtherProgramsIds = otherProgramsIds,
                    Message = "Request updated successfully"
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        [HttpGet("GetRequestStatus/{id}")]
        public async Task<IActionResult> GetRequestStatus(Guid id)
        {
            try
            {
                var request = await _context.InaRequests
                    .Include(r => r.Status)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (request == null)
                    return NotFound("Request not found.");

                var statusINA = await _context.ListItems
                    .SingleOrDefaultAsync(x => x.ListItemCode == "LI02");

                if (statusINA == null)
                    return NotFound("Status type not found.");

                var allStatuses = await _context.Statuses
                    .Where(x => x.StatusTypeId == statusINA.Id)
                    .OrderBy(s => s.Ordinal)
                    .ToListAsync();

                var currentStatus = request.Status;
                if (currentStatus == null)
                    return NotFound("Current status not found.");

                // Filter statuses based on current status
                List<Status> filteredStatuses;
                if (currentStatus.StatusName.ToLower().Trim() == "finishtest")
                {
                    filteredStatuses = allStatuses
                        .Where(s => s.Ordinal < 5 || s.StatusName.ToLower().Trim() == "finishtest")
                        .OrderBy(s => s.Ordinal)
                        .ToList();
                }
                else
                {
                    filteredStatuses = allStatuses;
                }

                var statusSteps = filteredStatuses
                    .Select(s => new { s.Id, s.StatusName, s.Ordinal })
                    .ToList();

                int currentStep = statusSteps.FindIndex(s => s.Id == request.StatusId) + 1;

                return Ok(new
                {
                    statusSteps,
                    currentStep,
                    currentStatusName = currentStatus.StatusName,
                    currentOrdinal = currentStatus.Ordinal
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving request status: {ex.Message}");
            }
        }

        [HttpPost("FinishTest/{id}")]
        public async Task<IActionResult> FinishTest(Guid id)
        {
            try
            {
                var request = await _context.InaRequests
                    .Include(r => r.Status)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (request == null)
                    return NotFound("Request not found.");

                // Find "Finish" status
                var finishStatus = await _context.Statuses
                    .FirstOrDefaultAsync(s => s.StatusName == "Finish Test");

                if (finishStatus == null)
                    return BadRequest("Finish status not found.");

                request.StatusId = finishStatus.Id;
                request.RequestFinishDate = DateTime.UtcNow;

                _context.InaRequests.Update(request);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Test finished successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("GetLots/{requestId}")]
        public async Task<IActionResult> GetLots(Guid requestId)
        {
            try
            {
                var lots = await _context.LotRequests
                    .Where(l => l.RequestId == requestId && l.IsDeleted == false)
                    .Select(l => new
                    {
                        l.Id,
                        l.LotNo,
                        l.RequestId
                    })
                    .OrderBy(l => l.LotNo)
                    .ToListAsync();

                return Ok(lots);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving lots: {ex.Message}");
            }
        }

        [HttpDelete("DeleteLot/{id}")]
        public async Task<IActionResult> DeleteLot(Guid id)
        {
            try
            {
                var lot = await _context.LotRequests.FindAsync(id);
                if (lot == null)
                    return NotFound("Lot not found.");

                lot.IsDeleted = true;
                _context.LotRequests.Update(lot);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Lot deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting lot: {ex.Message}");
            }
        }
    }
}