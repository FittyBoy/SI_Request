using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SI24004.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SI24004.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SI25024Controller : ControllerBase
    {
        private readonly sqlServerContext _sqlcontext;

        // เพิ่ม Constructor สำหรับ Dependency Injection
        public SI25024Controller(sqlServerContext sqlcontext)
        {
            _sqlcontext = sqlcontext;
        }

        [HttpGet("chart-data")]
        public async Task<IActionResult> GetChartData()
        {
            try
            {
                // กำหนดช่วงวันที่สำหรับปีปัจจุบันเท่านั้น
                var currentYear = DateTime.Now.Year;
                var startDate = new DateTime(currentYear, 1, 1);
                var endDate = new DateTime(currentYear, 12, 31);

                // ดึงข้อมูลทั้งหมดจากตาราง ThRecord ที่มี status เป็น OK หรือ rescreen
                // และมีวันที่อยู่ในปีปัจจุบัน
                var thRecords = await _sqlcontext.ThRecords
                    .Where(x => (x.Status == "OK" || x.Status == "rescreen") &&
                               x.DateProcess >= startDate && x.DateProcess <= endDate)
                    .ToListAsync();

                var results = new List<ChartDataResult>();

                // กรุ๊ปข้อมูลตาม product type
                var groupedData = thRecords.GroupBy(x => x.ImobileSize);

                foreach (var group in groupedData)
                {
                    var productType = group.Key;
                    var records = group.ToList();

                    // กำหนด USL และ LSL ตาม product type
                    var (usl, lsl) = GetSpecLimits(productType);

                    // คำนวณค่าเฉลี่ยจากคอลัมน์ทั้งหมด
                    var averageValue = CalculateAverage(records);

                    // คำนวณ Standard Deviation
                    var standardDeviation = CalculateStandardDeviation(records, averageValue);

                    // ป้องกันการหารด้วย 0
                    if (standardDeviation == 0)
                    {
                        results.Add(new ChartDataResult
                        {
                            ProductSize = productType,
                            USL = usl,
                            LSL = lsl,
                            Average = averageValue,
                            StandardDeviation = standardDeviation,
                            CPU = 0,
                            CPL = 0,
                            CPK = 0
                        });
                        continue;
                    }

                    // คำนวณ CPU
                    var cpu = (usl - averageValue) / (3 * standardDeviation);

                    // คำนวณ CPL
                    var cpl = (averageValue - lsl) / (3 * standardDeviation);

                    // คำนวณ CPK (เลือกค่าที่น้อยกว่า)
                    var cpk = Math.Min(cpu, cpl);

                    results.Add(new ChartDataResult
                    {
                        ProductSize = productType,
                        USL = usl,
                        LSL = lsl,
                        Average = averageValue,
                        StandardDeviation = standardDeviation,
                        CPU = cpu,
                        CPL = cpl,
                        CPK = cpk
                    });
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                // ส่งข้อมูล error แบบละเอียดมากขึ้นสำหรับ development
                return StatusCode(500, new
                {
                    message = "Internal server error",
                    error = ex.Message,
                    stackTrace = ex.StackTrace // เอาออกใน production
                });
            }
        }

        private (double USL, double LSL) GetSpecLimits(string productType)
        {
            // เพิ่มการตรวจสอบ null หรือ empty
            if (string.IsNullOrEmpty(productType))
                return (0, 0);

            return productType switch
            {
                "76x76x0.225" => (0.240, 0.210),
                "76x76x0.2" => (0.215, 0.185),
                "76x76x0.14" => (0.157, 0.127),
                _ => (0, 0) // default case
            };
        }

        private double CalculateAverage(List<ThRecord> records)
        {
            if (records == null || records.Count == 0)
                return 0;

            var allValues = new List<double>();

            foreach (var record in records)
            {
                // เพิ่มค่าจากทุกคอลัมน์ที่ระบุ
                var stringValues = new string[]
                {
                    record.Ca1In1, record.Ca1In2, record.Ca1In3, record.Ca1In4, record.Ca1In5,
                    record.Ca1Out1, record.Ca1Out2, record.Ca1Out3, record.Ca1Out4, record.Ca1Out5,
                    record.Ca2In1, record.Ca2In2, record.Ca2In3, record.Ca2In4, record.Ca2In5,
                    record.Ca2Out1, record.Ca2Out2, record.Ca2Out3, record.Ca2Out4, record.Ca2Out5,
                    record.Ca3In1, record.Ca3In2, record.Ca3In3, record.Ca3In4, record.Ca3In5,
                    record.Ca3Out1, record.Ca3Out2, record.Ca3Out3, record.Ca3Out4, record.Ca3Out5,
                    record.Ca4In1, record.Ca4In2, record.Ca4In3, record.Ca4In4, record.Ca4In5,
                    record.Ca4Out1, record.Ca4Out2, record.Ca4Out3, record.Ca4Out4, record.Ca4Out5,
                    record.Ca5In1, record.Ca5In2, record.Ca5In3, record.Ca5In4, record.Ca5In5,
                    record.Ca5Out1, record.Ca5Out2, record.Ca5Out3, record.Ca5Out4, record.Ca5Out5
                };

                // แปลง string เป็น double และกรองเฉพาะค่าที่แปลงได้
                foreach (var stringValue in stringValues)
                {
                    if (!string.IsNullOrEmpty(stringValue) && double.TryParse(stringValue, out double numericValue))
                    {
                        allValues.Add(numericValue);
                    }
                }
            }

            return allValues.Count > 0 ? allValues.Average() : 0;
        }

        private double CalculateStandardDeviation(List<ThRecord> records, double mean)
        {
            if (records == null || records.Count == 0)
                return 0;

            var allValues = new List<double>();

            foreach (var record in records)
            {
                // เพิ่มค่าจากทุกคอลัมน์ที่ระบุ
                var stringValues = new string[]
                {
                    record.Ca1In1, record.Ca1In2, record.Ca1In3, record.Ca1In4, record.Ca1In5,
                    record.Ca1Out1, record.Ca1Out2, record.Ca1Out3, record.Ca1Out4, record.Ca1Out5,
                    record.Ca2In1, record.Ca2In2, record.Ca2In3, record.Ca2In4, record.Ca2In5,
                    record.Ca2Out1, record.Ca2Out2, record.Ca2Out3, record.Ca2Out4, record.Ca2Out5,
                    record.Ca3In1, record.Ca3In2, record.Ca3In3, record.Ca3In4, record.Ca3In5,
                    record.Ca3Out1, record.Ca3Out2, record.Ca3Out3, record.Ca3Out4, record.Ca3Out5,
                    record.Ca4In1, record.Ca4In2, record.Ca4In3, record.Ca4In4, record.Ca4In5,
                    record.Ca4Out1, record.Ca4Out2, record.Ca4Out3, record.Ca4Out4, record.Ca4Out5,
                    record.Ca5In1, record.Ca5In2, record.Ca5In3, record.Ca5In4, record.Ca5In5,
                    record.Ca5Out1, record.Ca5Out2, record.Ca5Out3, record.Ca5Out4, record.Ca5Out5
                };

                // แปลง string เป็น double และกรองเฉพาะค่าที่แปลงได้
                foreach (var stringValue in stringValues)
                {
                    if (!string.IsNullOrEmpty(stringValue) && double.TryParse(stringValue, out double numericValue))
                    {
                        allValues.Add(numericValue);
                    }
                }
            }

            if (allValues.Count <= 1) return 0;

            // คำนวณ Standard Deviation (Sample Standard Deviation)
            var sumOfSquaredDeviations = allValues.Sum(x => Math.Pow(x - mean, 2));
            return Math.Sqrt(sumOfSquaredDeviations / (allValues.Count - 1));
        }
    }

    // Result model สำหรับส่งกลับ
    public class ChartDataResult
    {
        public string ProductSize { get; set; }
        public double USL { get; set; }
        public double LSL { get; set; }
        public double Average { get; set; }
        public double StandardDeviation { get; set; }
        public double CPU { get; set; }
        public double CPL { get; set; }
        public double CPK { get; set; }
    }
}