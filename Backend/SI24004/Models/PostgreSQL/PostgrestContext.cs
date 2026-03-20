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

    public virtual DbSet<Attachment> Attachments { get; set; }

    public virtual DbSet<AviRequest> AviRequests { get; set; }

    public virtual DbSet<Cache> Caches { get; set; }

    public virtual DbSet<CacheLock> CacheLocks { get; set; }

    public virtual DbSet<DownloadLogDrawing> DownloadLogDrawings { get; set; }

    public virtual DbSet<Drawing> Drawings { get; set; }

    public virtual DbSet<DwRequest> DwRequests { get; set; }

    public virtual DbSet<Employeemaster> Employeemasters { get; set; }

    public virtual DbSet<FailedJob> FailedJobs { get; set; }

    public virtual DbSet<InaRequest> InaRequests { get; set; }

    public virtual DbSet<InventoryTransaction> InventoryTransactions { get; set; }

    public virtual DbSet<IssueStatus> IssueStatuses { get; set; }

    public virtual DbSet<ItemHoldDetail> ItemHoldDetails { get; set; }

    public virtual DbSet<ItemInventoryStat> ItemInventoryStats { get; set; }

    public virtual DbSet<ItemReceiveDetail> ItemReceiveDetails { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<JobBatch> JobBatches { get; set; }

    public virtual DbSet<ListItem> ListItems { get; set; }

    public virtual DbSet<Locationmaster> Locationmasters { get; set; }

    public virtual DbSet<LotRequest> LotRequests { get; set; }

    public virtual DbSet<Materalinventory> Materalinventories { get; set; }

    public virtual DbSet<Materialreceiverecord> Materialreceiverecords { get; set; }

    public virtual DbSet<Materialtype> Materialtypes { get; set; }

    public virtual DbSet<Materialtype1> Materialtypes1 { get; set; }

    public virtual DbSet<Migration> Migrations { get; set; }

    public virtual DbSet<Objective> Objectives { get; set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<PoCheckFlow> PoCheckFlows { get; set; }

    public virtual DbSet<Productmaster> Productmasters { get; set; }

    public virtual DbSet<QaSubstance> QaSubstances { get; set; }

    public virtual DbSet<RegularSubstand> RegularSubstands { get; set; }

    public virtual DbSet<RegularSubstandReordered> RegularSubstandReordereds { get; set; }

    public virtual DbSet<RequestMachine> RequestMachines { get; set; }

    public virtual DbSet<RescreenCheckRecord> RescreenCheckRecords { get; set; }

    public virtual DbSet<RescreenCheckRecord1> RescreenCheckRecords1 { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<Shiftmaster> Shiftmasters { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<Suppliermaster> Suppliermasters { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<User1> Users1 { get; set; }

    public virtual DbSet<VwIssueSummary> VwIssueSummaries { get; set; }

    public virtual DbSet<VwItemCurrentStatus> VwItemCurrentStatuses { get; set; }

    public virtual DbSet<VwItemDailyMinmax> VwItemDailyMinmaxes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pg_catalog", "adminpack")
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("attrachment_pk");

            entity.ToTable("attachment", "master");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.AttachementFileData).HasColumnName("attachement_file_data");
            entity.Property(e => e.AttachementPath)
                .HasColumnType("character varying")
                .HasColumnName("attachement_path");
            entity.Property(e => e.AttachementType)
                .HasColumnType("character varying")
                .HasColumnName("attachement_type");
            entity.Property(e => e.AttachmentFileLocation)
                .HasColumnType("character varying")
                .HasColumnName("attachment_file_location");
            entity.Property(e => e.AttachmentName)
                .IsRequired()
                .HasColumnType("character varying")
                .HasColumnName("attachment_name");
            entity.Property(e => e.AttachmentSize)
                .HasColumnType("character varying")
                .HasColumnName("attachment_size");
            entity.Property(e => e.Category)
                .HasColumnType("character varying")
                .HasColumnName("category");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.UploadDate).HasColumnName("upload_date");
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
                .IsRequired()
                .HasColumnType("character varying")
                .HasColumnName("request_code");
            entity.Property(e => e.RequestDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("request_date");
            entity.Property(e => e.RequestDescription)
                .HasColumnType("character varying")
                .HasColumnName("request_description");
            entity.Property(e => e.RequestName)
                .IsRequired()
                .HasColumnType("character varying")
                .HasColumnName("request_name");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Attachment).WithMany(p => p.AviRequests)
                .HasForeignKey(d => d.AttachmentId)
                .HasConstraintName("avi_request_attachment_fk");

            entity.HasOne(d => d.User).WithMany(p => p.AviRequests)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("avi_request_users_fk");
        });

        modelBuilder.Entity<Cache>(entity =>
        {
            entity.HasKey(e => e.Key).HasName("cache_pkey");

            entity.ToTable("cache");

            entity.Property(e => e.Key)
                .HasMaxLength(255)
                .HasColumnName("key");
            entity.Property(e => e.Expiration).HasColumnName("expiration");
            entity.Property(e => e.Value)
                .IsRequired()
                .HasColumnName("value");
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
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("owner");
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

        modelBuilder.Entity<Drawing>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("drawing_pk");

            entity.ToTable("drawing", "master");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.DrawingCode)
                .IsRequired()
                .HasColumnType("character varying")
                .HasColumnName("drawing_code");
            entity.Property(e => e.DrawingName)
                .HasColumnType("character varying")
                .HasColumnName("drawing_name");
            entity.Property(e => e.ListItemId).HasColumnName("list_item_id");

            entity.HasOne(d => d.ListItem).WithMany(p => p.Drawings)
                .HasForeignKey(d => d.ListItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("drawing_list_item_fk");
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
                .IsRequired()
                .HasColumnType("character varying")
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.DrawingCode)
                .IsRequired()
                .HasColumnType("character varying")
                .HasColumnName("drawing_code");
            entity.Property(e => e.DrawingDescription)
                .HasColumnType("character varying")
                .HasColumnName("drawing_description");
            entity.Property(e => e.DrawingName)
                .IsRequired()
                .HasColumnType("character varying")
                .HasColumnName("drawing_name");
            entity.Property(e => e.DrawingRevise).HasColumnName("drawing_revise");
            entity.Property(e => e.DrawingTypeId).HasColumnName("drawing_type_id");
            entity.Property(e => e.IsDelete).HasColumnName("is_delete");
            entity.Property(e => e.RequestCode)
                .IsRequired()
                .HasColumnType("character varying")
                .HasColumnName("request_code");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.UpdateBy)
                .HasColumnType("character varying")
                .HasColumnName("update_by");
            entity.Property(e => e.UpdateDate).HasColumnName("update_date");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Attachment).WithMany(p => p.DwRequests)
                .HasForeignKey(d => d.AttachmentId)
                .HasConstraintName("dw_request_attachment_fk");

            entity.HasOne(d => d.DrawingType).WithMany(p => p.DwRequests)
                .HasForeignKey(d => d.DrawingTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dw_request_drawing_fk");

            entity.HasOne(d => d.Section).WithMany(p => p.DwRequests)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dw_request_section_fk");

            entity.HasOne(d => d.Status).WithMany(p => p.DwRequests)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dw_request_status_fk");

            entity.HasOne(d => d.User).WithMany(p => p.DwRequests)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("dw_request_users_fk");
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
                .IsRequired()
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

        modelBuilder.Entity<FailedJob>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("failed_jobs_pkey");

            entity.ToTable("failed_jobs");

            entity.HasIndex(e => e.Uuid, "failed_jobs_uuid_unique").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Connection)
                .IsRequired()
                .HasColumnName("connection");
            entity.Property(e => e.Exception)
                .IsRequired()
                .HasColumnName("exception");
            entity.Property(e => e.FailedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(0) without time zone")
                .HasColumnName("failed_at");
            entity.Property(e => e.Payload)
                .IsRequired()
                .HasColumnName("payload");
            entity.Property(e => e.Queue)
                .IsRequired()
                .HasColumnName("queue");
            entity.Property(e => e.Uuid)
                .IsRequired()
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
                .IsRequired()
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

            entity.HasOne(d => d.Attachment).WithMany(p => p.InaRequests)
                .HasForeignKey(d => d.AttachmentId)
                .HasConstraintName("ina_request_attachment_fk");

            entity.HasOne(d => d.Status).WithMany(p => p.InaRequests)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("ina_request_status_fk");

            entity.HasOne(d => d.User).WithMany(p => p.InaRequests)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ina_request_users_fk");
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
                .IsRequired()
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
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("transaction_id");
            entity.Property(e => e.TransactionSubtype)
                .HasMaxLength(20)
                .HasColumnName("transaction_subtype");
            entity.Property(e => e.TransactionType)
                .IsRequired()
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
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValueSql("'pending'::character varying")
                .HasColumnName("current_status");
            entity.Property(e => e.HoldDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("hold_date");
            entity.Property(e => e.IssueId)
                .IsRequired()
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
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("issue_id");
            entity.Property(e => e.ItemCode)
                .IsRequired()
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
                .IsRequired()
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
                .IsRequired()
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
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("issue_id");
            entity.Property(e => e.ItemCode)
                .IsRequired()
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
            entity.Property(e => e.Payload)
                .IsRequired()
                .HasColumnName("payload");
            entity.Property(e => e.Queue)
                .IsRequired()
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
            entity.Property(e => e.FailedJobIds)
                .IsRequired()
                .HasColumnName("failed_job_ids");
            entity.Property(e => e.FailedJobs).HasColumnName("failed_jobs");
            entity.Property(e => e.FinishedAt).HasColumnName("finished_at");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Options).HasColumnName("options");
            entity.Property(e => e.PendingJobs).HasColumnName("pending_jobs");
            entity.Property(e => e.TotalJobs).HasColumnName("total_jobs");
        });

        modelBuilder.Entity<ListItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("list_item_pk");

            entity.ToTable("list_item", "master");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.ListItemCode)
                .IsRequired()
                .HasColumnType("character varying")
                .HasColumnName("list_item_code");
            entity.Property(e => e.ListItemName)
                .IsRequired()
                .HasColumnType("character varying")
                .HasColumnName("list_item_name");
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
                .IsRequired()
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
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnName("lot_no");
            entity.Property(e => e.RequestId).HasColumnName("request_id");

            entity.HasOne(d => d.Request).WithMany(p => p.LotRequests)
                .HasForeignKey(d => d.RequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("lot_request_ina_request_fk");
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
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("location");
            entity.Property(e => e.Lotnumber)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("lotnumber");
            entity.Property(e => e.Matname)
                .IsRequired()
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
                .IsRequired()
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
                .IsRequired()
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
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("migration");
        });

        modelBuilder.Entity<Objective>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("objective_pk");

            entity.ToTable("objective", "master");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.ListItemId).HasColumnName("list_item_id");
            entity.Property(e => e.ObjectName)
                .HasColumnType("character varying")
                .HasColumnName("object_name");

            entity.HasOne(d => d.ListItem).WithMany(p => p.Objectives)
                .HasForeignKey(d => d.ListItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("objective_list_item_fk");
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
                .IsRequired()
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
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("productcode");
            entity.Property(e => e.Productname)
                .IsRequired()
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

        modelBuilder.Entity<RequestMachine>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("request_machine", "master");

            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Id)
                .IsRequired()
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.ListItemId).HasColumnName("list_item_id");
            entity.Property(e => e.RequestMachineName)
                .HasColumnType("character varying")
                .HasColumnName("request_machine_name");

            entity.HasOne(d => d.ListItem).WithMany()
                .HasForeignKey(d => d.ListItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("request_machine_list_item_fk");
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
                .IsRequired()
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
                .IsRequired()
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
            entity.Property(e => e.ApprovedSource)           // ← เพิ่ม
                .HasMaxLength(50)
                .HasComment("แหล่งที่มาของ Approve: TH100 Confirm, Approved, Pending")
                .HasColumnName("approved_source");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("rule_pk");

            entity.ToTable("role", "master");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.RoleName)
                .HasColumnType("character varying")
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("section_pk");

            entity.ToTable("section", "master");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.SectionCode)
                .IsRequired()
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SectionName)
                .IsRequired()
                .HasColumnType("character varying")
                .HasColumnName("section_name");
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
            entity.Property(e => e.Payload)
                .IsRequired()
                .HasColumnName("payload");
            entity.Property(e => e.UserAgent).HasColumnName("user_agent");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shif_pk");

            entity.ToTable("shift", "master");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.ListItemId).HasColumnName("list_item_id");
            entity.Property(e => e.ShiftName)
                .HasColumnType("character varying")
                .HasColumnName("shift_name");

            entity.HasOne(d => d.ListItem).WithMany(p => p.Shifts)
                .HasForeignKey(d => d.ListItemId)
                .HasConstraintName("shift_list_item_fk");
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
                .IsRequired()
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
            entity.HasKey(e => e.Id).HasName("status_pk");

            entity.ToTable("status", "master");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.Ordinal).HasColumnName("ordinal");
            entity.Property(e => e.StatusName)
                .HasColumnType("character varying")
                .HasColumnName("status_name");
            entity.Property(e => e.StatusTypeId).HasColumnName("status_type_id");

            entity.HasOne(d => d.StatusType).WithMany(p => p.Statuses)
                .HasForeignKey(d => d.StatusTypeId)
                .HasConstraintName("status_list_item_fk");
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
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("suppliercode");
            entity.Property(e => e.Suppliername)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("suppliername");
            entity.Property(e => e.Updateddate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("updateddate");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users", "master");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.UserId)
                .IsRequired()
                .HasColumnType("character varying")
                .HasColumnName("user_id");
            entity.Property(e => e.UserLastname)
                .HasColumnType("character varying")
                .HasColumnName("user_lastname");
            entity.Property(e => e.UserName)
                .HasColumnType("character varying")
                .HasColumnName("user_name");
            entity.Property(e => e.UserPassword)
                .IsRequired()
                .HasColumnType("character varying")
                .HasColumnName("user_password");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("users_role_fk");

            entity.HasOne(d => d.Section).WithMany(p => p.Users)
                .HasForeignKey(d => d.SectionId)
                .HasConstraintName("users_section_fk");
        });

        modelBuilder.Entity<User1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_unique").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp(0) without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.EmailVerifiedAt)
                .HasColumnType("timestamp(0) without time zone")
                .HasColumnName("email_verified_at");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.RememberToken)
                .HasMaxLength(100)
                .HasColumnName("remember_token");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp(0) without time zone")
                .HasColumnName("updated_at");
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
