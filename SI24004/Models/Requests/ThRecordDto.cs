using System;
using System.ComponentModel.DataAnnotations;

namespace SI24004.DTO
{
    public class ThRecordDTO
    {
        public int LotId { get; set; }
        public string? LotPo { get; set; }
        public string? McPo { get; set; }
        public string? NoPo { get; set; }
        public string? MemberId { get; set; }
        public string? Status { get; set; }
        public DateTime DateProcess { get; set; }
        public DateTime TimeProcess { get; set; }
        public string? ThBefore { get; set; }
        public string? AvgTh { get; set; }
        public string? ProcessTime { get; set; }
        public string? PoRate { get; set; }
        public string? ThDif { get; set; }
        public string? Margin { get; set; }
        public string? Result { get; set; }
        public string? Remark { get; set; }
        public string? ImobileLot { get; set; }
        public string? ImobileType { get; set; }
        public string? ImobileSize { get; set; }
        public string? McType { get; set; }
        public string? Process { get; set; }

        // Ca1In, Ca1Out, etc. จะเป็น List<decimal?> เนื่องจากข้อมูลบางค่ามีค่า null ได้
        public List<decimal?> Ca1In { get; set; }
        public List<decimal?> Ca1Out { get; set; }
        public List<decimal?> Ca2In { get; set; }
        public List<decimal?> Ca2Out { get; set; }
        public List<decimal?> Ca3In { get; set; }
        public List<decimal?> Ca3Out { get; set; }
        public List<decimal?> Ca4In { get; set; }
        public List<decimal?> Ca4Out { get; set; }
        public List<decimal?> Ca5In { get; set; }
        public List<decimal?> Ca5Out { get; set; }

        public string? ThCin { get; set; }
        public string? ThCout1 { get; set; }
        public string? ThCout2 { get; set; }
        public string? ThCout3 { get; set; }
        public string? ThCout4 { get; set; }
        public string? ThCout5 { get; set; }

        public string? Hostname { get; set; }
        public string? IpAddress { get; set; }
        public string? LaserMC { get; set; }
        public int? ProcessStep { get; set; }

        public List<decimal?> Ca1InExtra { get; set; }
        public List<decimal?> Ca1OutExtra { get; set; }
        public List<decimal?> Ca2InExtra { get; set; }
        public List<decimal?> Ca3InExtra { get; set; }
        public List<decimal?> Ca4InExtra { get; set; }
        public List<decimal?> Ca5InExtra { get; set; }
        public List<decimal?> ThCinExtra { get; set; }

        public string? Stutus { get; set; }
    }

}
