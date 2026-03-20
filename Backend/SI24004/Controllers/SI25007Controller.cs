using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using SI24004.Models.PostgreSQL;
using SI24004.Models.DTOs;
using SI24004.Services;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using static System.Collections.Specialized.BitVector32;

namespace SI24004.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SI25007Controller : ControllerBase
    {
        private readonly PostgrestContext _context;
        private readonly SI25007Service _service;
        private readonly SmtpSettings _smtpSettings;
        public SI25007Controller(PostgrestContext context, SI25007Service service, IOptions<SmtpSettings> smtpSettings)
        {
            _context = context;
            _service = service;
            _smtpSettings = smtpSettings.Value; // Extract the value from IOptions
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DwRequest>>> GetDwRequests()
        {
            var inaRequests = await _context.DwRequests
                //.Where(x => x.IsDelete != true) // ?????????? IsDelete ???? IsDeleted ????????? Model
                .Include(x => x.Attachment)
                .OrderBy(x => x.UpdateDate)
                .ThenBy(x => x.RequestCode)
                    .Select(x => new DwRequest
                    {
                        Id = x.Id,
                        RequestCode = x.RequestCode,
                        DrawingCode = x.DrawingCode, // ????? DrawingCode ????????? Model
                        DrawingName = x.DrawingName, // ?????????? RequestName ???? DrawingName
                        DrawingDescription = x.DrawingDescription, // ?????????? RequestDescription
                        SectionId = x.SectionId,
                        DrawingTypeId = x.DrawingTypeId,
                        StatusId = x.StatusId,
                        CreatedDate = x.CreatedDate,
                        CreatedBy = x.CreatedBy,
                        UpdateDate = x.UpdateDate,
                        UpdateBy = x.UpdateBy,
                        AttachmentId = x.AttachmentId,
                        Active = x.Active,
                        IsDelete = x.IsDelete,
                        UserId = x.UserId,

                        // ? ???????? Map ????????? Status
                        Status = x.Status != null
                            ? new Status
                            {
                                Id = x.Status.Id,
                                StatusName = x.Status.StatusName,
                                Ordinal = x.Status.Ordinal
                            }
                            : null, // ?????????? null
                        Section = x.Section != null ? new SI24004.Models.PostgreSQL.Section
                        {
                            Id = x.Section.Id,
                            SectionCode = x.Section.SectionCode,
                            SectionName = x.Section.SectionName
                        } : null,
                        DrawingType = x.DrawingType != null ? new Drawing
                        {
                            Id = x.DrawingType.Id,
                            DrawingCode = x.DrawingCode,
                            DrawingName = x.DrawingName,
                        } : null,
                        // ? ???????? Map ????????? Attachment
                        Attachment = x.Attachment != null
                            ? new SI24004.Models.PostgreSQL.Attachment
                            {
                                Id = x.Attachment.Id,
                                AttachmentName = x.Attachment.AttachmentName,
                                AttachementPath = x.Attachment.AttachementPath,
                                AttachementType = x.Attachment.AttachementType,
                            }
                            : null // ?????????? null
                    }).ToListAsync();
            return Ok(inaRequests);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDwRequest(Guid id)
        {
            var DwRequest = await _context.DwRequests.FindAsync(id);
            if (DwRequest == null)
            {
                return NotFound(new { message = "Request not found" });
            }

            DwRequest.IsDelete = true;
            DwRequest.Active = false;
            DwRequest.StatusId = Guid.Parse("c18ca7b2-e69c-4375-b9ba-372323ef0fce");
            _context.Entry(DwRequest).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Request has been soft deleted successfully" });
        }

        [HttpGet("ReportDwRequest/{id}")]
        public async Task<IActionResult> ReportDwRequest(Guid id)
        {
            var DwRequest = await _context.DwRequests.FindAsync(id);
            if (DwRequest == null)
            {
                return NotFound(new { message = "Request not found" });
            }
            DwRequest.StatusId = Guid.Parse("7743968d-d0b4-4097-9cd0-498417b8b6d0");
            DwRequest.IsDelete = false;
            DwRequest.Active = false;
            _context.Entry(DwRequest).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Request has been soft deleted successfully" });
        }

        public class StatusStepDto
        {
            public Guid Id { get; set; }
            public string StatusName { get; set; }
            public int Ordinal { get; set; }
        }

        [HttpGet("GetRequestStatus/{id}")]
        public async Task<IActionResult> GetRequestStatus(Guid id)
        {
            var request = await _context.DwRequests.FindAsync(id);
            if (request == null)
                return NotFound("Request not found.");

            var statusDw = await _context.ListItems
                .Where(x => x.ListItemCode == "LI04")
                .FirstOrDefaultAsync();

            if (statusDw == null)
                return NotFound("Status type not found.");

            var statusSteps = await _context.Statuses
                .Where(x => x.StatusTypeId == statusDw.Id && x.Ordinal > 0)
                .OrderBy(s => s.Ordinal)
                .Select(s => new StatusStepDto
                {
                    Id = s.Id,
                    StatusName = s.StatusName,
                    Ordinal = s.Ordinal.Value
                })
                .ToListAsync();

            Guid reviseId = Guid.Parse("54415422-e4a1-4308-8a8f-bcfb2723ae3f");

            if (statusSteps.Any())
            {
                // ???????????????? Ordinal
                statusSteps = statusSteps
                    .GroupBy(s => s.Ordinal)
                    .Select(g => g.First())
                    .ToList();

                // ??????? Revise
                if (request.StatusId == reviseId || request.DrawingRevise == true)
                {
                    // ?? Revise ??? Request ?????
                    statusSteps.RemoveAll(s => s.Id == reviseId || s.Ordinal == 1);

                    // ????? Revise ????????????
                    statusSteps.Insert(0, new StatusStepDto
                    {
                        Id = reviseId,
                        StatusName = "Revise",
                        Ordinal = 0
                    });
                }
                else
                {
                    // ??????????? Ordinal == 1 ???? "Request"
                    foreach (var step in statusSteps)
                    {
                        if (step.Ordinal == 1)
                            step.StatusName = "Request";
                    }
                }
            }


            int currentStep = statusSteps.FindIndex(s => s.Id == request.StatusId) + 1;

            return Ok(new
            {
                statusSteps = statusSteps.Select(s => s.StatusName),
                currentStep
            });

        }

        // ? ??????????????????????
        private async Task SendEmailToApprovers(DwRequest newRequest, string requesterName, string sectionName)
        {
            try
            {
                var RolesAdmin = await _context.Roles
                    .Where(x => x.RoleName.ToLower().Trim() == "admin")
                    .FirstOrDefaultAsync();

                if (RolesAdmin == null)
                {
                    Console.WriteLine("Admin role not found.");
                    return;
                }

                // ????? approvers ??????? admin ????? section_id ????????? user
                var approvers = await _context.Users
                    .Where(u => u.RoleId == RolesAdmin.Id && u.SectionId == newRequest.SectionId)
                    .ToListAsync();

                if (!approvers.Any())
                {
                    Console.WriteLine("No approvers found for this section.");
                    return;
                }

                // ? Skip SSL certificate validation for internal servers
                ServicePointManager.ServerCertificateValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) => true;

                // ????? SMTP Client
                var smtpClient = new SmtpClient(_smtpSettings.Host)
                {
                    Port = _smtpSettings.Port,
                    EnableSsl = _smtpSettings.EnableSsl,
                };

                // ??????????????? authentication ???????
                if (_smtpSettings.UseDefaultCredentials)
                {
                    smtpClient.UseDefaultCredentials = true;
                }
                else if (!string.IsNullOrEmpty(_smtpSettings.Username) && !string.IsNullOrEmpty(_smtpSettings.Password))
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);
                }
                else
                {
                    smtpClient.UseDefaultCredentials = false;
                }

                // ?????????????? approver
                foreach (var approver in approvers)
                {
                    if (string.IsNullOrEmpty(approver.UserName))
                        continue;

                    var systemUrl = "http://172.18.106.100:9014";

                    // ????? email format: user_name.user_lastname@agc.com
                    string emailAddress = GenerateEmailAddress(approver.UserName, approver.UserLastname);

                    if (string.IsNullOrEmpty(emailAddress))
                    {
                        Console.WriteLine($"Cannot generate email address for approver: {approver.UserName}");
                        continue;
                    }

                    var subject = $"New Drawing Request - {newRequest.RequestCode}";

                    var body = $@"
                            <!DOCTYPE html>
                            <html>
                            <head>
                                <style>
                                    body {{ font-family: Arial, sans-serif; margin: 20px; }}
                                    .header {{ background-color: #f0f0f0; padding: 15px; border-radius: 5px; }}
                                    .content {{ margin: 20px 0; }}
                                    .details {{ background-color: #f9f9f9; padding: 15px; border-left: 4px solid #007bff; }}
                                    .footer {{ margin-top: 20px; font-size: 12px; color: #666; }}
                                </style>
                            </head>
                            <body>
                                <div class='header'>
                                    <h2>New Drawing Request Notification</h2>
                                </div>

                                <div class='content'>
                                    <p>?????? {approver.UserName} {approver.UserLastname},</p>
                                    <p>??????????????????????????????????????</p>

                                    <div class='details'>
                                        <h3>??????????????:</h3>
                                        <p><strong>Request Code:</strong> {newRequest.RequestCode}</p>
                                        <p><strong>Drawing Code:</strong> {newRequest.DrawingCode}</p>
                                        <p><strong>Drawing Name:</strong> {newRequest.DrawingName}</p>
                                        <p><strong>Description:</strong> {newRequest.DrawingDescription}</p>
                                        <p><strong>Section:</strong> {sectionName}</p>
                                        <p><strong>Requested by:</strong> {requesterName}</p>
                                        <p><strong>Created Date:</strong> {newRequest.CreatedDate:dd/MM/yyyy HH:mm}</p>
                                        <p><strong>Created By:</strong> {newRequest.CreatedBy}</p>
                                    </div>

                                    <p>???????????????????????????????????</p>
                                    <p><a href=""{systemUrl}/registerdac"" style=""color: #007bff; text-decoration: none;"">??????????????????????????</a></p>
                                </div>

                                <div class='footer'>
                                    <p>This is an automated message. Please do not reply to this email.</p>
                                    <p>??????????????????????????????????? ????????????????????????</p>
                                </div>
                            </body>
                            </html>";

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true,
                    };

                    mailMessage.To.Add(emailAddress);

                    await smtpClient.SendMailAsync(mailMessage);

                    Console.WriteLine($"Email sent successfully to {emailAddress} for Request Code: {newRequest.RequestCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email notification: {ex.Message}");
                // Log inner exception for more details
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddRequest([FromForm] DwRequestDto requestDto, [FromForm] IFormFile? attachmentFile)
        {
            if (requestDto == null)
                return BadRequest("Request data is required.");

            try
            {
                var newRequestId = requestDto.Id ?? Guid.NewGuid();
                var statusId = requestDto.StatusId ?? Guid.Parse("64996b06-87cd-460d-a31e-92b985c976fb");
                Guid? attachmentId = null;

                // ? ???????????? DrawingCode ??????????
                if (!string.IsNullOrEmpty(requestDto.DrawingCode) && await IsDuplicateDrawingCode(requestDto.DrawingCode, newRequestId))
                {
                    return BadRequest(new { message = "Duplicate DrawingCode. Please use a unique DrawingCode." });
                }

                // ? ???????????? ??????????? SMB ????????????
                if (attachmentFile != null)
                {
                    attachmentId = Guid.NewGuid();
                    string fileExtension = Path.GetExtension(attachmentFile.FileName);
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(attachmentFile.FileName);
                    string fileName = $"{fileNameWithoutExtension}{fileExtension}"; // ? ??????????????

                    string smbPath = @"\\172.18.106.100\d\TEST";

                    // ? ???????????????????????????????
                    if (!Directory.Exists(smbPath))
                        return StatusCode(500, "SMB path not found.");

                    string fullPath = Path.Combine(smbPath, fileName);

                    // ? ??? FileStream + await using
                    await using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await attachmentFile.CopyToAsync(stream);
                    }

                    // ? ???????????? byte[] ????????????????????
                    byte[] fileData;
                    await using (var memoryStream = new MemoryStream())
                    {
                        await attachmentFile.CopyToAsync(memoryStream);
                        fileData = memoryStream.ToArray();
                    }

                    // ? ?????????????????????
                    var newAttachment = new SI24004.Models.PostgreSQL.Attachment
                    {
                        Id = attachmentId.Value,
                        AttachmentName = fileName, // ? ???????????????????????
                        AttachementPath = fullPath,
                        AttachementType = fileExtension,
                        UploadDate = DateTime.UtcNow,
                        AttachementFileData = fileData,
                        IsDeleted = false
                    };

                    await _context.Attachments.AddAsync(newAttachment);
                }

                var UserName = _context.Users.Where(x => x.Id == requestDto.UserId).FirstOrDefault();

                // ? ????????????????
                var newRequest = new DwRequest
                {
                    Id = newRequestId,
                    DrawingCode = requestDto.DrawingCode,
                    RequestCode = requestDto.RequestCode ?? await _service.GenerateDrawingCode(), // ??? RequestCode ?????????????????????
                    DrawingName = requestDto.DrawingName,
                    SectionId = requestDto.SectionId,
                    DrawingTypeId = requestDto.DrawingTypeId,
                    StatusId = statusId, // ??????????????????????
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = UserName.UserName,
                    UpdateDate = DateTime.UtcNow,
                    UpdateBy = UserName.UserName,
                    DrawingDescription = requestDto.DrawingDescription,
                    UserId = requestDto.UserId,
                    AttachmentId = attachmentId,
                    Active = requestDto.Active,
                    IsDelete = requestDto.IsDeleted ?? false,
                    DrawingRevise = false
                };

                await _context.DwRequests.AddAsync(newRequest);
                await _context.SaveChangesAsync();

                // ? ?????????????????????????????????
                try
                {
                    // ????????? Section ??????????????
                    var section = await _context.Sections
                        .FirstOrDefaultAsync(s => s.Id == requestDto.SectionId);

                    string sectionName = section?.SectionName ?? "Unknown Section";

                    // ????????? approvers
                    await SendEmailToApprovers(newRequest, UserName.UserName, sectionName);
                }
                catch (Exception emailEx)
                {
                    // Log email error ?????????????????????????????? request
                    Console.WriteLine($"Email notification failed: {emailEx.Message}");
                }

                return Ok(new { Id = newRequest.Id, AttachmentId = attachmentId, Message = "Request added successfully" });
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

        [HttpPut("UpdateData")]
        public async Task<IActionResult> UpdateDrawingRequest([FromForm] DwRequestDto requestDto)
        {
            if (requestDto == null || requestDto.Id == Guid.Empty)
                return BadRequest("Invalid request data.");

            try
            {
                // ??????????????????????????
                var existingRequest = await _context.DwRequests
                    .FirstOrDefaultAsync(x => x.Id == requestDto.Id);

                if (existingRequest == null)
                    return NotFound("Request not found.");

                // ???????????
                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.Id == requestDto.UserId);

                if (user == null)
                    return NotFound("User not found.");

                // ????????????
                existingRequest.DrawingCode = requestDto.DrawingCode ?? existingRequest.DrawingCode;
                existingRequest.RequestCode = requestDto.RequestCode ?? existingRequest.RequestCode ?? await _service.GenerateDrawingCode();
                existingRequest.DrawingName = requestDto.DrawingName ?? existingRequest.DrawingName;
                existingRequest.SectionId = requestDto.SectionId;
                existingRequest.DrawingTypeId = requestDto.DrawingTypeId;
                existingRequest.UpdateDate = DateTime.UtcNow;
                existingRequest.UpdateBy = user.UserName; // ?????????????????????????
                existingRequest.DrawingDescription = requestDto.DrawingDescription ?? existingRequest.DrawingDescription;
                existingRequest.Active = requestDto.Active;
                existingRequest.IsDelete = requestDto.IsDeleted ?? existingRequest.IsDelete;
                existingRequest.DrawingRevise = false; // ????????

                if (requestDto.StatusId == Guid.Parse("c18ca7b2-e69c-4375-b9ba-372323ef0fce"))
                {
                    existingRequest.StatusId = Guid.Parse("54415422-e4a1-4308-8a8f-bcfb2723ae3f");
                    existingRequest.Active = true;
                    existingRequest.IsDelete = false;
                    existingRequest.DrawingRevise = true; // ????????
                }
                // ????????????????????
                _context.DwRequests.Update(existingRequest);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Update successful", updatedRequest = existingRequest });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Update Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"?? Inner Exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, "An error occurred while updating the request.");
            }
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFileData(string fileName)
        {
            try
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(fileName))
                    return BadRequest("File name is required");

                // Sanitize filename to prevent path traversal attacks
                fileName = Path.GetFileName(fileName);

                var pdfPath = Path.Combine("\\\\172.18.106.100\\d\\TEST", fileName);

                // Check if PDF file exists
                if (!System.IO.File.Exists(pdfPath))
                    return NotFound($"PDF file not found: {fileName}");

                var stampPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "stamp.png");

                // Check if stamp image exists
                if (!System.IO.File.Exists(stampPath))
                    return NotFound($"Stamp image not found at: {stampPath}");

                using var stream = new MemoryStream();

                // Open PDF in Import mode to avoid security issues
                using var pdf = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import);
                using var outputDocument = new PdfDocument();

                // Load image once before the loop for better performance
                using var image = XImage.FromFile(stampPath);

                // Process each page
                foreach (PdfPage page in pdf.Pages)
                {
                    var newPage = outputDocument.AddPage(page);
                    using var gfx = XGraphics.FromPdfPage(newPage);

                    // Configure stamp size (adjusted for header alignment)
                    double imageWidth = 150;  // Slightly smaller to fit with header
                    double imageHeight = 150;  // Rectangular to match header proportion

                    // Alternative: Use actual image dimensions
                    // double imageWidth = image.PixelWidth * 72.0 / image.HorizontalResolution;
                    // double imageHeight = image.PixelHeight * 72.0 / image.VerticalResolution;

                    // Position stamp at top-left corner
                    // ?????????????
                    double y = 50;
                    // ?????????????????????  
                    double x = newPage.Width - imageWidth - 50;
                    // Save current graphics state
                    var state = gfx.Save();

                    // Move to the position where we want to draw
                    gfx.TranslateTransform(x + imageWidth / 2, y + imageHeight / 2);

                    // Rotate 90 degrees clockwise
                    gfx.RotateTransform(90);  // Changed from -90 to 90 degrees

                    // Draw image centered at the transformed origin
                    gfx.DrawImage(image, -imageWidth / 2, -imageHeight / 2, imageWidth, imageHeight);

                    // Restore graphics state
                    gfx.Restore(state);
                }

                // Save PDF to stream
                outputDocument.Save(stream, false);
                stream.Position = 0;

                // Return as file download
                var result = File(stream.ToArray(), "application/pdf", fileName);
                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle network/file access issues
                return StatusCode(403, "Access denied to the requested file");
            }
            catch (DirectoryNotFoundException ex)
            {
                return NotFound("The specified directory was not found");
            }
            catch (FileNotFoundException ex)
            {
                return NotFound($"File not found: {ex.FileName}");
            }
            catch (Exception ex)
            {
                // Log error for debugging (uncomment if logger is available)
                // _logger.LogError(ex, "Error adding stamp to PDF: {fileName}", fileName);

                return StatusCode(500, $"Error processing PDF: {ex.Message}");
            }
        }

        [HttpGet("pdf/{fileName}")]
        public IActionResult GetPdf(string fileName)
        {
            try
            {
                fileName = Path.GetFileName(fileName); // ??????? path traversal
                string filePath = $@"\\172.18.106.100\d\TEST\{fileName}";

                if (!System.IO.File.Exists(filePath))
                    return NotFound("File not found");

                var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                string safeFileName = WebUtility.UrlEncode(fileName).Replace("+", "%20");

                // ? Header ?????? iframe
                Response.Headers["Content-Disposition"] = $"inline; filename*=UTF-8''{safeFileName}";
                Response.Headers["X-Content-Type-Options"] = "nosniff";
                Response.Headers["X-Frame-Options"] = "ALLOWALL";

                return File(stream, "application/pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }

        private async Task<bool> IsDuplicateDrawingCode(string drawingCode, Guid excludeId)
        {
            return await _context.DwRequests.AnyAsync(x => x.DrawingCode == drawingCode && x.Id != excludeId);
        }
    
        [HttpPut("ApprovedData")]
                public async Task<IActionResult> ApprovedDrawingRequest([FromBody] DwRequestDto requestDto)
                {
                    if (requestDto == null || requestDto.Id == Guid.Empty || requestDto.UserId == Guid.Empty)
                        return BadRequest("Invalid request data.");

                    try
                    {
                        var existingRequest = await _context.DwRequests
                            .Include(x => x.Status)
                            .FirstOrDefaultAsync(x => x.Id == requestDto.Id);

                        if (existingRequest == null)
                            return NotFound("Request not found.");

                        var user = await _context.Users
                            .FirstOrDefaultAsync(x => x.Id == requestDto.UserId);

                        if (user == null)
                            return NotFound("User not found.");

                        var currentStatus = await _context.Statuses.FindAsync(existingRequest.StatusId);
                        if (currentStatus == null)
                            return BadRequest("Invalid status ID.");

                        int nextStatusOrdinal = currentStatus.Ordinal.HasValue ? currentStatus.Ordinal.Value + 1 : 1;
                        Guid nextStatusId;
                        string nextStatusName = "";

                        if (nextStatusOrdinal > 3)
                        {
                            // ??????????????????????????? StatusId ???????????
                            nextStatusId = Guid.Parse("54415422-e4a1-4308-8a8f-bcfb2723ae3f");
                            nextStatusName = "Completed";
                        }
                        else
                        {
                            var nextStatus = await _context.Statuses
                                .Where(x => x.StatusTypeId == currentStatus.StatusTypeId)
                                .FirstOrDefaultAsync(s => s.Ordinal == nextStatusOrdinal);

                            if (nextStatus == null)
                                return BadRequest("Next status not found.");

                            nextStatusId = nextStatus.Id;
                            nextStatusName = nextStatus.StatusName;
                        }

                        if (existingRequest.StatusId == nextStatusId)
                            return BadRequest("The status is already at the next step.");

                        existingRequest.StatusId = nextStatusId;
                        existingRequest.UpdateDate = DateTime.UtcNow;
                        existingRequest.UpdateBy = user.UserName;

                        _context.DwRequests.Update(existingRequest);
                        await _context.SaveChangesAsync();

                        // ?????????????????
                        await SendApprovalNotificationEmail(existingRequest);
                        await SendEmailToApprovers(existingRequest, user.UserName, nextStatusName);

                return Ok(new { message = "Update successful", updatedRequest = existingRequest });
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, "An error occurred while updating the request.");
                    }
                }

                private async Task SendApprovalNotificationEmail(DwRequest request)
                {
                    try
                    {
                        var creator = await _context.Users
                            .FirstOrDefaultAsync(x => x.Id == request.UserId);

                        if (creator == null)
                            return;

                        // ????? email format: user_name.user_lastname@agc.com
                        string emailAddress = GenerateEmailAddress(creator.UserName, creator.UserLastname);

                        if (string.IsNullOrEmpty(emailAddress))
                        {
                            Console.WriteLine($"Cannot generate email address for user ID: {request.UserId}");
                            return;
                        }

                        var currentStatus = await _context.Statuses
                            .FirstOrDefaultAsync(x => x.Id == request.StatusId);

                        string statusName = currentStatus?.StatusName ?? "Unknown";

                        // ? Skip SSL certificate validation for internal servers
                        ServicePointManager.ServerCertificateValidationCallback =
                            (sender, certificate, chain, sslPolicyErrors) => true;

                        // ????? SMTP Client
                        var smtpClient = new SmtpClient(_smtpSettings.Host)
                        {
                            Port = _smtpSettings.Port,
                            EnableSsl = _smtpSettings.EnableSsl,
                        };

                        // ??????????????? authentication ???????
                        if (_smtpSettings.UseDefaultCredentials)
                        {
                            smtpClient.UseDefaultCredentials = true;
                        }
                        else if (!string.IsNullOrEmpty(_smtpSettings.Username) && !string.IsNullOrEmpty(_smtpSettings.Password))
                        {
                            smtpClient.UseDefaultCredentials = false;
                            smtpClient.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);
                        }
                        else
                        {
                            smtpClient.UseDefaultCredentials = false;
                        }

                        var subject = $"Drawing Request Approved - {request.DrawingCode}";
                        var body = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <style>
                                body {{ font-family: Arial, sans-serif; margin: 20px; }}
                                .header {{ background-color: #f0f0f0; padding: 15px; border-radius: 5px; }}
                                .content {{ margin: 20px 0; }}
                                .details {{ background-color: #f9f9f9; padding: 15px; border-left: 4px solid #007bff; }}
                                .footer {{ margin-top: 20px; font-size: 12px; color: #666; }}
                            </style>
                        </head>
                        <body>
                            <div class='header'>
                                <h2>Drawing Request Approved</h2>
                            </div>

                            <div class='content'>
                                <p>?????? {creator.UserName} {creator.UserLastname},</p>
                                <p>?????????????????????????????????????</p>

                                <div class='details'>
                                    <h3>??????????????:</h3>
                                    <p><strong>Drawing Code:</strong> {request.DrawingCode}</p>
                                    <p><strong>Request Code:</strong> {request.RequestCode}</p>
                                    <p><strong>Drawing Name:</strong> {request.DrawingName}</p>
                                    <p><strong>Created By:</strong> {request.CreatedBy}</p>
                                    <p><strong>Created Date:</strong> {request.CreatedDate:dd/MM/yyyy HH:mm}</p>
                                    <p><strong>Current Status:</strong> {statusName}</p>
                                    <p><strong>Updated Date:</strong> {request.UpdateDate:dd/MM/yyyy HH:mm}</p>
                                    <p><strong>Updated By:</strong> {request.UpdateBy}</p>
                                </div>

                                <p>???????????????????????????????????????????</p>
                            </div>

                            <div class='footer'>
                                <p>This is an automated message. Please do not reply to this email.</p>
                                <p>??????????????????????????????????? ????????????????????????</p>
                            </div>
                        </body>
                        </html>";

                        var mailMessage = new MailMessage
                        {
                            From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName),
                            Subject = subject,
                            Body = body,
                            IsBodyHtml = true,
                        };

                        mailMessage.To.Add(emailAddress);

                        await smtpClient.SendMailAsync(mailMessage);

                        Console.WriteLine($"Email sent successfully to {emailAddress} for Drawing Code: {request.DrawingCode}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send email notification: {ex.Message}");
                        // Log inner exception for more details
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                        }
                    }
                }

                // Helper method ??????????? email address
                private string GenerateEmailAddress(string userName, string lastName)
                {
                    if (string.IsNullOrWhiteSpace(userName))
                        return string.Empty;

                    // ??????????????? (??????????, ???????????)
                    userName = CleanEmailPart(userName);

                    if (string.IsNullOrWhiteSpace(lastName))
                    {
                        // ??????????????? ?????????????
                        return $"{userName}@agc.com";
                    }

                    lastName = CleanEmailPart(lastName);
                    return $"{userName}.{lastName}@agc.com";
                }

                // Helper method ???????????????????????? email
                private string CleanEmailPart(string input)
                {
                    if (string.IsNullOrWhiteSpace(input))
                        return string.Empty;

                    // ??????????, ???????????????????????, ?????????????
                    return input.Trim()
                                .ToLower()
                                .Replace(" ", "")
                                .Replace("-", "")
                                .Replace("_", "");
                }




    }

}
