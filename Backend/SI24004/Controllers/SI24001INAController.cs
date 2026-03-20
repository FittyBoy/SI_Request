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
                    
                    .OrderBy(x => x.RequestCode)
                    .ThenBy(x => x.RequestDate)
                    .ToListAsync();

                // Attachments table not in current schema - return empty
                List<Guid> requestIds = requests.Select(r => r.Id).ToList();

                // Get all lots for these requests
                var lots = await _context.LotRequests
                    .Where(l => requestIds.Contains(l.RequestId) && l.IsDeleted == false)
                    .OrderBy(l => l.LotNo)
                    .ToListAsync();

                // Group by RequestId
                var attachmentsByRequest = new Dictionary<Guid?, List<InaRequest>>();
                var lotsByRequest = lots.GroupBy(l => l.RequestId).ToDictionary(g => g.Key, g => g.ToList());

                var result = requests.Select(x => new
                {
                    Id = x.Id,
                    RequestCode = x.RequestCode,
                    // ?? RequestName ???
                    RequestPurpose = x.RequestPurpose, // ???? Purpose ?????????
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
                    Status = x.StatusId.HasValue ? new
                    {
                        Id = x.StatusId.Value,
                        StatusName = (string?)null,
                        Ordinal = (int?)null
                    } : null,

                    // ????? LotNumbers ????????????
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

                        // Other Programs - ?????????????
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
                    
                    .FirstOrDefaultAsync();

                if (inaRequest == null)
                    return NotFound(new { message = "Request not found" });

                // Attachments table not in current schema - return empty
                var attachments = new List<InaRequest>();

                // Get lots for this request
                var lots = await _context.LotRequests
                    .Where(l => l.RequestId == id && l.IsDeleted == false)
                    .OrderBy(l => l.LotNo)
                    .ToListAsync();

                var result = new
                {
                    Id = inaRequest.Id,
                    RequestCode = inaRequest.RequestCode,
                    // ?? RequestName ???
                    RequestPurpose = inaRequest.RequestPurpose, // ???? Purpose ?????????
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
                    Status = inaRequest.StatusId.HasValue ? new
                    {
                        Id = inaRequest.StatusId.Value,
                        StatusName = _context.Statuses.Where(s => s.Id == inaRequest.StatusId.Value).Select(s => s.StatusName).FirstOrDefault(),
                        Ordinal = (int?)null
                    } : null,

                    // ????? LotNumbers ????????????
                    LotNumbers = lots.Select(l => l.LotNo).ToList(),

                    // Attachments not available - table not in current schema
                    Attachments = new
                    {
                        Recipe = new List<object>(),
                        OtherPrograms = new List<object>(),
                        General = new List<object>()
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
                    [FromForm] List<string> otherPrograms) // ??????????? other programs array
        {
            // Validation
            if (requestDto == null)
                return BadRequest("Request data is required.");

            // ????? validate RequestName ??????????????
            if (string.IsNullOrWhiteSpace(requestDto.RequestPurpose))
                return BadRequest("Request purpose is required.");

            if (requestDto.UserId == Guid.Empty)
                return BadRequest("User ID is required.");

            // Validate lots - ??? validation ??? required ????
            if (lots == null || !lots.Any())
                return BadRequest("At least one lot is required.");

            if (lots.Count > 10)
                return BadRequest("Maximum 10 lots allowed per request.");

            // Validate lot numbers - ??? check ??????????
            var lotNumbers = lots.Where(l => !string.IsNullOrWhiteSpace(l.LotNo)).Select(l => l.LotNo.Trim()).ToList();
            if (lotNumbers.Count != lotNumbers.Distinct().Count())
                return BadRequest("Duplicate lot numbers are not allowed.");

            if (lotNumbers.Count == 0)
                return BadRequest("At least one valid lot number is required.");

            try
            {
                var newRequestId = requestDto.Id ?? Guid.NewGuid();

                // Get default status (first status alphabetically as fallback)
                var defaultStatus = await _context.Statuses
                    .OrderBy(s => s.StatusName)
                    .FirstOrDefaultAsync();

                if (defaultStatus == null)
                    return BadRequest("Default status not found.");

                // Validate user exists
                var userExists = await _context.Users.AnyAsync(u => u.Id == requestDto.UserId);
                if (!userExists)
                    return BadRequest("User not found.");

                Guid? firstAttachmentId = null;
                Guid? recipeAttachmentId = null;
                var otherProgramsAttachmentIds = new List<Guid>(); // ??????????? list

                // Handle attachments - ????????????????? other programs ???????
                if (attachments != null && attachments.Any())
                {
                    foreach (var attachment in attachments.Where(a => !string.IsNullOrWhiteSpace(a.AttachmentName) && !string.IsNullOrWhiteSpace(a.AttachementPath)))
                    {
                        var attachmentId = Guid.NewGuid();
                        var newAttachment = new SI24004.Models.PostgreSQL.Attachment
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

                        // Attachment not saved - Attachments table not in current schema

                        // Set specific attachment IDs based on category
                        if (attachment.Category?.ToLower() == "recipe")
                        {
                            recipeAttachmentId = attachmentId;
                        }
                        else if (attachment.Category?.ToLower() == "otherprograms")
                        {
                            otherProgramsAttachmentIds.Add(attachmentId); // ???? multiple IDs
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
                    // ?? RequestName ???
                    RequestPurpose = requestDto.RequestPurpose, // ???? Purpose ?????????
                    UserId = requestDto.UserId,
                    RequestDescription = requestDto.RequestDescription ?? "", // ????????? blank ???
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
                    // OtherPrograms ?????????? array ??? ?????????????? JSON string
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
                    OtherProgramsIds = otherProgramsAttachmentIds, // ??????? multiple IDs
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
            [FromForm] List<string> otherPrograms, // ??????????? other programs array
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
                    
                    .FirstOrDefaultAsync(r => r.Id == requestDto.Id);

                if (updateRequest == null)
                    return NotFound($"Request with Id {requestDto.Id} not found.");

                var currentStatus = updateRequest.StatusId.HasValue
                    ? await _context.Statuses.FindAsync(updateRequest.StatusId.Value)
                    : null;

                // Update Status only if approved
                if (isApproved && currentStatus != null)
                {
                    var allStatuses = await _context.Statuses.OrderBy(s => s.StatusName).ToListAsync();
                    var currentIndex = allStatuses.FindIndex(s => s.Id == currentStatus.Id);
                    var nextStatus = (currentIndex >= 0 && currentIndex < allStatuses.Count - 1)
                        ? allStatuses[currentIndex + 1]
                        : null;

                    if (nextStatus != null)
                    {
                        updateRequest.StatusId = nextStatus.Id;
                    }
                }
                else
                {
                    updateRequest.StatusId = requestDto.StatusId;
                }

                // Update Request data - ?? RequestName ?????? Purpose ???
                updateRequest.RequestPurpose = requestDto.RequestPurpose ?? updateRequest.RequestPurpose;
                updateRequest.UserId = requestDto.UserId != Guid.Empty ? requestDto.UserId : updateRequest.UserId;
                updateRequest.RequestDescription = requestDto.RequestDescription ?? updateRequest.RequestDescription; // ????????? blank
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

                // Handle Comments - update all regardless of ordinal (ordinal not in DB)
                updateRequest.RequestComment1 = requestDto.RequestComment1 ?? updateRequest.RequestComment1;
                updateRequest.RequestInstallDate = requestDto.RequestInstallDate ?? updateRequest.RequestInstallDate;
                updateRequest.RequestComment2 = requestDto.RequestComment2 ?? updateRequest.RequestComment2;
                updateRequest.RequestClearDate = requestDto.RequestClearDate ?? updateRequest.RequestClearDate;
                updateRequest.RequestComment3 = requestDto.RequestComment3 ?? updateRequest.RequestComment3;

                // Handle attachments - ????????????????? other programs ???????
                var otherProgramsIds = new List<Guid>();

                // Attachments table not in current schema - attachment processing skipped

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

                    // Get lot numbers from request (clean data) - ??? check ??????????
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
                    
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (request == null)
                    return NotFound("Request not found.");

                var allStatuses = await _context.Statuses
                    .OrderBy(s => s.StatusName)
                    .ToListAsync();

                var currentStatus = request.StatusId.HasValue
                    ? await _context.Statuses.FindAsync(request.StatusId.Value)
                    : null;
                if (currentStatus == null)
                    return NotFound("Current status not found.");

                // Filter statuses based on current status
                List<Status> filteredStatuses;
                if (currentStatus.StatusName?.ToLower().Trim() == "finishtest")
                {
                    filteredStatuses = allStatuses
                        .Where(s => s.StatusName?.ToLower().Trim() == "finishtest")
                        .ToList();
                }
                else
                {
                    filteredStatuses = allStatuses;
                }

                var statusSteps = filteredStatuses
                    .Select((s, i) => new { s.Id, s.StatusName, Ordinal = i + 1 })
                    .ToList();

                int currentStep = statusSteps.FindIndex(s => s.Id == request.StatusId) + 1;

                return Ok(new
                {
                    statusSteps,
                    currentStep,
                    currentStatusName = currentStatus.StatusName,
                    currentOrdinal = currentStep
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
