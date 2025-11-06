using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SI24004.Models;

public partial class PostgrestContext : DbContext
{
    public PostgrestContext(DbContextOptions<PostgrestContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attachment> Attachments { get; set; }

    public virtual DbSet<AviRequest> AviRequests { get; set; }

    public virtual DbSet<Drawing> Drawings { get; set; }

    public virtual DbSet<DwRequest> DwRequests { get; set; }

    public virtual DbSet<Employeemaster> Employeemasters { get; set; }

    public virtual DbSet<InaRequest> InaRequests { get; set; }

    public virtual DbSet<InventoryTransaction> InventoryTransactions { get; set; }

    public virtual DbSet<IssueStatus> IssueStatuses { get; set; }

    public virtual DbSet<ItemHoldDetail> ItemHoldDetails { get; set; }

    public virtual DbSet<ItemInventoryStat> ItemInventoryStats { get; set; }

    public virtual DbSet<ItemReceiveDetail> ItemReceiveDetails { get; set; }

    public virtual DbSet<ListItem> ListItems { get; set; }

    public virtual DbSet<Locationmaster> Locationmasters { get; set; }

    public virtual DbSet<LotRequest> LotRequests { get; set; }

    public virtual DbSet<MateralInventory> MateralInventories { get; set; }

    public virtual DbSet<MaterialReceiveRecord> MaterialReceiveRecords { get; set; }

    public virtual DbSet<MaterialType> MaterialTypes { get; set; }

    public virtual DbSet<Objective> Objectives { get; set; }

    public virtual DbSet<QaSubstance> QaSubstances { get; set; }

    public virtual DbSet<RegularSubstand> RegularSubstands { get; set; }

    public virtual DbSet<RequestMachine> RequestMachines { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VwIssueSummary> VwIssueSummaries { get; set; }

    public virtual DbSet<VwItemCurrentStatus> VwItemCurrentStatuses { get; set; }

    public virtual DbSet<VwItemDailyMinmax> VwItemDailyMinmaxes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pg_catalog", "adminpack")
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
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("employeename");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Position)
                .HasMaxLength(50)
                .HasColumnName("position");
            entity.Property(e => e.Shift)
                .HasMaxLength(1)
                .HasColumnName("shift");
            entity.Property(e => e.Updateddate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("updateddate");
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
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("locationname");
            entity.Property(e => e.Maxcapacity)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("1000")
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

        modelBuilder.Entity<MateralInventory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("MateralInventories_pkey");

            entity.ToTable("MateralInventories", "LP");

            entity.HasIndex(e => e.Location, "idx_material_inventory_location");

            entity.HasIndex(e => e.MatName, "idx_material_inventory_matname");

            entity.HasIndex(e => e.EmpId, "idx_materialinventories_empid");

            entity.HasIndex(e => e.ExpDate, "idx_materialinventories_expdate");

            entity.HasIndex(e => e.InsertDate, "idx_materialinventories_insertdate");

            entity.HasIndex(e => e.MatName, "idx_materialinventories_matname");

            entity.HasIndex(e => e.MatTypeId, "idx_materialinventories_mattypeid");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.Case).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.InsertDate).HasDefaultValueSql("CURRENT_DATE");
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.LotNumber).HasMaxLength(100);
            entity.Property(e => e.MatName)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Product).HasMaxLength(255);
            entity.Property(e => e.Shift).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<MaterialReceiveRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("MaterialReceiveRecords_pkey");

            entity.ToTable("MaterialReceiveRecords", "LP");

            entity.HasIndex(e => e.InsertDate, "idx_material_receive_records_insertdate");

            entity.HasIndex(e => e.MatName, "idx_material_receive_records_matname");

            entity.HasIndex(e => e.EmpId, "idx_materialreceiverecords_empid");

            entity.HasIndex(e => e.ExpDate, "idx_materialreceiverecords_expdate");

            entity.HasIndex(e => e.InsertDate, "idx_materialreceiverecords_insertdate");

            entity.HasIndex(e => e.MatName, "idx_materialreceiverecords_matname");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.Case).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.InsertDate).HasDefaultValueSql("CURRENT_DATE");
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.LotNumber).HasMaxLength(100);
            entity.Property(e => e.MatName)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Shift).HasMaxLength(20);
            entity.Property(e => e.Status)
                .HasColumnType("character varying")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<MaterialType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("MaterialTypes_pkey");

            entity.ToTable("MaterialTypes", "LP");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.TypeName)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
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

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("rule_pk");

            entity.ToTable("role", "master");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
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
            entity.Property(e => e.ListItemId).HasColumnName("list_item_id");
            entity.Property(e => e.SectionCode)
                .IsRequired()
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SectionName)
                .IsRequired()
                .HasColumnType("character varying")
                .HasColumnName("section_name");

            entity.HasOne(d => d.ListItem).WithMany(p => p.Sections)
                .HasForeignKey(d => d.ListItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("section_list_item_fk");
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

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users", "master");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
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
