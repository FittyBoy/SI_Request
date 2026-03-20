using System;
using System.Collections.Generic;

namespace SI24004.Models.PostgreSQL;

public partial class ExamQuestion
{
    public Guid Id { get; set; }

    public string ExamCode { get; set; } = null!;

    public string LicenseCode { get; set; } = null!;

    public int? Rev { get; set; }

    public int QuestionNo { get; set; }

    public string QuestionText { get; set; } = null!;

    public string Choices { get; set; } = null!;

    public string? AnswerKey { get; set; }

    public decimal? ScoreWeight { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }
}
