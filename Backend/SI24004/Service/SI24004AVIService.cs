using Microsoft.EntityFrameworkCore;
using SI24004.Models;

namespace SI24004.Service
{
    public class SI24004AVIService
    {
        private readonly PostgrestContext _context;

        public SI24004AVIService(PostgrestContext context)
        {
            _context = context;
        }
        public async Task<string> GenerateRequestCode()
        {
            string prefix = "AVIR";
            string yearPart = DateTime.Now.ToString("yy"); // ปี ค.ศ. แบบ 2 หลัก เช่น 2025 -> 25

            // นับจำนวนคำร้องในปีปัจจุบัน
            int count = await _context.AviRequests
                .Where(r => r.RequestDate.HasValue && r.RequestDate.Value.Year == DateTime.Now.Year)
                .CountAsync();

            string sequence = (count + 1).ToString("D4"); // แปลงเป็น 4 หลัก เช่น 0001, 0002

            return $"{prefix}-{yearPart}-{sequence}";

        }

        public async Task<string> GenerateRequestCodeIna()
        {
            string prefix = "INA";
            string yearPart = DateTime.Now.ToString("yy"); // ปี ค.ศ. แบบ 2 หลัก เช่น 2025 -> 25

            // นับจำนวนคำร้องในปีปัจจุบัน
            int count = await _context.InaRequests
                .Where(r => r.RequestDate.HasValue && r.RequestDate.Value.Year == DateTime.Now.Year)
                .CountAsync();

            string sequence = (count + 1).ToString("D4"); // แปลงเป็น 4 หลัก เช่น 0001, 0002

            return $"{prefix}_{yearPart}-{sequence}";
        }


    }
}
