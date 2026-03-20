using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SI24004.Models.PostgreSQL;

public partial class PostgrestContext : DbContext
{
    public PostgrestContext(DbContextOptions<PostgrestContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Admin1> Admins1 { get; set; }

    public virtual DbSet<AviRequest> AviRequests { get; set; }

    public virtual DbSet<Cache> Caches { get; set; }

    public virtual DbSet<CacheLock> CacheLocks { get; set; }

    public virtual DbSet<CompareDifference> CompareDifferences { get; set; }

    public virtual DbSet<CompareResult> CompareResults { get; set; }

    public virtual DbSet<CompareRun> CompareRuns { get; set; }

    public virtual DbSet<ComparisonResult> ComparisonResults { get; set; }

    public virtual DbSet<ComparisonResult1> ComparisonResults1 { get; set; }

    public virtual DbSet<ControlPlanEntry> ControlPlanEntries { get; set; }

    public virtual DbSet<ControlPlanEntry1> ControlPlanEntries1 { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Department1> Departments1 { get; set; }

    public virtual DbSet<Department2> Departments2 { get; set; }

    public virtual DbSet<Division> Divisions { get; set; }

    public virtual DbSet<Division1> Divisions1 { get; set; }

    public virtual DbSet<DownloadLogDrawing> DownloadLogDrawings { get; set; }

    public virtual DbSet<DwRequest> DwRequests { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Employeemaster> Employeemasters { get; set; }

    public virtual DbSet<ExamQuestion> ExamQuestions { get; set; }

    public virtual DbSet<Factory> Factories { get; set; }

    public virtual DbSet<FailedJob> FailedJobs { get; set; }

    public virtual DbSet<InaRequest> InaRequests { get; set; }

    public virtual DbSet<InventoryTransaction> InventoryTransactions { get; set; }

    public virtual DbSet<IssueStatus> IssueStatuses { get; set; }

    public virtual DbSet<ItemHoldDetail> ItemHoldDetails { get; set; }

    public virtual DbSet<ItemInventoryStat> ItemInventoryStats { get; set; }

    public virtual DbSet<ItemReceiveDetail> ItemReceiveDetails { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<JobBatch> JobBatches { get; set; }

    public virtual DbSet<License> Licenses { get; set; }

    public virtual DbSet<LicenseExam> LicenseExams { get; set; }

    public virtual DbSet<LicenseMaterial> LicenseMaterials { get; set; }

    public virtual DbSet<LicenseRecord> LicenseRecords { get; set; }

    public virtual DbSet<LicenseSo> LicenseSos { get; set; }

    public virtual DbSet<Locationmaster> Locationmasters { get; set; }

    public virtual DbSet<LotRequest> LotRequests { get; set; }

    public virtual DbSet<MachineNameMismatch> MachineNameMismatches { get; set; }

    public virtual DbSet<Materalinventory> Materalinventories { get; set; }

    public virtual DbSet<Materialreceiverecord> Materialreceiverecords { get; set; }

    public virtual DbSet<Materialtype> Materialtypes { get; set; }

    public virtual DbSet<Materialtype1> Materialtypes1 { get; set; }

    public virtual DbSet<Migration> Migrations { get; set; }

    public virtual DbSet<ParameterDifference> ParameterDifferences { get; set; }

    public virtual DbSet<ParameterDifference1> ParameterDifferences1 { get; set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<PoCheckFlow> PoCheckFlows { get; set; }

    public virtual DbSet<Process> Processes { get; set; }

    public virtual DbSet<Productmaster> Productmasters { get; set; }

    public virtual DbSet<QaSubstance> QaSubstances { get; set; }

    public virtual DbSet<RegularSubstand> RegularSubstands { get; set; }

    public virtual DbSet<RegularSubstandReordered> RegularSubstandReordereds { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<RescreenCheckRecord> RescreenCheckRecords { get; set; }

    public virtual DbSet<RescreenCheckRecord1> RescreenCheckRecords1 { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RunSession> RunSessions { get; set; }

    public virtual DbSet<RunSession1> RunSessions1 { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<Shiftmaster> Shiftmasters { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<Suppliermaster> Suppliermasters { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<User1> Users1 { get; set; }

    public virtual DbSet<VDailySummary> VDailySummaries { get; set; }

    public virtual DbSet<VNgSummary> VNgSummaries { get; set; }

    public virtual DbSet<VParamDiffDetail> VParamDiffDetails { get; set; }

    public virtual DbSet<VProblematicMachine> VProblematicMachines { get; set; }

    public virtual DbSet<VSessionSummary> VSessionSummaries { get; set; }

    public virtual DbSet<VTrend30d> VTrend30ds { get; set; }

    public virtual DbSet<VWeeklySummary> VWeeklySummaries { get; set; }

    public virtual DbSet<VwIssueSummary> VwIssueSummaries { get; set; }

    public virtual DbSet<VwItemCurrentStatus> VwItemCurrentStatuses { get; set; }

    public virtual DbSet<VwItemDailyMinmax> VwItemDailyMinmaxes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pg_catalog", "adminpack")
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("admins_pkey");

            entity.ToTable("admins", "PI", tb => tb.HasComment("Admin users for the centralized data system"));

            entity.HasIndex(e => e.Username, "admins_username_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(200)
                .HasColumnName("full_name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValueSql("'admin'::character varying")
                .HasColumnName("role");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
        });

        modelBuilder.Entity<Admin1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("admins_pkey");

            entity.ToTable("admins", "pi", tb => tb.HasComment("Admin users for the centralized data system"));

            entity.HasIndex(e => e.Username, "admins_username_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(200)
                .HasColumnName("full_name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValueSql("'admin'::character varying")
                .HasColumnName("role");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
        });

        modelBuilder.Entity<AviRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("avi_request_pk");

            entity.ToTable("avi_request", "AVI");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.ApproveDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("approve_date");
            entity.Property(e => e.AttachmentId).HasColumnName("attachment_id");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.RequestApprove).HasColumnName("request_approve");
            entity.Property(e => e.RequestCode)
                .HasColumnType("character varying")
                .HasColumnName("request_code");
            entity.Property(e => e.RequestDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("request_date");
            entity.Property(e => e.RequestDescription)
                .HasColumnType("character varying")
                .HasColumnName("request_description");
            entity.Property(e => e.RequestName)
                .HasColumnType("character varying")
                .HasColumnName("request_name");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<Cache>(entity =>
        {
            entity.HasKey(e => e.Key).HasName("cache_pkey");

            entity.ToTable("cache");

            entity.Property(e => e.Key)
                .HasMaxLength(255)
                .HasColumnName("key");
            entity.Property(e => e.Expiration).HasColumnName("expiration");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<CacheLock>(entity =>
        {
            entity.HasKey(e => e.Key).HasName("cache_locks_pkey");

            entity.ToTable("cache_locks");

            entity.Property(e => e.Key)
                .HasMaxLength(255)
                .HasColumnName("key");
            entity.Property(e => e.Expiration).HasColumnName("expiration");
            entity.Property(e => e.Owner)
                .HasMaxLength(255)
                .HasColumnName("owner");
        });

        modelBuilder.Entity<CompareDifference>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("compare_differences_pkey");

            entity.ToTable("compare_differences", "QC", tb => tb.HasComment("รายละเอียดความแตกต่างในแต่ละ cell/sheet"));

            entity.HasIndex(e => e.ResultId, "idx_diffs_result_id");

            entity.HasIndex(e => e.SheetName, "idx_diffs_sheet");

            entity.HasIndex(e => e.DiffType, "idx_diffs_type");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CellRef)
                .HasMaxLength(20)
                .HasColumnName("cell_ref");
            entity.Property(e => e.CompareValue).HasColumnName("compare_value");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DiffType)
                .HasMaxLength(30)
                .HasColumnName("diff_type");
            entity.Property(e => e.Dimension)
                .HasMaxLength(10)
                .HasColumnName("dimension");
            entity.Property(e => e.MasterValue).HasColumnName("master_value");
            entity.Property(e => e.ResultId).HasColumnName("result_id");
            entity.Property(e => e.SheetName)
                .HasMaxLength(100)
                .HasColumnName("sheet_name");

            entity.HasOne(d => d.Result).WithMany(p => p.CompareDifferences)
                .HasForeignKey(d => d.ResultId)
                .HasConstraintName("compare_differences_result_id_fkey");
        });

        modelBuilder.Entity<CompareResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("compare_results_pkey");

            entity.ToTable("compare_results", "QC", tb => tb.HasComment("ผลการเปรียบเทียบแต่ละคู่ไฟล์"));

            entity.HasIndex(e => e.MachineName, "idx_compare_results_machine");

            entity.HasIndex(e => e.RunId, "idx_compare_results_run_id");

            entity.HasIndex(e => e.RunId, "idx_compare_results_rundate");

            entity.HasIndex(e => e.Status, "idx_compare_results_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CompareFilename).HasColumnName("compare_filename");
            entity.Property(e => e.CompareUrl).HasColumnName("compare_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DiffCount)
                .HasDefaultValue(0)
                .HasColumnName("diff_count");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.MachineName)
                .HasMaxLength(100)
                .HasColumnName("machine_name");
            entity.Property(e => e.MachineNameOk)
                .HasDefaultValue(true)
                .HasColumnName("machine_name_ok");
            entity.Property(e => e.MasterFilename).HasColumnName("master_filename");
            entity.Property(e => e.MasterUrl).HasColumnName("master_url");
            entity.Property(e => e.ProductName)
                .HasMaxLength(100)
                .HasColumnName("product_name");
            entity.Property(e => e.RunId).HasColumnName("run_id");
            entity.Property(e => e.SeqNo).HasColumnName("seq_no");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");

            entity.HasOne(d => d.Run).WithMany(p => p.CompareResults)
                .HasForeignKey(d => d.RunId)
                .HasConstraintName("compare_results_run_id_fkey");
        });

        modelBuilder.Entity<CompareRun>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("compare_runs_pkey");

            entity.ToTable("compare_runs", "QC", tb => tb.HasComment("บันทึกแต่ละ session การรันเปรียบเทียบ AR/UVIR"));

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EmailSent)
                .HasDefaultValue(false)
                .HasColumnName("email_sent");
            entity.Property(e => e.EmailTo).HasColumnName("email_to");
            entity.Property(e => e.HasIssues)
                .HasDefaultValue(false)
                .HasColumnName("has_issues");
            entity.Property(e => e.MachineNameMismatches)
                .HasDefaultValue(0)
                .HasColumnName("machine_name_mismatches");
            entity.Property(e => e.ReportFile).HasColumnName("report_file");
            entity.Property(e => e.RunAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("run_at");
            entity.Property(e => e.RunDate).HasColumnName("run_date");
            entity.Property(e => e.RunType)
                .HasMaxLength(10)
                .HasColumnName("run_type");
            entity.Property(e => e.RunWeekday)
                .HasMaxLength(10)
                .HasColumnName("run_weekday");
            entity.Property(e => e.TotalChecked)
                .HasDefaultValue(0)
                .HasColumnName("total_checked");
            entity.Property(e => e.TotalDiffs)
                .HasDefaultValue(0)
                .HasColumnName("total_diffs");
            entity.Property(e => e.TotalError)
                .HasDefaultValue(0)
                .HasColumnName("total_error");
            entity.Property(e => e.TotalMatch)
                .HasDefaultValue(0)
                .HasColumnName("total_match");
            entity.Property(e => e.TotalMismatch)
                .HasDefaultValue(0)
                .HasColumnName("total_mismatch");
        });

        modelBuilder.Entity<ComparisonResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("comparison_results_pkey");

            entity.ToTable("comparison_results", "QC", tb => tb.HasComment("ผลเปรียบเทียบแต่ละคู่ folder"));

            entity.HasIndex(e => new { e.Machine, e.ModelType }, "idx_cr_machine");

            entity.HasIndex(e => e.RunAt, "idx_cr_run_at").IsDescending();

            entity.HasIndex(e => e.SessionId, "idx_cr_session");

            entity.HasIndex(e => e.Status, "idx_cr_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CommonFileCount)
                .HasDefaultValue(0)
                .HasColumnName("common_file_count");
            entity.Property(e => e.CompareFileCount)
                .HasDefaultValue(0)
                .HasColumnName("compare_file_count");
            entity.Property(e => e.CompareFolder).HasColumnName("compare_folder");
            entity.Property(e => e.ContentNgFiles)
                .HasDefaultValue(0)
                .HasColumnName("content_ng_files");
            entity.Property(e => e.ContentNgParams)
                .HasDefaultValue(0)
                .HasColumnName("content_ng_params");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Machine).HasColumnName("machine");
            entity.Property(e => e.MasterFileCount)
                .HasDefaultValue(0)
                .HasColumnName("master_file_count");
            entity.Property(e => e.MasterFolder).HasColumnName("master_folder");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.MissingInCompare)
                .HasDefaultValue(0)
                .HasColumnName("missing_in_compare");
            entity.Property(e => e.MissingInMaster)
                .HasDefaultValue(0)
                .HasColumnName("missing_in_master");
            entity.Property(e => e.ModelType).HasColumnName("model_type");
            entity.Property(e => e.No).HasColumnName("no");
            entity.Property(e => e.RunAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("run_at");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Session).WithMany(p => p.ComparisonResults)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("comparison_results_session_id_fkey");
        });

        modelBuilder.Entity<ComparisonResult1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("comparison_results_pkey");

            entity.ToTable("comparison_results", "qc");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CommonFileCount)
                .HasDefaultValue(0)
                .HasColumnName("common_file_count");
            entity.Property(e => e.CompareFileCount)
                .HasDefaultValue(0)
                .HasColumnName("compare_file_count");
            entity.Property(e => e.CompareFolder).HasColumnName("compare_folder");
            entity.Property(e => e.ContentNgFiles)
                .HasDefaultValue(0)
                .HasColumnName("content_ng_files");
            entity.Property(e => e.ContentNgParams)
                .HasDefaultValue(0)
                .HasColumnName("content_ng_params");
            entity.Property(e => e.Machine).HasColumnName("machine");
            entity.Property(e => e.MasterFileCount)
                .HasDefaultValue(0)
                .HasColumnName("master_file_count");
            entity.Property(e => e.MasterFolder).HasColumnName("master_folder");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.MissingInCompare)
                .HasDefaultValue(0)
                .HasColumnName("missing_in_compare");
            entity.Property(e => e.MissingInMaster)
                .HasDefaultValue(0)
                .HasColumnName("missing_in_master");
            entity.Property(e => e.ModelType).HasColumnName("model_type");
            entity.Property(e => e.No).HasColumnName("no");
            entity.Property(e => e.RunAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("run_at");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Session).WithMany(p => p.ComparisonResult1s)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("comparison_results_session_id_fkey");
        });

        modelBuilder.Entity<ControlPlanEntry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("control_plan_entries_pkey");

            entity.ToTable("control_plan_entries", "PI", tb => tb.HasComment("Control Plan entries based on Excel CONTROL_PLAN format"));

            entity.HasIndex(e => e.DepartmentId, "idx_control_plan_department");

            entity.HasIndex(e => e.EntryNo, "idx_control_plan_no");

            entity.HasIndex(e => e.SectionName, "idx_control_plan_section");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CheckSheet)
                .HasMaxLength(300)
                .HasColumnName("check_sheet");
            entity.Property(e => e.ConfirmFrequency)
                .HasMaxLength(200)
                .HasColumnName("confirm_frequency");
            entity.Property(e => e.ConfirmMethod)
                .HasMaxLength(300)
                .HasColumnName("confirm_method");
            entity.Property(e => e.ControlItem)
                .HasMaxLength(300)
                .HasColumnName("control_item");
            entity.Property(e => e.ControlPlanNo)
                .HasMaxLength(100)
                .HasColumnName("control_plan_no");
            entity.Property(e => e.ControlValue).HasColumnName("control_value");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CriticalItem)
                .HasMaxLength(300)
                .HasColumnName("critical_item");
            entity.Property(e => e.DataFormat)
                .HasMaxLength(300)
                .HasColumnName("data_format");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.DocumentConcern)
                .HasMaxLength(300)
                .HasColumnName("document_concern");
            entity.Property(e => e.EntryNo).HasColumnName("entry_no");
            entity.Property(e => e.PathData).HasColumnName("path_data");
            entity.Property(e => e.ProcessEquipmentName)
                .HasMaxLength(300)
                .HasColumnName("process_equipment_name");
            entity.Property(e => e.Product)
                .HasMaxLength(200)
                .HasColumnName("product");
            entity.Property(e => e.QcConfirmMethod)
                .HasMaxLength(300)
                .HasColumnName("qc_confirm_method");
            entity.Property(e => e.QcControlItem)
                .HasMaxLength(300)
                .HasColumnName("qc_control_item");
            entity.Property(e => e.QcDataFormat)
                .HasMaxLength(300)
                .HasColumnName("qc_data_format");
            entity.Property(e => e.QcFrequency)
                .HasMaxLength(200)
                .HasColumnName("qc_frequency");
            entity.Property(e => e.QcPathData).HasColumnName("qc_path_data");
            entity.Property(e => e.QcResp)
                .HasMaxLength(200)
                .HasColumnName("qc_resp");
            entity.Property(e => e.RespPerson)
                .HasMaxLength(200)
                .HasColumnName("resp_person");
            entity.Property(e => e.Revision)
                .HasMaxLength(50)
                .HasColumnName("revision");
            entity.Property(e => e.RevisionDate).HasColumnName("revision_date");
            entity.Property(e => e.SectionName)
                .HasMaxLength(200)
                .HasColumnName("section_name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ControlPlanEntryCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("control_plan_entries_created_by_fkey");

            entity.HasOne(d => d.Department).WithMany(p => p.ControlPlanEntries)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("control_plan_entries_department_id_fkey");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.ControlPlanEntryUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("control_plan_entries_updated_by_fkey");
        });

        modelBuilder.Entity<ControlPlanEntry1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("control_plan_entries_pkey");

            entity.ToTable("control_plan_entries", "pi", tb => tb.HasComment("Control Plan entries based on Excel CONTROL_PLAN format"));

            entity.HasIndex(e => e.DepartmentId, "idx_control_plan_department");

            entity.HasIndex(e => e.EntryNo, "idx_control_plan_no");

            entity.HasIndex(e => e.SectionName, "idx_control_plan_section");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ControlPlanNo)
                .HasMaxLength(100)
                .HasColumnName("control_plan_no");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CriticalItem)
                .HasMaxLength(300)
                .HasColumnName("critical_item");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.EntryNo).HasColumnName("entry_no");
            entity.Property(e => e.KpivCharacteristic)
                .HasMaxLength(300)
                .HasColumnName("kpiv_characteristic");
            entity.Property(e => e.KpivCheckSheet)
                .HasMaxLength(300)
                .HasColumnName("kpiv_check_sheet");
            entity.Property(e => e.KpivDataFormat)
                .HasMaxLength(300)
                .HasColumnName("kpiv_data_format");
            entity.Property(e => e.KpivDocument)
                .HasMaxLength(300)
                .HasColumnName("kpiv_document");
            entity.Property(e => e.KpivFrequency)
                .HasMaxLength(200)
                .HasColumnName("kpiv_frequency");
            entity.Property(e => e.KpivMethod)
                .HasMaxLength(300)
                .HasColumnName("kpiv_method");
            entity.Property(e => e.KpivPathData).HasColumnName("kpiv_path_data");
            entity.Property(e => e.KpivResp)
                .HasMaxLength(200)
                .HasColumnName("kpiv_resp");
            entity.Property(e => e.KpivSpec).HasColumnName("kpiv_spec");
            entity.Property(e => e.KpovCharacteristic)
                .HasMaxLength(300)
                .HasColumnName("kpov_characteristic");
            entity.Property(e => e.KpovDataFormat)
                .HasMaxLength(300)
                .HasColumnName("kpov_data_format");
            entity.Property(e => e.KpovFrequency)
                .HasMaxLength(200)
                .HasColumnName("kpov_frequency");
            entity.Property(e => e.KpovMethod)
                .HasMaxLength(300)
                .HasColumnName("kpov_method");
            entity.Property(e => e.KpovPathData).HasColumnName("kpov_path_data");
            entity.Property(e => e.KpovResp)
                .HasMaxLength(200)
                .HasColumnName("kpov_resp");
            entity.Property(e => e.ProcessEquipmentName)
                .HasMaxLength(300)
                .HasColumnName("process_equipment_name");
            entity.Property(e => e.Product)
                .HasMaxLength(200)
                .HasColumnName("product");
            entity.Property(e => e.Revision)
                .HasMaxLength(50)
                .HasColumnName("revision");
            entity.Property(e => e.RevisionDate).HasColumnName("revision_date");
            entity.Property(e => e.SectionName)
                .HasMaxLength(200)
                .HasColumnName("section_name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ControlPlanEntry1CreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("control_plan_entries_created_by_fkey");

            entity.HasOne(d => d.Department).WithMany(p => p.ControlPlanEntry1s)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("control_plan_entries_department_id_fkey");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.ControlPlanEntry1UpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("control_plan_entries_updated_by_fkey");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("departments_pkey");

            entity.ToTable("departments", "PI", tb => tb.HasComment("Departments within each division, e.g. PO (Polishing) in Front-end"));

            entity.HasIndex(e => e.Code, "departments_code_key").IsUnique();

            entity.HasIndex(e => e.DivisionId, "idx_departments_division");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DivisionId).HasColumnName("division_id");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0)
                .HasColumnName("sort_order");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Division).WithMany(p => p.Departments)
                .HasForeignKey(d => d.DivisionId)
                .HasConstraintName("departments_division_id_fkey");
        });

        modelBuilder.Entity<Department1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("departments_pkey");

            entity.ToTable("departments", "pi", tb => tb.HasComment("Departments within each division, e.g. PO (Polishing) in Front-end"));

            entity.HasIndex(e => e.Code, "departments_code_key").IsUnique();

            entity.HasIndex(e => e.DivisionId, "idx_departments_division");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DivisionId).HasColumnName("division_id");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0)
                .HasColumnName("sort_order");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Division).WithMany(p => p.Department1s)
                .HasForeignKey(d => d.DivisionId)
                .HasConstraintName("departments_division_id_fkey");
        });

        modelBuilder.Entity<Department2>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("department_pkey");

            entity.ToTable("department", "master");

            entity.HasIndex(e => e.Code, "department_code_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Division>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("divisions_pkey");

            entity.ToTable("divisions", "PI", tb => tb.HasComment("Production divisions: Front-end, Middle-end, Back-end, Support"));

            entity.HasIndex(e => e.Code, "divisions_code_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.Color)
                .HasMaxLength(20)
                .HasDefaultValueSql("'#3B82F6'::character varying")
                .HasColumnName("color");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0)
                .HasColumnName("sort_order");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Division1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("divisions_pkey");

            entity.ToTable("divisions", "pi", tb => tb.HasComment("Production divisions: Front-end, Middle-end, Back-end, Support"));

            entity.HasIndex(e => e.Code, "divisions_code_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.Color)
                .HasMaxLength(20)
                .HasDefaultValueSql("'#3B82F6'::character varying")
                .HasColumnName("color");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0)
                .HasColumnName("sort_order");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<DownloadLogDrawing>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("download_log_drawing_pk");

            entity.ToTable("download_log_drawing", "audit");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreateDate).HasColumnName("create_date");
            entity.Property(e => e.IpAddress)
                .HasColumnType("character varying")
                .HasColumnName("ip_address");
            entity.Property(e => e.UserEmail)
                .HasColumnType("character varying")
                .HasColumnName("user_email");
            entity.Property(e => e.UserUsername)
                .HasColumnType("character varying")
                .HasColumnName("user_username");
        });

        modelBuilder.Entity<DwRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("dw_request_pk");

            entity.ToTable("dw_request", "SSCH");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.AttachmentId).HasColumnName("attachment_id");
            entity.Property(e => e.CreatedBy)
                .HasColumnType("character varying")
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.DrawingCode)
                .HasColumnType("character varying")
                .HasColumnName("drawing_code");
            entity.Property(e => e.DrawingDescription)
                .HasColumnType("character varying")
                .HasColumnName("drawing_description");
            entity.Property(e => e.DrawingName)
                .HasColumnType("character varying")
                .HasColumnName("drawing_name");
            entity.Property(e => e.DrawingRevise).HasColumnName("drawing_revise");
            entity.Property(e => e.DrawingTypeId).HasColumnName("drawing_type_id");
            entity.Property(e => e.IsDelete).HasColumnName("is_delete");
            entity.Property(e => e.RequestCode)
                .HasColumnType("character varying")
                .HasColumnName("request_code");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.UpdateBy)
                .HasColumnType("character varying")
                .HasColumnName("update_by");
            entity.Property(e => e.UpdateDate).HasColumnName("update_date");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.OptId).HasName("employee_pkey");

            entity.ToTable("employee", "RW");

            entity.HasIndex(e => e.OptCode, "employee_opt_code_key").IsUnique();

            entity.Property(e => e.OptId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("opt_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.OptCode)
                .HasMaxLength(20)
                .HasColumnName("opt_code");
            entity.Property(e => e.OptDep)
                .HasMaxLength(100)
                .HasColumnName("opt_dep");
            entity.Property(e => e.OptDepId).HasColumnName("opt_dep_id");
            entity.Property(e => e.OptEmpType)
                .HasMaxLength(20)
                .HasDefaultValueSql("'contract'::character varying")
                .HasColumnName("opt_emp_type");
            entity.Property(e => e.OptEnddate).HasColumnName("opt_enddate");
            entity.Property(e => e.OptFac)
                .HasMaxLength(100)
                .HasColumnName("opt_fac");
            entity.Property(e => e.OptFacId).HasColumnName("opt_fac_id");
            entity.Property(e => e.OptName)
                .HasMaxLength(100)
                .HasColumnName("opt_name");
            entity.Property(e => e.OptPosition)
                .HasMaxLength(100)
                .HasColumnName("opt_position");
            entity.Property(e => e.OptShift)
                .HasMaxLength(20)
                .HasColumnName("opt_shift");
            entity.Property(e => e.OptStartdate).HasColumnName("opt_startdate");
            entity.Property(e => e.OptStatus)
                .HasMaxLength(20)
                .HasDefaultValueSql("'active'::character varying")
                .HasColumnName("opt_status");
            entity.Property(e => e.OptSurname)
                .HasMaxLength(100)
                .HasColumnName("opt_surname");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.OptDepNavigation).WithMany(p => p.Employees)
                .HasForeignKey(d => d.OptDepId)
                .HasConstraintName("employee_opt_dep_id_fkey");

            entity.HasOne(d => d.OptFacNavigation).WithMany(p => p.Employees)
                .HasForeignKey(d => d.OptFacId)
                .HasConstraintName("employee_opt_fac_id_fkey");
        });

        modelBuilder.Entity<Employeemaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("employeemaster_pkey");

            entity.ToTable("employeemaster", "LP");

            entity.HasIndex(e => e.Employeeid, "employeemaster_employeeid_key").IsUnique();

            entity.HasIndex(e => e.Employeeid, "idx_employeemaster_employeeid");

            entity.HasIndex(e => e.Shift, "idx_employeemaster_shift");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("createddate");
            entity.Property(e => e.Department)
                .HasMaxLength(50)
                .HasColumnName("department");
            entity.Property(e => e.Employeeid)
                .HasMaxLength(20)
                .HasColumnName("employeeid");
            entity.Property(e => e.Employeename)
                .HasMaxLength(100)
                .HasColumnName("employeename");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Position)
                .HasMaxLength(50)
                .HasColumnName("position");
            entity.Property(e => e.Shift).HasColumnName("shift");
            entity.Property(e => e.Updateddate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("updateddate");

            entity.HasOne(d => d.ShiftNavigation).WithMany(p => p.Employeemasters)
                .HasForeignKey(d => d.Shift)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("employeemaster_shift_fkey");
        });

        modelBuilder.Entity<ExamQuestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("exam_question_pkey");

            entity.ToTable("exam_question", "RW");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AnswerKey)
                .HasMaxLength(5)
                .HasColumnName("answer_key");
            entity.Property(e => e.Choices)
                .HasColumnType("jsonb")
                .HasColumnName("choices");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.ExamCode)
                .HasMaxLength(50)
                .HasColumnName("exam_code");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LicenseCode)
                .HasMaxLength(50)
                .HasColumnName("license_code");
            entity.Property(e => e.QuestionNo).HasColumnName("question_no");
            entity.Property(e => e.QuestionText).HasColumnName("question_text");
            entity.Property(e => e.Rev)
                .HasDefaultValue(0)
                .HasColumnName("rev");
            entity.Property(e => e.ScoreWeight)
                .HasDefaultValueSql("1")
                .HasColumnName("score_weight");
        });

        modelBuilder.Entity<Factory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("factory_pkey");

            entity.ToTable("factory", "master");

            entity.HasIndex(e => e.FacCode, "factory_fac_code_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.FacCode)
                .HasMaxLength(20)
                .HasColumnName("fac_code");
            entity.Property(e => e.FacName)
                .HasMaxLength(100)
                .HasColumnName("fac_name");
        });

        modelBuilder.Entity<FailedJob>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("failed_jobs_pkey");

            entity.ToTable("failed_jobs");

            entity.HasIndex(e => e.Uuid, "failed_jobs_uuid_unique").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Connection).HasColumnName("connection");
            entity.Property(e => e.Exception).HasColumnName("exception");
            entity.Property(e => e.FailedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(0) without time zone")
                .HasColumnName("failed_at");
            entity.Property(e => e.Payload).HasColumnName("payload");
            entity.Property(e => e.Queue).HasColumnName("queue");
            entity.Property(e => e.Uuid)
                .HasMaxLength(255)
                .HasColumnName("uuid");
        });

        modelBuilder.Entity<InaRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("avi_request_pk");

            entity.ToTable("ina_request", "INA");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.AttachmentId).HasColumnName("attachment_id");
            entity.Property(e => e.CtBookCheck).HasColumnName("ct_book_check");
            entity.Property(e => e.CtBookCheckComment)
                .HasColumnType("character varying")
                .HasColumnName("ct_book_check_comment");
            entity.Property(e => e.CtCopyRp).HasColumnName("ct_copy_rp");
            entity.Property(e => e.CtCopyRpComment)
                .HasColumnType("character varying")
                .HasColumnName("ct_copy_rp_comment");
            entity.Property(e => e.CtDeletedOther).HasColumnName("ct_deleted_other");
            entity.Property(e => e.CtDeletedOtherComment)
                .HasColumnType("character varying")
                .HasColumnName("ct_deleted_other_comment");
            entity.Property(e => e.CtRpDeleted).HasColumnName("ct_rp_deleted");
            entity.Property(e => e.CtRpDeletedComment)
                .HasColumnType("character varying")
                .HasColumnName("ct_rp_deleted_comment");
            entity.Property(e => e.FlCheckMass).HasColumnName("fl_check_mass");
            entity.Property(e => e.FlCheckMassComment)
                .HasColumnType("character varying")
                .HasColumnName("fl_check_mass_comment");
            entity.Property(e => e.FlDeletedOther).HasColumnName("fl_deleted_other");
            entity.Property(e => e.FlDeletedOtherComment)
                .HasColumnType("character varying")
                .HasColumnName("fl_deleted_other_comment");
            entity.Property(e => e.FlMcDeleted).HasColumnName("fl_mc_deleted");
            entity.Property(e => e.FlMcDeletedComment)
                .HasColumnType("character varying")
                .HasColumnName("fl_mc_deleted_comment");
            entity.Property(e => e.FlTgDeleted).HasColumnName("fl_tg_deleted");
            entity.Property(e => e.FlTgDeletedComment)
                .HasColumnType("character varying")
                .HasColumnName("fl_tg_deleted_comment");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.OtherPrograms).HasColumnName("other_programs");
            entity.Property(e => e.Recipe).HasColumnName("recipe");
            entity.Property(e => e.RequestBook)
                .HasColumnType("character varying")
                .HasColumnName("request_book");
            entity.Property(e => e.RequestBy)
                .HasColumnType("character varying")
                .HasColumnName("request_by");
            entity.Property(e => e.RequestClearDate).HasColumnName("request_clear_date");
            entity.Property(e => e.RequestClearProgram).HasColumnName("request_clear_program");
            entity.Property(e => e.RequestCode)
                .HasColumnType("character varying")
                .HasColumnName("request_code");
            entity.Property(e => e.RequestComment1)
                .HasColumnType("character varying")
                .HasColumnName("request_comment1");
            entity.Property(e => e.RequestComment2)
                .HasColumnType("character varying")
                .HasColumnName("request_comment2");
            entity.Property(e => e.RequestComment3)
                .HasColumnType("character varying")
                .HasColumnName("request_comment3");
            entity.Property(e => e.RequestDate).HasColumnName("request_date");
            entity.Property(e => e.RequestDescription)
                .HasColumnType("character varying")
                .HasColumnName("request_description");
            entity.Property(e => e.RequestFinishDate).HasColumnName("request_finish_date");
            entity.Property(e => e.RequestInstallDate).HasColumnName("request_install_date");
            entity.Property(e => e.RequestMachine).HasColumnName("request_machine");
            entity.Property(e => e.RequestMass).HasColumnName("request_mass");
            entity.Property(e => e.RequestMcNo)
                .HasColumnType("character varying")
                .HasColumnName("request_mc_no");
            entity.Property(e => e.RequestObject).HasColumnName("request_object");
            entity.Property(e => e.RequestProcess)
                .HasColumnType("character varying")
                .HasColumnName("request_process");
            entity.Property(e => e.RequestProduct)
                .HasColumnType("character varying")
                .HasColumnName("request_product");
            entity.Property(e => e.RequestPurpose)
                .HasColumnType("character varying")
                .HasColumnName("request_purpose");
            entity.Property(e => e.RequestStartDate).HasColumnName("request_start_date");
            entity.Property(e => e.RequestTest).HasColumnName("request_test");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inventory_transactions_pkey");

            entity.ToTable("inventory_transactions", "WH");

            entity.HasIndex(e => e.TransactionDate, "idx_trans_date");

            entity.HasIndex(e => e.IssueId, "idx_trans_issue");

            entity.HasIndex(e => e.ItemCode, "idx_trans_item");

            entity.HasIndex(e => new { e.ReferenceId, e.ReferenceType }, "idx_trans_ref");

            entity.HasIndex(e => e.TransactionType, "idx_trans_type");

            entity.HasIndex(e => e.TransactionId, "inventory_transactions_transaction_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.BalanceAfter)
                .HasPrecision(18, 4)
                .HasColumnName("balance_after");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DailySequence).HasColumnName("daily_sequence");
            entity.Property(e => e.IssueId)
                .HasMaxLength(20)
                .HasColumnName("issue_id");
            entity.Property(e => e.ItemCode)
                .HasMaxLength(20)
                .HasColumnName("item_code");
            entity.Property(e => e.ItemName)
                .HasColumnType("character varying")
                .HasColumnName("item_name");
            entity.Property(e => e.LotNumber)
                .HasColumnType("character varying")
                .HasColumnName("lot_number");
            entity.Property(e => e.PerformedBy)
                .HasMaxLength(50)
                .HasColumnName("performed_by");
            entity.Property(e => e.ProcessId)
                .HasColumnType("character varying")
                .HasColumnName("process_id");
            entity.Property(e => e.Quantity)
                .HasPrecision(18, 4)
                .HasColumnName("quantity");
            entity.Property(e => e.ReferenceId)
                .HasMaxLength(50)
                .HasColumnName("reference_id");
            entity.Property(e => e.ReferenceType)
                .HasMaxLength(20)
                .HasColumnName("reference_type");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.SectionId)
                .HasMaxLength(20)
                .HasColumnName("section_id");
            entity.Property(e => e.SectionName)
                .HasMaxLength(100)
                .HasColumnName("section_name");
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("transaction_date");
            entity.Property(e => e.TransactionId)
                .HasMaxLength(50)
                .HasColumnName("transaction_id");
            entity.Property(e => e.TransactionSubtype)
                .HasMaxLength(20)
                .HasColumnName("transaction_subtype");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(20)
                .HasColumnName("transaction_type");
            entity.Property(e => e.Unit)
                .HasMaxLength(20)
                .HasColumnName("unit");
        });

        modelBuilder.Entity<IssueStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("issue_status_pkey");

            entity.ToTable("issue_status", "WH");

            entity.HasIndex(e => e.CurrentStatus, "idx_issue_status_current");

            entity.HasIndex(e => new { e.IssuedDate, e.ReceivedDate }, "idx_issue_status_dates");

            entity.HasIndex(e => e.IssueId, "idx_issue_status_id");

            entity.HasIndex(e => e.IssueId, "issue_status_issue_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CompletedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completed_date");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrentStatus)
                .HasMaxLength(20)
                .HasDefaultValueSql("'pending'::character varying")
                .HasColumnName("current_status");
            entity.Property(e => e.HoldDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("hold_date");
            entity.Property(e => e.IssueId)
                .HasMaxLength(20)
                .HasColumnName("issue_id");
            entity.Property(e => e.IssuedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("issued_date");
            entity.Property(e => e.ReceivedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("received_date");
            entity.Property(e => e.ReceivedItems)
                .HasDefaultValue(0)
                .HasColumnName("received_items");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.SectionId)
                .HasMaxLength(20)
                .HasColumnName("section_id");
            entity.Property(e => e.StatusHistory)
                .HasColumnType("jsonb")
                .HasColumnName("status_history");
            entity.Property(e => e.TotalItems)
                .HasDefaultValue(0)
                .HasColumnName("total_items");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(50)
                .HasColumnName("updated_by");
            entity.Property(e => e.UserId)
                .HasMaxLength(20)
                .HasColumnName("user_id");
        });

        modelBuilder.Entity<ItemHoldDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("item_hold_details_pkey");

            entity.ToTable("item_hold_details", "WH");

            entity.HasIndex(e => new { e.ItemCode, e.HoldStatus }, "idx_hold_active").HasFilter("((hold_status)::text = 'active'::text)");

            entity.HasIndex(e => e.IssueId, "idx_hold_issue");

            entity.HasIndex(e => e.ItemCode, "idx_hold_item");

            entity.HasIndex(e => e.HoldStatus, "idx_hold_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.HoldBy)
                .HasMaxLength(50)
                .HasColumnName("hold_by");
            entity.Property(e => e.HoldDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("hold_date");
            entity.Property(e => e.HoldStatus)
                .HasMaxLength(20)
                .HasDefaultValueSql("'active'::character varying")
                .HasColumnName("hold_status");
            entity.Property(e => e.HoldUntil)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("hold_until");
            entity.Property(e => e.IssueId)
                .HasMaxLength(20)
                .HasColumnName("issue_id");
            entity.Property(e => e.ItemCode)
                .HasMaxLength(20)
                .HasColumnName("item_code");
            entity.Property(e => e.ItemName)
                .HasColumnType("character varying")
                .HasColumnName("item_name");
            entity.Property(e => e.LotNumber)
                .HasColumnType("character varying")
                .HasColumnName("lot_number");
            entity.Property(e => e.ProcessId)
                .HasColumnType("character varying")
                .HasColumnName("process_id");
            entity.Property(e => e.QuantityHold)
                .HasPrecision(18, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("quantity_hold");
            entity.Property(e => e.ReleasedBy)
                .HasMaxLength(50)
                .HasColumnName("released_by");
            entity.Property(e => e.ReleasedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("released_date");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Subunit)
                .HasColumnType("character varying")
                .HasColumnName("subunit");
            entity.Property(e => e.SubunitQty)
                .HasColumnType("character varying")
                .HasColumnName("subunit_qty");
            entity.Property(e => e.Unit)
                .HasMaxLength(20)
                .HasColumnName("unit");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<ItemInventoryStat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("item_inventory_stats_pkey");

            entity.ToTable("item_inventory_stats", "WH");

            entity.HasIndex(e => e.PeriodDate, "idx_stats_date");

            entity.HasIndex(e => e.ItemCode, "idx_stats_item");

            entity.HasIndex(e => e.PeriodType, "idx_stats_type");

            entity.HasIndex(e => new { e.ItemCode, e.PeriodDate, e.PeriodType }, "uk_stats_period").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.AvgQuantity)
                .HasPrecision(18, 4)
                .HasColumnName("avg_quantity");
            entity.Property(e => e.ClosingBalance)
                .HasPrecision(18, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("closing_balance");
            entity.Property(e => e.CountIn)
                .HasDefaultValue(0)
                .HasColumnName("count_in");
            entity.Property(e => e.CountOut)
                .HasDefaultValue(0)
                .HasColumnName("count_out");
            entity.Property(e => e.CountTransactions)
                .HasDefaultValue(0)
                .HasColumnName("count_transactions");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.ItemCode)
                .HasMaxLength(20)
                .HasColumnName("item_code");
            entity.Property(e => e.MaxDatetime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("max_datetime");
            entity.Property(e => e.MaxQuantity)
                .HasPrecision(18, 4)
                .HasColumnName("max_quantity");
            entity.Property(e => e.MinDatetime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("min_datetime");
            entity.Property(e => e.MinQuantity)
                .HasPrecision(18, 4)
                .HasColumnName("min_quantity");
            entity.Property(e => e.OpeningBalance)
                .HasPrecision(18, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("opening_balance");
            entity.Property(e => e.PeriodDate).HasColumnName("period_date");
            entity.Property(e => e.PeriodType)
                .HasMaxLength(10)
                .HasColumnName("period_type");
            entity.Property(e => e.TotalHold)
                .HasPrecision(18, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("total_hold");
            entity.Property(e => e.TotalIn)
                .HasPrecision(18, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("total_in");
            entity.Property(e => e.TotalOut)
                .HasPrecision(18, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("total_out");
            entity.Property(e => e.TotalRelease)
                .HasPrecision(18, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("total_release");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<ItemReceiveDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("item_receive_details_pkey");

            entity.ToTable("item_receive_details", "WH");

            entity.HasIndex(e => e.IssueId, "idx_receive_issue");

            entity.HasIndex(e => e.ItemCode, "idx_receive_item");

            entity.HasIndex(e => e.ReceiveStatus, "idx_receive_status");

            entity.HasIndex(e => new { e.IssueId, e.ItemCode }, "uk_receive_item").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IssueId)
                .HasMaxLength(20)
                .HasColumnName("issue_id");
            entity.Property(e => e.ItemCode)
                .HasMaxLength(20)
                .HasColumnName("item_code");
            entity.Property(e => e.ItemName)
                .HasColumnType("character varying")
                .HasColumnName("item_name");
            entity.Property(e => e.LotNumber)
                .HasColumnType("character varying")
                .HasColumnName("lot_number");
            entity.Property(e => e.ProcessId)
                .HasColumnType("character varying")
                .HasColumnName("process_id");
            entity.Property(e => e.QuantityIssued)
                .HasPrecision(18, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("quantity_issued");
            entity.Property(e => e.QuantityReceived)
                .HasPrecision(18, 4)
                .HasDefaultValueSql("0")
                .HasColumnName("quantity_received");
            entity.Property(e => e.ReceiveStatus)
                .HasMaxLength(20)
                .HasDefaultValueSql("'pending'::character varying")
                .HasColumnName("receive_status");
            entity.Property(e => e.ReceivedBy)
                .HasMaxLength(50)
                .HasColumnName("received_by");
            entity.Property(e => e.ReceivedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("received_date");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Subunit)
                .HasColumnType("character varying")
                .HasColumnName("subunit");
            entity.Property(e => e.SubunitQty)
                .HasColumnType("character varying")
                .HasColumnName("subunit_qty");
            entity.Property(e => e.Unit)
                .HasMaxLength(20)
                .HasColumnName("unit");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("jobs_pkey");

            entity.ToTable("jobs");

            entity.HasIndex(e => e.Queue, "jobs_queue_index");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Attempts).HasColumnName("attempts");
            entity.Property(e => e.AvailableAt).HasColumnName("available_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Payload).HasColumnName("payload");
            entity.Property(e => e.Queue)
                .HasMaxLength(255)
                .HasColumnName("queue");
            entity.Property(e => e.ReservedAt).HasColumnName("reserved_at");
        });

        modelBuilder.Entity<JobBatch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("job_batches_pkey");

            entity.ToTable("job_batches");

            entity.Property(e => e.Id)
                .HasMaxLength(255)
                .HasColumnName("id");
            entity.Property(e => e.CancelledAt).HasColumnName("cancelled_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.FailedJobIds).HasColumnName("failed_job_ids");
            entity.Property(e => e.FailedJobs).HasColumnName("failed_jobs");
            entity.Property(e => e.FinishedAt).HasColumnName("finished_at");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Options).HasColumnName("options");
            entity.Property(e => e.PendingJobs).HasColumnName("pending_jobs");
            entity.Property(e => e.TotalJobs).HasColumnName("total_jobs");
        });

        modelBuilder.Entity<License>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("license_pkey");

            entity.ToTable("license", "RW");

            entity.HasIndex(e => e.LicenseCode, "license_license_code_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Criteria)
                .HasMaxLength(20)
                .HasColumnName("criteria");
            entity.Property(e => e.Department)
                .HasMaxLength(100)
                .HasColumnName("department");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.EffectiveDate).HasColumnName("effective_date");
            entity.Property(e => e.LicenseCode)
                .HasMaxLength(50)
                .HasColumnName("license_code");
            entity.Property(e => e.LicenseName)
                .HasMaxLength(200)
                .HasColumnName("license_name");
            entity.Property(e => e.Process)
                .HasMaxLength(100)
                .HasColumnName("process");
            entity.Property(e => e.ProcessId).HasColumnName("process_id");
            entity.Property(e => e.Rev)
                .HasDefaultValue(0)
                .HasColumnName("rev");
            entity.Property(e => e.Section)
                .HasMaxLength(100)
                .HasColumnName("section");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.DepartmentNavigation).WithMany(p => p.Licenses)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("license_department_id_fkey");

            entity.HasOne(d => d.ProcessNavigation).WithMany(p => p.Licenses)
                .HasForeignKey(d => d.ProcessId)
                .HasConstraintName("license_process_id_fkey");

            entity.HasOne(d => d.SectionNavigation).WithMany(p => p.Licenses)
                .HasForeignKey(d => d.SectionId)
                .HasConstraintName("license_section_id_fkey");
        });

        modelBuilder.Entity<LicenseExam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("license_exam_pkey");

            entity.ToTable("license_exam", "RW");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.EffectiveDate).HasColumnName("effective_date");
            entity.Property(e => e.ExamCode)
                .HasMaxLength(50)
                .HasColumnName("exam_code");
            entity.Property(e => e.LicenseId).HasColumnName("license_id");
            entity.Property(e => e.Rev)
                .HasDefaultValue(0)
                .HasColumnName("rev");

            entity.HasOne(d => d.License).WithMany(p => p.LicenseExams)
                .HasForeignKey(d => d.LicenseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("license_exam_license_id_fkey");
        });

        modelBuilder.Entity<LicenseMaterial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("license_material_pkey");

            entity.ToTable("license_material", "RW");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.EffectiveDate).HasColumnName("effective_date");
            entity.Property(e => e.LicenseId).HasColumnName("license_id");
            entity.Property(e => e.MaterialCode)
                .HasMaxLength(50)
                .HasColumnName("material_code");
            entity.Property(e => e.MaterialName)
                .HasMaxLength(200)
                .HasColumnName("material_name");
            entity.Property(e => e.MaterialType)
                .HasMaxLength(100)
                .HasColumnName("material_type");
            entity.Property(e => e.Rev)
                .HasDefaultValue(0)
                .HasColumnName("rev");

            entity.HasOne(d => d.License).WithMany(p => p.LicenseMaterials)
                .HasForeignKey(d => d.LicenseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("license_material_license_id_fkey");
        });

        modelBuilder.Entity<LicenseRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("license_record_pkey");

            entity.ToTable("license_record", "RW");

            entity.Property(e => e.RecordId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("record_id");
            entity.Property(e => e.Answers)
                .HasColumnType("jsonb")
                .HasColumnName("answers");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.ExamPercent)
                .HasDefaultValueSql("0")
                .HasColumnName("exam_percent");
            entity.Property(e => e.ExamScore)
                .HasDefaultValueSql("0")
                .HasColumnName("exam_score");
            entity.Property(e => e.ExamTotal)
                .HasDefaultValueSql("0")
                .HasColumnName("exam_total");
            entity.Property(e => e.RecordExpiredcer).HasColumnName("record_expiredcer");
            entity.Property(e => e.RecordIssuecer).HasColumnName("record_issuecer");
            entity.Property(e => e.RecordIssuetem).HasColumnName("record_issuetem");
            entity.Property(e => e.RecordLevel)
                .HasMaxLength(20)
                .HasColumnName("record_level");
            entity.Property(e => e.RecordLicensecode)
                .HasMaxLength(50)
                .HasColumnName("record_licensecode");
            entity.Property(e => e.RecordOptcode)
                .HasMaxLength(50)
                .HasColumnName("record_optcode");
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(20)
                .HasDefaultValueSql("'pending'::character varying")
                .HasColumnName("record_status");
            entity.Property(e => e.RecordVercount)
                .HasDefaultValue(0)
                .HasColumnName("record_vercount");
            entity.Property(e => e.RecordVerdate).HasColumnName("record_verdate");
            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Request).WithMany(p => p.LicenseRecords)
                .HasForeignKey(d => d.RequestId)
                .HasConstraintName("license_record_request_id_fkey");
        });

        modelBuilder.Entity<LicenseSo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("license_sos_pkey");

            entity.ToTable("license_sos", "RW");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.EffectiveDate).HasColumnName("effective_date");
            entity.Property(e => e.LicenseId).HasColumnName("license_id");
            entity.Property(e => e.Rev)
                .HasDefaultValue(0)
                .HasColumnName("rev");
            entity.Property(e => e.SosCode)
                .HasMaxLength(50)
                .HasColumnName("sos_code");

            entity.HasOne(d => d.License).WithMany(p => p.LicenseSos)
                .HasForeignKey(d => d.LicenseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("license_sos_license_id_fkey");
        });

        modelBuilder.Entity<Locationmaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("locationmaster_pkey");

            entity.ToTable("locationmaster", "LP");

            entity.HasIndex(e => e.Locationcode, "locationmaster_locationcode_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("createddate");
            entity.Property(e => e.Currentcapacity)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("currentcapacity");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Locationcode)
                .HasMaxLength(50)
                .HasColumnName("locationcode");
            entity.Property(e => e.Locationname)
                .HasMaxLength(200)
                .HasColumnName("locationname");
            entity.Property(e => e.Maxcapacity)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("100")
                .HasColumnName("maxcapacity");
            entity.Property(e => e.Updateddate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("updateddate");
            entity.Property(e => e.Zone)
                .HasMaxLength(50)
                .HasColumnName("zone");
        });

        modelBuilder.Entity<LotRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lot_request_pk");

            entity.ToTable("lot_request", "INA");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.LotNo)
                .HasMaxLength(10)
                .HasColumnName("lot_no");
            entity.Property(e => e.RequestId).HasColumnName("request_id");

            entity.HasOne(d => d.Request).WithMany(p => p.LotRequests)
                .HasForeignKey(d => d.RequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("lot_request_ina_request_fk");
        });

        modelBuilder.Entity<MachineNameMismatch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("machine_name_mismatches_pkey");

            entity.ToTable("machine_name_mismatches", "QC");

            entity.HasIndex(e => e.RunId, "idx_machine_mismatches_run_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.MachineName)
                .HasMaxLength(100)
                .HasColumnName("machine_name");
            entity.Property(e => e.MasterUrl).HasColumnName("master_url");
            entity.Property(e => e.ProductName)
                .HasMaxLength(100)
                .HasColumnName("product_name");
            entity.Property(e => e.RowNumber).HasColumnName("row_number");
            entity.Property(e => e.RunId).HasColumnName("run_id");

            entity.HasOne(d => d.Run).WithMany(p => p.MachineNameMismatchesNavigation)
                .HasForeignKey(d => d.RunId)
                .HasConstraintName("machine_name_mismatches_run_id_fkey");
        });

        modelBuilder.Entity<Materalinventory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("materalinventories_pkey");

            entity.ToTable("materalinventories", "LP");

            entity.HasIndex(e => e.Location, "idx_materalinventories_location");

            entity.HasIndex(e => e.Matname, "idx_materalinventories_matname");

            entity.HasIndex(e => e.Product, "idx_materalinventories_product");

            entity.HasIndex(e => e.Shift, "idx_materalinventories_shift");

            entity.HasIndex(e => e.Supplier, "idx_materalinventories_supplier");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Case)
                .HasMaxLength(100)
                .HasColumnName("case");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Empid).HasColumnName("empid");
            entity.Property(e => e.Expdate).HasColumnName("expdate");
            entity.Property(e => e.Insertdate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("insertdate");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");
            entity.Property(e => e.Lotnumber)
                .HasMaxLength(100)
                .HasColumnName("lotnumber");
            entity.Property(e => e.Matname)
                .HasMaxLength(255)
                .HasColumnName("matname");
            entity.Property(e => e.Matquantity).HasColumnName("matquantity");
            entity.Property(e => e.Mattypeid).HasColumnName("mattypeid");
            entity.Property(e => e.Product).HasColumnName("product");
            entity.Property(e => e.Shift).HasColumnName("shift");
            entity.Property(e => e.Supplier).HasColumnName("supplier");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");

            entity.HasOne(d => d.Emp).WithMany(p => p.Materalinventories)
                .HasForeignKey(d => d.Empid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("materalinventories_empid_fkey");

            entity.HasOne(d => d.Mattype).WithMany(p => p.Materalinventories)
                .HasForeignKey(d => d.Mattypeid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("materalinventories_mattypeid_fkey");

            entity.HasOne(d => d.ProductNavigation).WithMany(p => p.Materalinventories)
                .HasForeignKey(d => d.Product)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("materalinventories_product_fkey");

            entity.HasOne(d => d.ShiftNavigation).WithMany(p => p.Materalinventories)
                .HasForeignKey(d => d.Shift)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("materalinventories_shift_fkey");

            entity.HasOne(d => d.SupplierNavigation).WithMany(p => p.Materalinventories)
                .HasForeignKey(d => d.Supplier)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("materalinventories_supplier_fkey");
        });

        modelBuilder.Entity<Materialreceiverecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("materialreceiverecords_pkey");

            entity.ToTable("materialreceiverecords", "LP");

            entity.HasIndex(e => e.Insertdate, "idx_materialreceiverecords_insertdate").IsDescending();

            entity.HasIndex(e => e.Matname, "idx_materialreceiverecords_matname");

            entity.HasIndex(e => e.Product, "idx_materialreceiverecords_product");

            entity.HasIndex(e => e.Shift, "idx_materialreceiverecords_shift");

            entity.HasIndex(e => e.Status, "idx_materialreceiverecords_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Case)
                .HasMaxLength(100)
                .HasColumnName("case");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Empid).HasColumnName("empid");
            entity.Property(e => e.Expdate).HasColumnName("expdate");
            entity.Property(e => e.Insertdate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("insertdate");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");
            entity.Property(e => e.Lotnumber)
                .HasMaxLength(100)
                .HasColumnName("lotnumber");
            entity.Property(e => e.Matname)
                .HasMaxLength(255)
                .HasColumnName("matname");
            entity.Property(e => e.Matquantity).HasColumnName("matquantity");
            entity.Property(e => e.Mattypeid).HasColumnName("mattypeid");
            entity.Property(e => e.Product).HasColumnName("product");
            entity.Property(e => e.Shift).HasColumnName("shift");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'in'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Supplier).HasColumnName("supplier");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");

            entity.HasOne(d => d.Emp).WithMany(p => p.Materialreceiverecords)
                .HasForeignKey(d => d.Empid)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("materialreceiverecords_empid_fkey");

            entity.HasOne(d => d.Mattype).WithMany(p => p.Materialreceiverecords)
                .HasForeignKey(d => d.Mattypeid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("materialreceiverecords_mattypeid_fkey");

            entity.HasOne(d => d.ProductNavigation).WithMany(p => p.Materialreceiverecords)
                .HasForeignKey(d => d.Product)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("materialreceiverecords_product_fkey");

            entity.HasOne(d => d.ShiftNavigation).WithMany(p => p.Materialreceiverecords)
                .HasForeignKey(d => d.Shift)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("materialreceiverecords_shift_fkey");

            entity.HasOne(d => d.SupplierNavigation).WithMany(p => p.Materialreceiverecords)
                .HasForeignKey(d => d.Supplier)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("materialreceiverecords_supplier_fkey");
        });

        modelBuilder.Entity<Materialtype>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("materialtypes_pkey");

            entity.ToTable("materialtypes");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Typename)
                .HasMaxLength(255)
                .HasColumnName("typename");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
        });

        modelBuilder.Entity<Materialtype1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("materialtypes_pkey");

            entity.ToTable("materialtypes", "LP");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Typename)
                .HasMaxLength(255)
                .HasColumnName("typename");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
        });

        modelBuilder.Entity<Migration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("migrations_pkey");

            entity.ToTable("migrations");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Batch).HasColumnName("batch");
            entity.Property(e => e.Migration1)
                .HasMaxLength(255)
                .HasColumnName("migration");
        });

        modelBuilder.Entity<ParameterDifference>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("parameter_differences_pkey");

            entity.ToTable("parameter_differences", "QC", tb => tb.HasComment("รายละเอียด parameter ที่ต่างกัน"));

            entity.HasIndex(e => new { e.Machine, e.DatFile }, "idx_pd_machine");

            entity.HasIndex(e => e.ResultId, "idx_pd_result");

            entity.HasIndex(e => e.SessionId, "idx_pd_session");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompareLine).HasColumnName("compare_line");
            entity.Property(e => e.CompareValue)
                .HasComment("ค่าใน Production")
                .HasColumnName("compare_value");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DatFile).HasColumnName("dat_file");
            entity.Property(e => e.Machine).HasColumnName("machine");
            entity.Property(e => e.MasterLine).HasColumnName("master_line");
            entity.Property(e => e.MasterValue)
                .HasComment("ค่าใน QA (master)")
                .HasColumnName("master_value");
            entity.Property(e => e.ModelType).HasColumnName("model_type");
            entity.Property(e => e.Parameter).HasColumnName("parameter");
            entity.Property(e => e.ResultId).HasColumnName("result_id");
            entity.Property(e => e.RunAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("run_at");
            entity.Property(e => e.SessionId).HasColumnName("session_id");

            entity.HasOne(d => d.Result).WithMany(p => p.ParameterDifferences)
                .HasForeignKey(d => d.ResultId)
                .HasConstraintName("parameter_differences_result_id_fkey");

            entity.HasOne(d => d.Session).WithMany(p => p.ParameterDifferences)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("parameter_differences_session_id_fkey");
        });

        modelBuilder.Entity<ParameterDifference1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("parameter_differences_pkey");

            entity.ToTable("parameter_differences", "qc");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompareLine).HasColumnName("compare_line");
            entity.Property(e => e.CompareValue).HasColumnName("compare_value");
            entity.Property(e => e.DatFile).HasColumnName("dat_file");
            entity.Property(e => e.Machine).HasColumnName("machine");
            entity.Property(e => e.MasterLine).HasColumnName("master_line");
            entity.Property(e => e.MasterValue).HasColumnName("master_value");
            entity.Property(e => e.ModelType).HasColumnName("model_type");
            entity.Property(e => e.Parameter).HasColumnName("parameter");
            entity.Property(e => e.ResultId).HasColumnName("result_id");
            entity.Property(e => e.RunAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("run_at");
            entity.Property(e => e.SessionId).HasColumnName("session_id");

            entity.HasOne(d => d.Result).WithMany(p => p.ParameterDifference1s)
                .HasForeignKey(d => d.ResultId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("parameter_differences_result_id_fkey");

            entity.HasOne(d => d.Session).WithMany(p => p.ParameterDifference1s)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("parameter_differences_session_id_fkey");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Email).HasName("password_reset_tokens_pkey");

            entity.ToTable("password_reset_tokens");

            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp(0) without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Token)
                .HasMaxLength(255)
                .HasColumnName("token");
        });

        modelBuilder.Entity<PoCheckFlow>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("po_check_flow_pkey");

            entity.ToTable("po_check_flow", "PO", tb => tb.HasComment("บันทึกการตรวจสอบ LOT ในกระบวนการผลิต"));

            entity.HasIndex(e => e.CheckDate, "idx_po_check_flow_check_date");

            entity.HasIndex(e => e.McNo, "idx_po_check_flow_mc_no");

            entity.HasIndex(e => e.PoLot, "idx_po_check_flow_po_lot");

            entity.HasIndex(e => e.StatusTn, "idx_po_check_flow_status");

            entity.HasIndex(e => e.Imobilelot, "uq_po_check_flow_imobilelot").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasComment("Primary Key (UUID)")
                .HasColumnName("id");
            entity.Property(e => e.Cassetteno)
                .HasMaxLength(50)
                .HasColumnName("cassetteno");
            entity.Property(e => e.CheckDate)
                .HasComment("วันที่ตรวจสอบ")
                .HasColumnName("check_date");
            entity.Property(e => e.CheckSt)
                .HasDefaultValue(false)
                .HasComment("Check Status (true = OK, false = NG)")
                .HasColumnName("check_st");
            entity.Property(e => e.Imobilelot)
                .HasMaxLength(100)
                .HasComment("Imobile LOT Number (Unique)")
                .HasColumnName("imobilelot");
            entity.Property(e => e.LotQty)
                .HasComment("จำนวน LOT")
                .HasColumnName("lot_qty");
            entity.Property(e => e.McNo)
                .HasMaxLength(50)
                .HasComment("Machine Number")
                .HasColumnName("mc_no");
            entity.Property(e => e.PoLot)
                .HasMaxLength(100)
                .HasComment("PO LOT Number (รูปแบบ: DDMM-MC-NoPo)")
                .HasColumnName("po_lot");
            entity.Property(e => e.StatusTn)
                .HasMaxLength(50)
                .HasComment("Status (OK, NG, SCRAP, HOLD, Rescreen)")
                .HasColumnName("status_tn");
        });

        modelBuilder.Entity<Process>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("process_pkey");

            entity.ToTable("process", "master");

            entity.HasIndex(e => e.Code, "process_code_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Section).WithMany(p => p.Processes)
                .HasForeignKey(d => d.SectionId)
                .HasConstraintName("process_section_id_fkey");
        });

        modelBuilder.Entity<Productmaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("productmaster_pkey");

            entity.ToTable("productmaster", "LP");

            entity.HasIndex(e => e.Productcode, "productmaster_productcode_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("createddate");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Productcode)
                .HasMaxLength(50)
                .HasColumnName("productcode");
            entity.Property(e => e.Productname)
                .HasMaxLength(255)
                .HasColumnName("productname");
            entity.Property(e => e.Updateddate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("updateddate");
        });

        modelBuilder.Entity<QaSubstance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("qa_substance_pk");

            entity.ToTable("qa_substance", "QA");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CasNo)
                .HasColumnType("character varying")
                .HasColumnName("cas_no");
            entity.Property(e => e.EcNo)
                .HasColumnType("character varying")
                .HasColumnName("ec_no");
            entity.Property(e => e.ReasonForInclusion)
                .HasColumnType("character varying")
                .HasColumnName("reason_for_inclusion");
            entity.Property(e => e.SubstanceName)
                .HasColumnType("character varying")
                .HasColumnName("substance_name");
            entity.Property(e => e.SvhcCandidate).HasColumnName("svhc_candidate");
            entity.Property(e => e.Uses)
                .HasColumnType("character varying")
                .HasColumnName("uses");
        });

        modelBuilder.Entity<RegularSubstand>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("regular_substand_pkey");

            entity.ToTable("regular_substand", "QA");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.SubstanceCasNo)
                .HasColumnType("character varying")
                .HasColumnName("substance_cas_no");
            entity.Property(e => e.SubstanceChemical)
                .HasColumnType("character varying")
                .HasColumnName("substance_chemical");
            entity.Property(e => e.SubstanceExamples)
                .HasColumnType("character varying")
                .HasColumnName("substance_examples");
            entity.Property(e => e.SubstanceIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("substance_identifier");
            entity.Property(e => e.SubstanceReferences)
                .HasColumnType("character varying")
                .HasColumnName("substance_references");
            entity.Property(e => e.SubstanceScope)
                .HasColumnType("character varying")
                .HasColumnName("substance_scope");
            entity.Property(e => e.SubstanceThresholdLimit)
                .HasColumnType("character varying")
                .HasColumnName("substance_threshold_limit");
        });

        modelBuilder.Entity<RegularSubstandReordered>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("regular_substand_reordered");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.SubstanceCasNo)
                .HasColumnType("character varying")
                .HasColumnName("substance_cas_no");
            entity.Property(e => e.SubstanceChemical)
                .HasColumnType("character varying")
                .HasColumnName("substance_chemical");
            entity.Property(e => e.SubstanceExamples)
                .HasColumnType("character varying")
                .HasColumnName("substance_examples");
            entity.Property(e => e.SubstanceIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("substance_identifier");
            entity.Property(e => e.SubstanceReferences)
                .HasColumnType("character varying")
                .HasColumnName("substance_references");
            entity.Property(e => e.SubstanceScope)
                .HasColumnType("character varying")
                .HasColumnName("substance_scope");
            entity.Property(e => e.SubstanceThresholdLimit)
                .HasColumnType("character varying")
                .HasColumnName("substance_threshold_limit");
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("request_pkey");

            entity.ToTable("request", "RW");

            entity.Property(e => e.RequestId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("request_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.RequestCode)
                .HasMaxLength(50)
                .HasColumnName("request_code");
            entity.Property(e => e.RequestDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("request_date");
            entity.Property(e => e.RequestFactory)
                .HasMaxLength(100)
                .HasColumnName("request_factory");
            entity.Property(e => e.RequestLicensecode)
                .HasMaxLength(100)
                .HasColumnName("request_licensecode");
            entity.Property(e => e.RequestOptcode)
                .HasMaxLength(50)
                .HasColumnName("request_optcode");
            entity.Property(e => e.RequestReason)
                .HasMaxLength(100)
                .HasColumnName("request_reason");
            entity.Property(e => e.RequestRemark).HasColumnName("request_remark");
            entity.Property(e => e.RequestStatus)
                .HasMaxLength(20)
                .HasDefaultValueSql("'requesting'::character varying")
                .HasColumnName("request_status");
            entity.Property(e => e.RequestType)
                .HasMaxLength(100)
                .HasColumnName("request_type");
            entity.Property(e => e.Requester)
                .HasMaxLength(100)
                .HasColumnName("requester");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<RescreenCheckRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("rescreen_check_record_pkey");

            entity.ToTable("rescreen_check_record", tb => tb.HasComment("บันทึกข้อมูล LOT ที่ผ่านการ Rescreen"));

            entity.HasIndex(e => e.DateProcess, "idx_rescreen_date");

            entity.HasIndex(e => e.ImobileLot, "idx_rescreen_imobile_lot").IsUnique();

            entity.HasIndex(e => new { e.LotPo, e.McPo }, "idx_rescreen_lot_mc");

            entity.HasIndex(e => e.Status, "idx_rescreen_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasComment("Primary Key")
                .HasColumnName("id");
            entity.Property(e => e.DateProcess)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("วันที่บันทึก")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_process");
            entity.Property(e => e.ImobileLot)
                .HasMaxLength(100)
                .HasComment("Imobile LOT Number (Unique)")
                .HasColumnName("imobile_lot");
            entity.Property(e => e.LotPo)
                .HasMaxLength(50)
                .HasComment("LOT PO Number")
                .HasColumnName("lot_po");
            entity.Property(e => e.McPo)
                .HasMaxLength(50)
                .HasComment("Machine Number")
                .HasColumnName("mc_po");
            entity.Property(e => e.NoPo)
                .HasMaxLength(50)
                .HasComment("NO PO")
                .HasColumnName("no_po");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'OK'::character varying")
                .HasComment("Status: OK, HOLD, PENDING")
                .HasColumnName("status");
        });

        modelBuilder.Entity<RescreenCheckRecord1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("rescreen_check_record_pkey");

            entity.ToTable("rescreen_check_record", "PO", tb => tb.HasComment("บันทึกการตรวจสอบ LOT ที่มี Status = Rescreen"));

            entity.HasIndex(e => e.CheckDate, "idx_rescreen_check_date");

            entity.HasIndex(e => e.DateProcess, "idx_rescreen_date_process");

            entity.HasIndex(e => e.FinalStatus, "idx_rescreen_final_status");

            entity.HasIndex(e => e.IsApproved, "idx_rescreen_is_approved");

            entity.HasIndex(e => new { e.LotPo, e.McPo }, "idx_rescreen_lot_mc");

            entity.HasIndex(e => e.McPo, "idx_rescreen_mc");

            entity.HasIndex(e => e.ImobileLot, "uq_rescreen_imobile_lot").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasComment("Primary Key (UUID)")
                .HasColumnName("id");
            entity.Property(e => e.ApprovedSource)
                .HasMaxLength(50)
                .HasColumnName("approved_source");
            entity.Property(e => e.CheckDate)
                .HasComment("วันที่ตรวจสอบและบันทึก")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("check_date");
            entity.Property(e => e.CheckedBy)
                .HasMaxLength(100)
                .HasComment("ผู้ตรวจสอบ")
                .HasColumnName("checked_by");
            entity.Property(e => e.DateProcess)
                .HasComment("วันที่ประมวลผล LOT")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_process");
            entity.Property(e => e.FinalStatus)
                .HasMaxLength(50)
                .HasComment("Status สุดท้ายหลังตรวจสอบ (OK, HOLD, SCRAP, Rescreen, PENDING)")
                .HasColumnName("final_status");
            entity.Property(e => e.ImobileLot)
                .HasMaxLength(100)
                .HasComment("Imobile LOT Number (Unique)")
                .HasColumnName("imobile_lot");
            entity.Property(e => e.IsApproved)
                .HasDefaultValue(false)
                .HasComment("สถานะการอนุมัติ (true = อนุมัติ, false = รอตรวจสอบ)")
                .HasColumnName("is_approved");
            entity.Property(e => e.LotPo)
                .HasMaxLength(50)
                .HasComment("LOT PO Number")
                .HasColumnName("lot_po");
            entity.Property(e => e.McPo)
                .HasMaxLength(50)
                .HasComment("Machine Number")
                .HasColumnName("mc_po");
            entity.Property(e => e.NoPo)
                .HasMaxLength(50)
                .HasComment("NO PO")
                .HasColumnName("no_po");
            entity.Property(e => e.Remarks)
                .HasMaxLength(500)
                .HasComment("หมายเหตุเพิ่มเติม")
                .HasColumnName("remarks");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasComment("Status จาก TH Record (Rescreen)")
                .HasColumnName("status");
            entity.Property(e => e.Th100Status)
                .HasMaxLength(50)
                .HasComment("Status จาก TH100 Record")
                .HasColumnName("th100_status");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("role_pkey");

            entity.ToTable("role", "master");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<RunSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("run_sessions_pkey");

            entity.ToTable("run_sessions", "QC", tb => tb.HasComment("แต่ละครั้งที่รัน compare.py"));

            entity.HasIndex(e => new { e.EmailSent, e.WeeklyEmailSent }, "idx_run_sessions_email");

            entity.HasIndex(e => e.RunAt, "idx_run_sessions_run_at").IsDescending();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.EmailSent)
                .HasDefaultValue(false)
                .HasColumnName("email_sent");
            entity.Property(e => e.ExcelFile).HasColumnName("excel_file");
            entity.Property(e => e.JsonFile).HasColumnName("json_file");
            entity.Property(e => e.RunAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("run_at");
            entity.Property(e => e.TotalChecked)
                .HasDefaultValue(0)
                .HasColumnName("total_checked");
            entity.Property(e => e.TotalError)
                .HasDefaultValue(0)
                .HasColumnName("total_error");
            entity.Property(e => e.TotalNg)
                .HasDefaultValue(0)
                .HasColumnName("total_ng");
            entity.Property(e => e.TotalOk)
                .HasDefaultValue(0)
                .HasColumnName("total_ok");
            entity.Property(e => e.TxtFile).HasColumnName("txt_file");
            entity.Property(e => e.WeeklyEmailSent)
                .HasDefaultValue(false)
                .HasComment("ส่ง weekly summary แล้วหรือยัง (วันจันทร์)")
                .HasColumnName("weekly_email_sent");
        });

        modelBuilder.Entity<RunSession1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("run_sessions_pkey");

            entity.ToTable("run_sessions", "qc");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EmailSent)
                .HasDefaultValue(false)
                .HasColumnName("email_sent");
            entity.Property(e => e.ExcelFile).HasColumnName("excel_file");
            entity.Property(e => e.JsonFile).HasColumnName("json_file");
            entity.Property(e => e.RunAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("run_at");
            entity.Property(e => e.TotalChecked)
                .HasDefaultValue(0)
                .HasColumnName("total_checked");
            entity.Property(e => e.TotalError)
                .HasDefaultValue(0)
                .HasColumnName("total_error");
            entity.Property(e => e.TotalNg)
                .HasDefaultValue(0)
                .HasColumnName("total_ng");
            entity.Property(e => e.TotalOk)
                .HasDefaultValue(0)
                .HasColumnName("total_ok");
            entity.Property(e => e.TxtFile).HasColumnName("txt_file");
            entity.Property(e => e.WeeklyEmailSent)
                .HasDefaultValue(false)
                .HasColumnName("weekly_email_sent");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("section_pkey");

            entity.ToTable("section", "master");

            entity.HasIndex(e => e.Code, "section_code_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Department).WithMany(p => p.Sections)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("section_department_id_fkey");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sessions_pkey");

            entity.ToTable("sessions");

            entity.HasIndex(e => e.LastActivity, "sessions_last_activity_index");

            entity.HasIndex(e => e.UserId, "sessions_user_id_index");

            entity.Property(e => e.Id)
                .HasMaxLength(255)
                .HasColumnName("id");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ip_address");
            entity.Property(e => e.LastActivity).HasColumnName("last_activity");
            entity.Property(e => e.Payload).HasColumnName("payload");
            entity.Property(e => e.UserAgent).HasColumnName("user_agent");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shift_pkey");

            entity.ToTable("shift", "master");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ShiftName)
                .HasMaxLength(20)
                .HasColumnName("shift_name");
        });

        modelBuilder.Entity<Shiftmaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shiftmaster_pkey");

            entity.ToTable("shiftmaster", "LP");

            entity.HasIndex(e => e.Shiftcode, "shiftmaster_shiftcode_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("createddate");
            entity.Property(e => e.Endtime).HasColumnName("endtime");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Shiftcode)
                .HasMaxLength(20)
                .HasColumnName("shiftcode");
            entity.Property(e => e.Shiftname)
                .HasMaxLength(100)
                .HasColumnName("shiftname");
            entity.Property(e => e.Starttime).HasColumnName("starttime");
            entity.Property(e => e.Updateddate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("updateddate");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("status_pkey");

            entity.ToTable("status", "master");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<Suppliermaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("suppliermaster_pkey");

            entity.ToTable("suppliermaster", "LP");

            entity.HasIndex(e => e.Suppliercode, "suppliermaster_suppliercode_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(500)
                .HasColumnName("address");
            entity.Property(e => e.Contactperson)
                .HasMaxLength(100)
                .HasColumnName("contactperson");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("createddate");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Suppliercode)
                .HasMaxLength(50)
                .HasColumnName("suppliercode");
            entity.Property(e => e.Suppliername)
                .HasMaxLength(255)
                .HasColumnName("suppliername");
            entity.Property(e => e.Updateddate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("updateddate");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_unique").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp(0) without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.EmailVerifiedAt)
                .HasColumnType("timestamp(0) without time zone")
                .HasColumnName("email_verified_at");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.RememberToken)
                .HasMaxLength(100)
                .HasColumnName("remember_token");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp(0) without time zone")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<User1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users", "master");

            entity.HasIndex(e => e.UserId, "users_user_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Active)
                .HasDefaultValue(true)
                .HasColumnName("active");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasMaxLength(100)
                .HasColumnName("user_id");
            entity.Property(e => e.UserLastname)
                .HasMaxLength(100)
                .HasColumnName("user_lastname");
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .HasColumnName("user_name");
            entity.Property(e => e.UserPassword)
                .HasMaxLength(255)
                .HasColumnName("user_password");

            entity.HasOne(d => d.Role).WithMany(p => p.User1s)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("users_role_id_fkey");

            entity.HasOne(d => d.Section).WithMany(p => p.User1s)
                .HasForeignKey(d => d.SectionId)
                .HasConstraintName("users_section_id_fkey");
        });

        modelBuilder.Entity<VDailySummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_daily_summary", "QC");

            entity.Property(e => e.HasIssues).HasColumnName("has_issues");
            entity.Property(e => e.MachineNameMismatches).HasColumnName("machine_name_mismatches");
            entity.Property(e => e.RunDate).HasColumnName("run_date");
            entity.Property(e => e.RunType)
                .HasMaxLength(10)
                .HasColumnName("run_type");
            entity.Property(e => e.TotalChecked).HasColumnName("total_checked");
            entity.Property(e => e.TotalDiffs).HasColumnName("total_diffs");
            entity.Property(e => e.TotalError).HasColumnName("total_error");
            entity.Property(e => e.TotalMatch).HasColumnName("total_match");
            entity.Property(e => e.TotalMismatch).HasColumnName("total_mismatch");
            entity.Property(e => e.TotalRuns).HasColumnName("total_runs");
        });

        modelBuilder.Entity<VNgSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_ng_summary", "QC");

            entity.Property(e => e.CompareFolder).HasColumnName("compare_folder");
            entity.Property(e => e.ContentNgFiles).HasColumnName("content_ng_files");
            entity.Property(e => e.ContentNgParams).HasColumnName("content_ng_params");
            entity.Property(e => e.JsonFile).HasColumnName("json_file");
            entity.Property(e => e.Machine).HasColumnName("machine");
            entity.Property(e => e.MasterFolder).HasColumnName("master_folder");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.MissingInMaster).HasColumnName("missing_in_master");
            entity.Property(e => e.ModelType).HasColumnName("model_type");
            entity.Property(e => e.RunAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("run_at");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TxtFile).HasColumnName("txt_file");
        });

        modelBuilder.Entity<VParamDiffDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_param_diff_detail", "QC");

            entity.Property(e => e.DatFile).HasColumnName("dat_file");
            entity.Property(e => e.Machine).HasColumnName("machine");
            entity.Property(e => e.ModelType).HasColumnName("model_type");
            entity.Property(e => e.Parameter).HasColumnName("parameter");
            entity.Property(e => e.ProdLine).HasColumnName("prod_line");
            entity.Property(e => e.ProdValue).HasColumnName("prod_value");
            entity.Property(e => e.QaLine).HasColumnName("qa_line");
            entity.Property(e => e.QaValue).HasColumnName("qa_value");
            entity.Property(e => e.ResultId).HasColumnName("result_id");
            entity.Property(e => e.RunAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("run_at");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
        });

        modelBuilder.Entity<VProblematicMachine>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_problematic_machines", "QC");

            entity.Property(e => e.Errors).HasColumnName("errors");
            entity.Property(e => e.LastSeen).HasColumnName("last_seen");
            entity.Property(e => e.MachineName)
                .HasMaxLength(100)
                .HasColumnName("machine_name");
            entity.Property(e => e.MatchRatePct).HasColumnName("match_rate_pct");
            entity.Property(e => e.Matches).HasColumnName("matches");
            entity.Property(e => e.Mismatches).HasColumnName("mismatches");
            entity.Property(e => e.ProductName)
                .HasMaxLength(100)
                .HasColumnName("product_name");
            entity.Property(e => e.RunType)
                .HasMaxLength(10)
                .HasColumnName("run_type");
            entity.Property(e => e.TotalComparisons).HasColumnName("total_comparisons");
            entity.Property(e => e.TotalDiffs).HasColumnName("total_diffs");
        });

        modelBuilder.Entity<VSessionSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_session_summary", "QC");

            entity.Property(e => e.EmailSent).HasColumnName("email_sent");
            entity.Property(e => e.ExcelFile).HasColumnName("excel_file");
            entity.Property(e => e.JsonFile).HasColumnName("json_file");
            entity.Property(e => e.OkPercent).HasColumnName("ok_percent");
            entity.Property(e => e.RunAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("run_at");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.TotalChecked).HasColumnName("total_checked");
            entity.Property(e => e.TotalError).HasColumnName("total_error");
            entity.Property(e => e.TotalNg).HasColumnName("total_ng");
            entity.Property(e => e.TotalOk).HasColumnName("total_ok");
            entity.Property(e => e.TxtFile).HasColumnName("txt_file");
            entity.Property(e => e.WeeklyEmailSent).HasColumnName("weekly_email_sent");
        });

        modelBuilder.Entity<VTrend30d>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_trend_30d", "QC");

            entity.Property(e => e.Checked).HasColumnName("checked");
            entity.Property(e => e.Diffs).HasColumnName("diffs");
            entity.Property(e => e.Errored).HasColumnName("errored");
            entity.Property(e => e.MatchRatePct).HasColumnName("match_rate_pct");
            entity.Property(e => e.Matched).HasColumnName("matched");
            entity.Property(e => e.Mismatched).HasColumnName("mismatched");
            entity.Property(e => e.RunDate).HasColumnName("run_date");
            entity.Property(e => e.RunType)
                .HasMaxLength(10)
                .HasColumnName("run_type");
        });

        modelBuilder.Entity<VWeeklySummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_weekly_summary", "QC");

            entity.Property(e => e.ErrorCount).HasColumnName("error_count");
            entity.Property(e => e.MachinesChecked).HasColumnName("machines_checked");
            entity.Property(e => e.NgCount).HasColumnName("ng_count");
            entity.Property(e => e.OkCount).HasColumnName("ok_count");
            entity.Property(e => e.TotalNgParams).HasColumnName("total_ng_params");
            entity.Property(e => e.TotalRuns).HasColumnName("total_runs");
            entity.Property(e => e.WeekStart)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("week_start");
        });

        modelBuilder.Entity<VwIssueSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_issue_summary", "WH");

            entity.Property(e => e.ActiveHolds).HasColumnName("active_holds");
            entity.Property(e => e.CompletedItems).HasColumnName("completed_items");
            entity.Property(e => e.CurrentStatus)
                .HasMaxLength(20)
                .HasColumnName("current_status");
            entity.Property(e => e.IssueId)
                .HasMaxLength(20)
                .HasColumnName("issue_id");
            entity.Property(e => e.IssuedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("issued_date");
            entity.Property(e => e.ReceivedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("received_date");
            entity.Property(e => e.ReceivedItems).HasColumnName("received_items");
            entity.Property(e => e.SectionId)
                .HasMaxLength(20)
                .HasColumnName("section_id");
            entity.Property(e => e.TotalItemCodes).HasColumnName("total_item_codes");
            entity.Property(e => e.TotalItems).HasColumnName("total_items");
            entity.Property(e => e.TotalQuantityIssued).HasColumnName("total_quantity_issued");
            entity.Property(e => e.TotalQuantityReceived).HasColumnName("total_quantity_received");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(50)
                .HasColumnName("updated_by");
        });

        modelBuilder.Entity<VwItemCurrentStatus>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_item_current_status", "WH");

            entity.Property(e => e.AvailableQuantity).HasColumnName("available_quantity");
            entity.Property(e => e.CurrentBalance)
                .HasPrecision(18, 4)
                .HasColumnName("current_balance");
            entity.Property(e => e.HoldCount).HasColumnName("hold_count");
            entity.Property(e => e.ItemCode)
                .HasMaxLength(20)
                .HasColumnName("item_code");
            entity.Property(e => e.ItemName)
                .HasColumnType("character varying")
                .HasColumnName("item_name");
            entity.Property(e => e.LastTransactionDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_transaction_date");
            entity.Property(e => e.TotalHold).HasColumnName("total_hold");
        });

        modelBuilder.Entity<VwItemDailyMinmax>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_item_daily_minmax", "WH");

            entity.Property(e => e.AvgQuantity).HasColumnName("avg_quantity");
            entity.Property(e => e.ClosingBalance).HasColumnName("closing_balance");
            entity.Property(e => e.FirstTransaction)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("first_transaction");
            entity.Property(e => e.ItemCode)
                .HasMaxLength(20)
                .HasColumnName("item_code");
            entity.Property(e => e.LastTransaction)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_transaction");
            entity.Property(e => e.MaxQuantity).HasColumnName("max_quantity");
            entity.Property(e => e.MinQuantity).HasColumnName("min_quantity");
            entity.Property(e => e.OpeningBalance).HasColumnName("opening_balance");
            entity.Property(e => e.TransactionCount).HasColumnName("transaction_count");
            entity.Property(e => e.TransactionDate).HasColumnName("transaction_date");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
