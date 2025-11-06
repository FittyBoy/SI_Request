namespace SI24004.Models.Requests
{
    public class InaRequestDto
    {
        public Guid? Id { get; set; }
        public Guid UserId { get; set; }
        public string? RequestDescription { get; set; }
        public Guid? AttachmentId { get; set; }
        public bool Active { get; set; }
        public bool? IsDeleted { get; set; }
        public string? RequestCode { get; set; }
        public Guid? StatusId { get; set; }
        public DateTime? RequestDate { get; set; }
        public Guid? RequestMachine { get; set; }
        public string? RequestProduct { get; set; }
        public bool? RequestTest { get; set; }
        public bool? RequestMass { get; set; }
        public string? RequestComment1 { get; set; }
        public string? RequestComment2 { get; set; }
        public string? RequestComment3 { get; set; }
        public Guid? Recipe { get; set; }
        public Guid? OtherPrograms { get; set; }
        public string? RequestBy { get; set; }
        public string? RequestProcess { get; set; }
        public string? RequestPurpose { get; set; }
        public DateTime? RequestStartDate { get; set; }
        public DateTime? RequestFinishDate { get; set; }
        public bool? FlTgDeleted { get; set; }

        public bool? FlMcDeleted { get; set; }

        public bool? FlCheckMass { get; set; }

        public bool? FlDeletedOther { get; set; }

        public bool? CtCopyRp { get; set; }

        public bool? CtRpDeleted { get; set; }

        public bool? CtBookCheck { get; set; }

        public bool? CtDeletedOther { get; set; }

        public string? FlTgDeletedComment { get; set; }

        public string? FlMcDeletedComment { get; set; }

        public string? FlCheckMassComment { get; set; }

        public string? FlDeletedOtherComment { get; set; }

        public string? CtCopyRpComment { get; set; }

        public string? CtRpDeletedComment { get; set; }

        public string? CtBookCheckComment { get; set; }

        public string? CtDeletedOtherComment { get; set; }

        // Navigation properties
        public Attachment? Attachment { get; set; }
        public Status? Status { get; set; }
        public User? User { get; set; }

        public Guid? RequestObjective { get; set; }
        public Guid? RequestType { get; set; }
        public string? FlowInFactory { get; set; }
        public string? RequestMcNo { get; set; }
        public string? RequestBook { get; set; }
        public DateTime? RequestInstallDate { get; set; }
        public DateTime? RequestClearDate { get; set; }
        public bool? RequestClearProgram { get; set; }

    }
}