using Microsoft.EntityFrameworkCore;
using SI24004.Models.PostgreSQL;

namespace SI24004.Services
{
    public class SI25007Service
    {
        private readonly PostgrestContext _context;

        public SI25007Service(PostgrestContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateDrawingCode()
        {
            string prefix = "DR";
            string yearPart = DateTime.UtcNow.ToString("yy"); // ใช้ UTC เพื่อหลีกเลี่ยงปัญหาเรื่อง TimeZone

            // นับจำนวนคำร้องในปีปัจจุบัน
            int count = await _context.DwRequests
                .Where(r => r.CreatedDate.HasValue && r.CreatedDate.Value.Year == DateTime.UtcNow.Year) // ตรวจสอบปีใน UTC
                .CountAsync();

            string sequence = (count + 1).ToString("D4"); // แปลงเป็น 4 หลัก เช่น 0001, 0002

            return $"{prefix}-{yearPart}-{sequence}";
        }

    }
}
