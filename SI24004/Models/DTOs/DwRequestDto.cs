using SI24004.Models.PostgreSQL;
﻿using System.ComponentModel.DataAnnotations.Schema;

namespace SI24004.Models.DTOs
{
    public class DwRequestDto
    {
        public Guid? Id { get; set; }
        public string? DrawingCode { get; set; } // เปลี่ยนจาก RequestName
        public string? RequestCode { get; set; }
        public string? DrawingName { get; set; } // เปลี่ยนจาก RequestName
        public Guid SectionId { get; set; }
        public Guid DrawingTypeId { get; set; }
        public Guid? StatusId { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime? CreatedDate { get; set; } // แทน RequestDate
        public string? CreatedBy { get; set; } // แทน RequestDate
        [Column(TypeName = "timestamp with time zone")]
        public DateTime? UpdateDate { get; set; } // แทน RequestDate
        public string? UpdateBy { get; set; } // แทน RequestDate
        public string? DrawingDescription { get; set; } // เปลี่ยนจาก RequestDescription
        public Guid UserId { get; set; }
        public Guid? AttachmentId { get; set; }
        public bool Active { get; set; }
        public bool? IsDeleted { get; set; }
        public Attachment? Attachment { get; set; }

    }
}
