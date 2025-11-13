using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SI24004.Models;
using SI24004.ModelsMysql;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace SI24004.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SI24016Controller : ControllerBase
    {
        private readonly SqlServerContext _context;
        private readonly PostgrestContext _postcontext;

        public SI24016Controller(SqlServerContext context, PostgrestContext PostgrestContext)
        {
            _context = context;
            _postcontext = PostgrestContext;
        }

        [HttpGet("inventory/items-report")]
        public async Task<IActionResult> GetItemsReport()
        {
            try
            {
                var oneYearAgo = DateTime.Today.AddYears(-1);

                // Step 1: Get data from MySQL (Master Data) - เฉพาะ 1 ปีล่าสุด
                var mysqlData = await (from issueSlip in _context.IssueSlips
                                       where issueSlip.IssueDate >= oneYearAgo  // เพิ่มเงื่อนไขนี้
                                       join issueItem in _context.IssueItems on issueSlip.IssueId equals issueItem.IssueId
                                       join item in _context.ItemInfos on issueItem.ItemCode equals item.ItemCode
                                       join user in _context.UserInfos on issueSlip.IssueMember equals user.UserId
                                       join section in _context.SectionInfos on user.SectionId equals section.SectionId
                                       select new
                                       {
                                           Section = section.SectionCode ?? "",
                                           SectionId = section.SectionId,
                                           ItemCode = issueItem.ItemCode ?? "",
                                           Item_Name = issueItem.ItemName ?? "",
                                           Min = item != null ? item.ItemMin ?? 0 : 0,
                                           Max = item != null ? item.ItemMax ?? 0 : 0,
                                           Unit = issueItem.ItemUnit ?? "",
                                           subUnitqty = item != null ? item.ItemSubqty ?? "" : "",
                                           SubUnit = item != null ? item.ItemSubUnit ?? "" : "",
                                           Price = item != null ? item.ItemPrice ?? 0 : 0,
                                           Currency = item != null ? item.ItemCurrency ?? "" : "",
                                           IssueDate = issueSlip.IssueDate
                                       })
                                   .ToListAsync();

                // Step 2: Get item codes
                var itemCodes = mysqlData.Select(x => x.ItemCode).Distinct().ToList();

                // Step 3: Get current status from PostgreSQL
                var currentStatus = await _postcontext.VwItemCurrentStatuses
                    .Where(x => itemCodes.Contains(x.ItemCode))
                    .ToListAsync();

                // Step 4: Get today's Min/Max
                var today = DateOnly.FromDateTime(DateTime.Today);
                var todayMinMax = await _postcontext.VwItemDailyMinmaxes
                    .Where(x => itemCodes.Contains(x.ItemCode) && x.TransactionDate == today)
                    .ToListAsync();

                // Step 5: Get issued and received totals
                var issueTotals = await _postcontext.ItemReceiveDetails
                    .Where(x => itemCodes.Contains(x.ItemCode))
                    .GroupBy(x => x.ItemCode)
                    .Select(g => new
                    {
                        ItemCode = g.Key,
                        TotalIssued = g.Sum(x => x.QuantityIssued),
                        TotalReceived = g.Sum(x => x.QuantityReceived)
                    })
                    .ToListAsync();

                // Step 6: Combine data
                var processedData = mysqlData.Select(x =>
                {
                    var status = currentStatus.FirstOrDefault(s => s.ItemCode == x.ItemCode);
                    var minmax = todayMinMax.FirstOrDefault(m => m.ItemCode == x.ItemCode);
                    var totals = issueTotals.FirstOrDefault(t => t.ItemCode == x.ItemCode);

                    var currentBalance = status?.CurrentBalance ?? 0;
                    var totalHold = status?.TotalHold ?? 0;
                    var available = status?.AvailableQuantity ?? 0;
                    var planUsage = totals?.TotalIssued ?? 0;
                    var actualUsage = totals?.TotalReceived ?? 0;
                    var diff = actualUsage - planUsage;

                    return new
                    {
                        x.Section,
                        x.SectionId,
                        x.ItemCode,
                        x.Item_Name,
                        x.Min,
                        x.Max,
                        CurrentBalance = currentBalance,
                        HoldItem = totalHold,
                        Available = available,
                        x.Unit,
                        x.subUnitqty,
                        x.SubUnit,
                        TodayMin = minmax?.MinQuantity ?? 0,
                        TodayMax = minmax?.MaxQuantity ?? 0,
                        x.Price,
                        x.Currency,
                        PlanUsage = planUsage,
                        ActualUsage = actualUsage,
                        Diff = diff,
                        x.IssueDate,
                        Request = diff < 0 ? "Under plan" :
                                 diff > 0 ? "Over plan" :
                                 "As planned",
                        LastUpdate = status?.LastTransactionDate
                    };
                })
                .ToList();

                var response = new
                {
                    Headers = new[]
                    {
                        new { key = "Section", label = "แผนก", type = "text", sortable = true },
                        new { key = "ItemCode", label = "รหัสสินค้า", type = "text", sortable = true },
                        new { key = "Item_Name", label = "ชื่อสินค้า", type = "text", sortable = true },
                        new { key = "Min", label = "ขั้นต่ำ", type = "number", sortable = true },
                        new { key = "Max", label = "สูงสุด", type = "number", sortable = true },
                        new { key = "CurrentBalance", label = "คงเหลือ", type = "number", sortable = true },
                        new { key = "HoldItem", label = "จอง", type = "number", sortable = true },
                        new { key = "Available", label = "พร้อมใช้", type = "number", sortable = true },
                        new { key = "Unit", label = "หน่วย", type = "text", sortable = false },
                        new { key = "TodayMin", label = "ต่ำสุดวันนี้", type = "number", sortable = true },
                        new { key = "TodayMax", label = "สูงสุดวันนี้", type = "number", sortable = true },
                        new { key = "Price", label = "ราคา", type = "currency", sortable = true },
                        new { key = "PlanUsage", label = "แผนการใช้", type = "number", sortable = true },
                        new { key = "ActualUsage", label = "ใช้จริง", type = "number", sortable = true },
                        new { key = "Diff", label = "ผลต่าง", type = "number", sortable = true },
                        new { key = "Request", label = "สถานะ", type = "text", sortable = true },
                        new { key = "LastUpdate", label = "อัปเดต", type = "datetime", sortable = true }
                    },
                    Data = processedData,
                    Summary = new
                    {
                        TotalItems = processedData.Count,
                        TotalValue = processedData.Sum(x => x.Price * x.CurrentBalance),
                        TotalBalance = processedData.Sum(x => x.CurrentBalance),
                        TotalHold = processedData.Sum(x => x.HoldItem),
                        TotalAvailable = processedData.Sum(x => x.Available),
                        UnderPlan = processedData.Count(x => x.Request == "Under plan"),
                        OverPlan = processedData.Count(x => x.Request == "Over plan"),
                        AsPlanned = processedData.Count(x => x.Request == "As planned")
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
        [HttpPost("barcode/scan")]
        public async Task<IActionResult> ScanBarcode([FromBody] BarcodeRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Barcode))
                {
                    return BadRequest(new { success = false, message = "Barcode is required" });
                }
                var barcode = request.Barcode.Trim();

                // Check status from PostgreSQL first
                var issueStatus = await _postcontext.IssueStatuses
                    .Where(i => i.IssueId == barcode)
                    .FirstOrDefaultAsync();

                string status = issueStatus?.CurrentStatus ?? "pending";

                // If status is "received" or "hold", get data from PostgreSQL
                if (status == "received")
                {
                    // Get updated data from ItemReceiveDetails (PostgreSQL)
                    var receivedItems = await _postcontext.ItemReceiveDetails
                        .Where(r => r.IssueId == barcode)
                        .Select(r => new
                        {
                            Section = r.ProcessId ?? "",
                            ItemCode = r.ItemCode ?? "",
                            ItemName = r.ItemName ?? "",
                            LotNumber = r.LotNumber ?? "",
                            QuantityIssued = r.QuantityIssued ?? 0,  // Original from IssueSlip
                            QuantityReceived = r.QuantityReceived ?? 0,  // Current received quantity
                            Unit = r.Unit ?? "",
                            SubunitQty = r.SubunitQty ?? "",
                            Subunit = r.Subunit ?? "",
                            Status = status,
                            ReceivedDate = r.ReceivedDate,
                            ReceivedBy = r.ReceivedBy
                        })
                        .ToListAsync();

                    // Get current balance from inventory for each item
                    var itemCodes = receivedItems.Select(r => r.ItemCode).Distinct().ToList();
                    var currentBalances = await _postcontext.VwItemCurrentStatuses
                        .Where(v => itemCodes.Contains(v.ItemCode))
                        .ToDictionaryAsync(v => v.ItemCode, v => v.CurrentBalance ?? 0);

                    var receivedItemsWithBalance = receivedItems.Select(r =>
                    {
                        var balanceInStock = currentBalances.ContainsKey(r.ItemCode) ? currentBalances[r.ItemCode] : 0;
                        // Current Qty = Original - (Balance - Received)
                        // ???? Current Qty = Received (????????????????????????????)
                        var currentQty = r.QuantityReceived; // ????????????????

                        return new
                        {
                            r.Section,
                            r.ItemCode,
                            r.ItemName,
                            r.LotNumber,
                            OriginalQuantity = r.QuantityIssued,  // ????????????????
                            CurrentQuantity = currentQty,  // ?????????????/?????
                            BalanceInStock = balanceInStock,  // ?????????????
                            r.Unit,
                            r.SubunitQty,
                            r.Subunit,
                            r.Status,
                            r.ReceivedDate,
                            r.ReceivedBy
                        };
                    }).ToList();

                    if (receivedItemsWithBalance == null || !receivedItemsWithBalance.Any())
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = $"Received items not found for barcode {barcode}"
                        });
                    }

                    var response = new
                    {
                        success = true,
                        message = "Data found successfully",
                        status = status,
                        headers = new[]
                        {
                    new { key = "Section", label = "Section", type = "text", sortable = true },
                    new { key = "ItemCode", label = "Item Code", type = "text", sortable = true },
                    new { key = "ItemName", label = "Item Name", type = "text", sortable = true },
                    new { key = "LotNumber", label = "Lot Number", type = "text", sortable = true },
                    new { key = "OriginalQuantity", label = "Original Qty", type = "number", sortable = true },
                    new { key = "CurrentQuantity", label = "Current Qty", type = "number", sortable = true },
                    new { key = "BalanceInStock", label = "Balance in Stock", type = "number", sortable = true },
                    new { key = "Unit", label = "Unit", type = "text", sortable = false },
                    new { key = "SubunitQty", label = "Sub-unit Qty", type = "number", sortable = true },
                    new { key = "Subunit", label = "Sub-unit", type = "text", sortable = false },
                    new { key = "Status", label = "Status", type = "text", sortable = true }
                },
                        data = receivedItemsWithBalance
                    };

                    return Ok(response);
                }
                else if (status == "hold")
                {
                    // Get data from ItemHoldDetails (PostgreSQL)
                    var heldItems = await _postcontext.ItemHoldDetails
                        .Where(h => h.IssueId == barcode && h.HoldStatus == "active")
                        .Select(h => new
                        {
                            Section = "", // You may need to join with other tables to get section
                            ItemCode = h.ItemCode ?? "",
                            ItemName = h.ItemName ?? "",
                            LotNumber = h.LotNumber ?? "",
                            QuantityHold = h.QuantityHold ?? 0,
                            QuantityWH = h.QuantityHold.ToString() ?? "0",
                            Unit = h.Unit ?? "",
                            SubunitQty = h.SubunitQty ?? "",
                            Subunit = h.Subunit ?? "",
                            Status = status,
                            HoldDate = h.HoldDate,
                            HoldUntil = h.HoldUntil
                        })
                        .ToListAsync();

                    if (heldItems == null || !heldItems.Any())
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = $"Hold items not found for barcode {barcode}"
                        });
                    }

                    var response = new
                    {
                        success = true,
                        message = "Data found successfully",
                        status = status,
                        headers = new[]
                        {
                    new { key = "Section", label = "Section", type = "text", sortable = true },
                    new { key = "ItemCode", label = "Item Code", type = "text", sortable = true },
                    new { key = "ItemName", label = "Item Name", type = "text", sortable = true },
                    new { key = "LotNumber", label = "Lot Number", type = "text", sortable = true },
                    new { key = "QuantityWH", label = "Quantity", type = "number", sortable = true },
                    new { key = "Unit", label = "Unit", type = "text", sortable = false },
                    new { key = "SubunitQty", label = "Sub-unit Qty", type = "number", sortable = true },
                    new { key = "Subunit", label = "Sub-unit", type = "text", sortable = false },
                    new { key = "Status", label = "Status", type = "text", sortable = true }
                },
                        data = heldItems
                    };

                    return Ok(response);
                }
                else
                {
                    // For "pending" status, get data from MySQL
                    var items = await (from issueItem in _context.IssueItems
                                       join issueSlip in _context.IssueSlips on issueItem.IssueId equals issueSlip.IssueId
                                       join itemInfo in _context.ItemInfos on issueItem.ItemCode equals itemInfo.ItemCode into itemGroup
                                       join user in _context.UserInfos on issueSlip.IssueMember equals user.UserId
                                       join section in _context.SectionInfos on user.SectionId equals section.SectionId
                                       from itemInfo in itemGroup.DefaultIfEmpty()
                                       where issueItem.IssueId == barcode
                                       select new
                                       {
                                           Section = section.SectionName,
                                           ItemCode = issueItem.ItemCode,
                                           ItemName = issueItem.ItemName,
                                           LotNumber = issueItem.LotNumber,
                                           QuantityWH = issueItem.ItemQty ?? "0",
                                           Unit = itemInfo != null ? itemInfo.ItemUnit : "",
                                           SubunitQty = itemInfo != null ? itemInfo.ItemSubqty : "",
                                           Subunit = itemInfo != null ? itemInfo.ItemSubUnit : "",
                                       })
                          .ToListAsync();

                    if (items == null || !items.Any())
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = $"Data not found for barcode {barcode}"
                        });
                    }

                    var itemsWithStatus = items.Select(item => new
                    {
                        item.Section,
                        item.ItemCode,
                        item.ItemName,
                        item.LotNumber,
                        item.QuantityWH,
                        item.Unit,
                        item.SubunitQty,
                        item.Subunit,
                        Status = status
                    }).ToList();

                    var response = new
                    {
                        success = true,
                        message = "Data found successfully",
                        status = status,
                        headers = new[]
                        {
                    new { key = "Section", label = "Section", type = "text", sortable = true },
                    new { key = "ItemCode", label = "Item Code", type = "text", sortable = true },
                    new { key = "ItemName", label = "Item Name", type = "text", sortable = true },
                    new { key = "LotNumber", label = "Lot Number", type = "text", sortable = true },
                    new { key = "QuantityHold", label = "Hold Quantity", type = "number", sortable = true },
                    new { key = "Unit", label = "Unit", type = "text", sortable = false },
                    new { key = "SubunitQty", label = "Sub-unit Qty", type = "number", sortable = true },
                    new { key = "Subunit", label = "Sub-unit", type = "text", sortable = false },
                    new { key = "Status", label = "Status", type = "text", sortable = true },
                    new { key = "HoldUntil", label = "Hold Until", type = "datetime", sortable = true }
                },
                        data = itemsWithStatus
                    };

                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred",
                    error = ex.Message
                });
            }
        }

        [HttpPost("barcode/receive")]
        public async Task<IActionResult> ReceiveBarcode([FromBody] BarcodeDataRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Barcode))
                {
                    return BadRequest(new { success = false, message = "กรุณาระบุ barcode" });
                }

                if (request.Items == null || !request.Items.Any())
                {
                    return BadRequest(new { success = false, message = "กรุณาระบุข้อมูลสินค้า" });
                }

                var barcode = request.Barcode.Trim();
                var userId = request.UserId ?? "system";

                // Use execution strategy for retry-able transactions
                var strategy = _postcontext.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _postcontext.Database.BeginTransactionAsync();
                    try
                    {
                        // Check if already received
                        var issueStatus = await _postcontext.IssueStatuses
                            .FirstOrDefaultAsync(i => i.IssueId == barcode);

                        if (issueStatus?.CurrentStatus == "received")
                        {
                            throw new InvalidOperationException("ข้อมูลนี้ได้รับแล้ว");
                        }

                        // Create or update issue status
                        if (issueStatus == null)
                        {
                            issueStatus = new IssueStatus
                            {
                                Id = Guid.NewGuid(),
                                IssueId = barcode,
                                CurrentStatus = "received",
                                ReceivedDate = DateTime.Now,
                                TotalItems = request.Items.Count,
                                ReceivedItems = request.Items.Count,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now,
                                UpdatedBy = userId
                            };
                            _postcontext.IssueStatuses.Add(issueStatus);
                        }
                        else
                        {
                            // Update status history
                            var history = new List<object>();
                            if (!string.IsNullOrEmpty(issueStatus.StatusHistory))
                            {
                                history = JsonSerializer.Deserialize<List<object>>(issueStatus.StatusHistory) ?? new List<object>();
                            }
                            history.Add(new
                            {
                                from = issueStatus.CurrentStatus,
                                to = "received",
                                date = DateTime.Now,
                                by = userId
                            });

                            issueStatus.CurrentStatus = "received";
                            issueStatus.ReceivedDate = DateTime.Now;
                            issueStatus.StatusHistory = JsonSerializer.Serialize(history);
                            issueStatus.UpdatedAt = DateTime.Now;
                            issueStatus.UpdatedBy = userId;
                        }

                        // Cancel existing hold items
                        var holdItems = await _postcontext.ItemHoldDetails
                            .Where(h => h.IssueId == barcode && h.HoldStatus == "active")
                            .ToListAsync();

                        foreach (var hold in holdItems)
                        {
                            hold.HoldStatus = "converted";
                            hold.ReleasedDate = DateTime.Now;
                            hold.ReleasedBy = userId;
                        }

                        // Process each item
                        foreach (var item in request.Items)
                        {
                            var quantity = ConvertToDecimal(item.QuantityWH);

                            // Create or update receive details
                            var receiveDetail = await _postcontext.ItemReceiveDetails
                                .FirstOrDefaultAsync(r => r.IssueId == barcode && r.ItemCode == item.ItemCode);

                            if (receiveDetail == null)
                            {
                                receiveDetail = new ItemReceiveDetail
                                {
                                    Id = Guid.NewGuid(),
                                    IssueId = barcode,
                                    ItemCode = item.ItemCode,
                                    ItemName = item.ItemName,
                                    LotNumber = item.LotNumber,
                                    Unit = item.Unit,
                                    QuantityIssued = quantity,
                                    QuantityReceived = quantity,
                                    SubunitQty = item.SubunitQty,
                                    Subunit = item.Subunit,
                                    ReceiveStatus = "completed",
                                    ReceivedDate = DateTime.Now,
                                    ReceivedBy = userId,
                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                };
                                _postcontext.ItemReceiveDetails.Add(receiveDetail);
                            }
                            else
                            {
                                receiveDetail.QuantityReceived = quantity;
                                receiveDetail.ReceiveStatus = "completed";
                                receiveDetail.ReceivedDate = DateTime.Now;
                                receiveDetail.ReceivedBy = userId;
                                receiveDetail.UpdatedAt = DateTime.Now;
                            }

                            // Create inventory transaction (IN)
                            var transactionId = $"RCV-{barcode}-{item.ItemCode}-{DateTime.Now:yyyyMMddHHmmss}";
                            await CreateInventoryTransaction(
                                transactionId,
                                item.ItemCode,
                                item.ItemName,
                                item.LotNumber,
                                "IN",
                                "RECEIVE",
                                quantity,
                                item.Unit,
                                barcode,
                                barcode,
                                "RECEIVE",
                                null,
                                item.Section,
                                null,
                                userId,
                                "Received from issue"
                            );
                        }

                        await _postcontext.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                return Ok(new { success = true, message = "รับสินค้าเรียบร้อยแล้ว" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการรับสินค้า",
                    error = ex.Message
                });
            }
        }

        [HttpPost("barcode/hold")]
        public async Task<IActionResult> HoldBarcode([FromBody] BarcodeDataRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Barcode))
                {
                    return BadRequest(new { success = false, message = "กรุณาระบุ barcode" });
                }

                if (request.Items == null || !request.Items.Any())
                {
                    return BadRequest(new { success = false, message = "กรุณาระบุข้อมูลสินค้า" });
                }

                var barcode = request.Barcode.Trim();
                var userId = request.UserId ?? "system";

                // Use execution strategy for retry-able transactions
                var strategy = _postcontext.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _postcontext.Database.BeginTransactionAsync();
                    try
                    {
                        // Check if already received
                        var issueStatus = await _postcontext.IssueStatuses
                            .FirstOrDefaultAsync(i => i.IssueId == barcode);

                        if (issueStatus?.CurrentStatus == "received")
                        {
                            throw new InvalidOperationException("ไม่สามารถ Hold ได้เนื่องจากได้รับสินค้าแล้ว");
                        }

                        // Create or update issue status
                        if (issueStatus == null)
                        {
                            issueStatus = new IssueStatus
                            {
                                Id = Guid.NewGuid(),
                                IssueId = barcode,
                                CurrentStatus = "hold",
                                HoldDate = DateTime.Now,
                                TotalItems = request.Items.Count,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now,
                                UpdatedBy = userId
                            };
                            _postcontext.IssueStatuses.Add(issueStatus);
                        }
                        else
                        {
                            // Update status history
                            var history = new List<object>();
                            if (!string.IsNullOrEmpty(issueStatus.StatusHistory))
                            {
                                history = JsonSerializer.Deserialize<List<object>>(issueStatus.StatusHistory) ?? new List<object>();
                            }
                            history.Add(new
                            {
                                from = issueStatus.CurrentStatus,
                                to = "hold",
                                date = DateTime.Now,
                                by = userId
                            });

                            issueStatus.CurrentStatus = "hold";
                            issueStatus.HoldDate = DateTime.Now;
                            issueStatus.StatusHistory = JsonSerializer.Serialize(history);
                            issueStatus.UpdatedAt = DateTime.Now;
                            issueStatus.UpdatedBy = userId;
                        }

                        // Cancel existing hold items
                        var existingHold = await _postcontext.ItemHoldDetails
                            .Where(h => h.IssueId == barcode && h.HoldStatus == "active")
                            .ToListAsync();

                        foreach (var hold in existingHold)
                        {
                            hold.HoldStatus = "cancelled";
                            hold.ReleasedDate = DateTime.Now;
                            hold.ReleasedBy = userId;
                        }

                        // Add new hold items
                        foreach (var item in request.Items)
                        {
                            var quantity = ConvertToDecimal(item.QuantityWH);

                            var holdDetail = new ItemHoldDetail
                            {
                                Id = Guid.NewGuid(),
                                IssueId = barcode,
                                ItemCode = item.ItemCode,
                                ItemName = item.ItemName,
                                LotNumber = item.LotNumber,
                                Unit = item.Unit,
                                QuantityHold = quantity,
                                SubunitQty = item.SubunitQty,
                                Subunit = item.Subunit,
                                HoldStatus = "active",
                                HoldDate = DateTime.Now,
                                HoldUntil = DateTime.Now.AddDays(7),
                                HoldBy = userId,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            };
                            _postcontext.ItemHoldDetails.Add(holdDetail);

                            // Create inventory transaction (HOLD)
                            var transactionId = $"HLD-{barcode}-{item.ItemCode}-{DateTime.Now:yyyyMMddHHmmss}";
                            await CreateInventoryTransaction(
                                transactionId,
                                item.ItemCode,
                                item.ItemName,
                                item.LotNumber,
                                "HOLD",
                                "RESERVE",
                                quantity,
                                item.Unit,
                                barcode,
                                barcode,
                                "HOLD",
                                null,
                                item.Section,
                                null,
                                userId,
                                "Hold for future use"
                            );
                        }

                        await _postcontext.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                return Ok(new { success = true, message = "Hold สินค้าเรียบร้อยแล้ว" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการ Hold สินค้า",
                    error = ex.Message
                });
            }
        }

        [HttpPost("barcode/update")]
        public async Task<IActionResult> UpdateBarcode([FromBody] BarcodeDataRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Barcode))
                {
                    return BadRequest(new { success = false, message = "กรุณาระบุ barcode" });
                }

                if (request.Items == null || !request.Items.Any())
                {
                    return BadRequest(new { success = false, message = "กรุณาระบุข้อมูลสินค้า" });
                }

                var barcode = request.Barcode.Trim();
                var userId = request.UserId ?? "system";

                // Use execution strategy for retry-able transactions
                var strategy = _postcontext.Database.CreateExecutionStrategy();

                var result = await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _postcontext.Database.BeginTransactionAsync();
                    try
                    {
                        // Check if issue status exists and is received
                        var issueStatus = await _postcontext.IssueStatuses
                            .FirstOrDefaultAsync(i => i.IssueId == barcode);

                        if (issueStatus == null)
                        {
                            throw new InvalidOperationException("ไม่พบข้อมูลการเบิกสินค้า");
                        }

                        if (issueStatus.CurrentStatus != "received")
                        {
                            throw new InvalidOperationException($"ไม่สามารถแก้ไขได้ สถานะปัจจุบัน: {issueStatus.CurrentStatus}");
                        }

                        // Get all existing receive details for this barcode
                        var existingReceives = await _postcontext.ItemReceiveDetails
                            .Where(r => r.IssueId == barcode)
                            .ToListAsync();

                        if (!existingReceives.Any())
                        {
                            throw new InvalidOperationException("ไม่พบข้อมูลการรับสินค้า");
                        }

                        var updatedCount = 0;

                        // Update each item
                        foreach (var item in request.Items)
                        {
                            var receiveDetail = existingReceives
                                .FirstOrDefault(r => r.ItemCode == item.ItemCode);

                            if (receiveDetail == null)
                            {
                                // Item not found in receive details, skip
                                continue;
                            }

                            var oldQuantity = receiveDetail.QuantityReceived ?? 0;
                            var newQuantity = ConvertToDecimal(item.QuantityWH);
                            var adjustmentQty = newQuantity - oldQuantity;

                            // Check if ANY field has changed
                            var hasQuantityChange = Math.Abs(adjustmentQty) > 0.001m; // Use threshold for decimal comparison
                            var hasSubunitQtyChange = receiveDetail.SubunitQty != item.SubunitQty;
                            var hasSubunitChange = receiveDetail.Subunit != item.Subunit;
                            var hasUnitChange = receiveDetail.Unit != item.Unit;
                            var hasSectionChange = receiveDetail.ProcessId != item.Section;

                            // Update if any field has changed
                            if (hasQuantityChange || hasSubunitQtyChange || hasSubunitChange || hasUnitChange || hasSectionChange)
                            {
                                // Update receive detail
                                receiveDetail.QuantityReceived = newQuantity;
                                receiveDetail.SubunitQty = item.SubunitQty;
                                receiveDetail.Subunit = item.Subunit;
                                receiveDetail.Unit = item.Unit;
                                receiveDetail.ProcessId = item.Section;
                                receiveDetail.ItemName = item.ItemName ?? receiveDetail.ItemName;
                                receiveDetail.LotNumber = item.LotNumber ?? receiveDetail.LotNumber;
                                receiveDetail.ReceivedDate = DateTime.Now;
                                receiveDetail.ReceivedBy = userId;
                                receiveDetail.UpdatedAt = DateTime.Now;

                                // Create adjustment transaction ONLY if quantity changed
                                if (hasQuantityChange)
                                {
                                    var transactionId = $"ADJ-{barcode}-{item.ItemCode}-{DateTime.Now:yyyyMMddHHmmss}";
                                    var transType = adjustmentQty > 0 ? "IN" : "OUT";
                                    var absQuantity = Math.Abs(adjustmentQty);

                                    await CreateInventoryTransaction(
                                        transactionId,
                                        item.ItemCode,
                                        item.ItemName ?? receiveDetail.ItemName ?? string.Empty,
                                        item.LotNumber ?? receiveDetail.LotNumber ?? string.Empty,
                                        transType,
                                        "ADJUST",
                                        absQuantity,
                                        item.Unit ?? receiveDetail.Unit ?? string.Empty,
                                        barcode,
                                        barcode,
                                        "RECEIVE_UPDATE",
                                        null,
                                        item.Section ?? string.Empty,
                                        null,
                                        userId,
                                        $"Adjusted from {oldQuantity} to {newQuantity} (diff: {adjustmentQty:+0.##;-0.##})"
                                    );
                                }

                                updatedCount++;
                            }
                        }

                        if (updatedCount == 0)
                        {
                            throw new InvalidOperationException("ไม่มีข้อมูลที่ต้องอัพเดท");
                        }

                        // Update issue status
                        issueStatus.UpdatedAt = DateTime.Now;
                        issueStatus.UpdatedBy = userId;

                        // Update status history
                        var history = new List<object>();
                        if (!string.IsNullOrEmpty(issueStatus.StatusHistory))
                        {
                            history = JsonSerializer.Deserialize<List<object>>(issueStatus.StatusHistory)
                                ?? new List<object>();
                        }
                        history.Add(new
                        {
                            action = "update_quantities",
                            date = DateTime.Now,
                            by = userId,
                            itemsUpdated = updatedCount
                        });
                        issueStatus.StatusHistory = JsonSerializer.Serialize(history);

                        await _postcontext.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return updatedCount;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                return Ok(new { success = true, message = "อัพเดทข้อมูลเรียบร้อยแล้ว", itemsUpdated = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการอัพเดทข้อมูล",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("inventory/item-history/{itemCode}")]
        public async Task<IActionResult> GetItemHistory(string itemCode,[FromQuery] DateTime? startDate,[FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today.AddDays(1);

                var transactions = await _postcontext.InventoryTransactions
                    .Where(t => t.ItemCode == itemCode &&
                           t.TransactionDate >= start &&
                           t.TransactionDate < end)
                    .OrderBy(t => t.TransactionDate)
                    .ThenBy(t => t.DailySequence)
                    .Select(t => new
                    {
                        t.TransactionId,
                        t.TransactionType,
                        t.TransactionSubtype,
                        t.Quantity,
                        t.Unit,
                        t.BalanceAfter,
                        t.IssueId,
                        t.ReferenceId,
                        t.TransactionDate,
                        t.PerformedBy,
                        t.Remarks
                    })
                    .ToListAsync();

                // Get daily stats
                var startDate2 = DateOnly.FromDateTime(start);
                var endDate2 = DateOnly.FromDateTime(end);

                var dailyStats = await _postcontext.VwItemDailyMinmaxes
                    .Where(v => v.ItemCode == itemCode &&
                           v.TransactionDate >= startDate2 &&
                           v.TransactionDate <= endDate2)
                    .OrderBy(v => v.TransactionDate)
                    .Select(d => new
                    {
                        date = d.TransactionDate,
                        d.MinQuantity,
                        d.MaxQuantity,
                        d.AvgQuantity,
                        d.OpeningBalance,
                        d.ClosingBalance,
                        d.TransactionCount
                    })
                    .ToListAsync();

                return Ok(new
                {
                    itemCode,
                    period = new { start, end },
                    transactions,
                    dailyStats
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาด",
                    error = ex.Message
                });
            }
        }
        // Helper method to convert string to decimal
        // เพิ่ม endpoints เหล่านี้ใน SI24016Controller

        // 1. Report งานเข้า (Receive Report)
        [HttpGet("reports/receive")]
        public async Task<IActionResult> GetReceiveReport(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? itemCode,
            [FromQuery] string? sectionId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today.AddDays(1);

                var query = _postcontext.ItemReceiveDetails.AsQueryable();

                // Filters
                if (!string.IsNullOrWhiteSpace(itemCode))
                    query = query.Where(r => r.ItemCode.Contains(itemCode));

                if (!string.IsNullOrWhiteSpace(sectionId))
                    query = query.Where(r => r.ProcessId == sectionId);

                query = query.Where(r => r.ReceivedDate >= start && r.ReceivedDate < end);

                // Get total count before pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var receiveData = await query
                    .OrderByDescending(r => r.ReceivedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new
                    {
                        r.IssueId,
                        r.ItemCode,
                        r.ItemName,
                        r.LotNumber,
                        QuantityIssued = r.QuantityIssued ?? 0,
                        QuantityReceived = r.QuantityReceived ?? 0,
                        Difference = (r.QuantityReceived ?? 0) - (r.QuantityIssued ?? 0),
                        r.Unit,
                        r.SubunitQty,
                        r.Subunit,
                        Section = r.ProcessId ?? "",
                        r.ReceiveStatus,
                        r.ReceivedDate,
                        r.ReceivedBy,
                        r.Remarks
                    })
                    .ToListAsync();

                // Get summary from all data (not paginated)
                var summaryQuery = _postcontext.ItemReceiveDetails
                    .Where(r => r.ReceivedDate >= start && r.ReceivedDate < end);

                if (!string.IsNullOrWhiteSpace(itemCode))
                    summaryQuery = summaryQuery.Where(r => r.ItemCode.Contains(itemCode));

                if (!string.IsNullOrWhiteSpace(sectionId))
                    summaryQuery = summaryQuery.Where(r => r.ProcessId == sectionId);

                var summaryData = await summaryQuery.ToListAsync();

                var response = new
                {
                    Headers = new[]
                    {
                        new { key = "ReceivedDate", label = "วันที่รับ", type = "datetime", sortable = true },
                        new { key = "IssueId", label = "เลขที่เบิก", type = "text", sortable = true },
                        new { key = "ItemCode", label = "รหัสสินค้า", type = "text", sortable = true },
                        new { key = "ItemName", label = "ชื่อสินค้า", type = "text", sortable = true },
                        new { key = "LotNumber", label = "Lot Number", type = "text", sortable = true },
                        new { key = "QuantityIssued", label = "จำนวนเบิก", type = "number", sortable = true },
                        new { key = "QuantityReceived", label = "จำนวนรับ", type = "number", sortable = true },
                        new { key = "Difference", label = "ผลต่าง", type = "number", sortable = true },
                        new { key = "Unit", label = "หน่วย", type = "text", sortable = false },
                        new { key = "Section", label = "แผนก", type = "text", sortable = true },
                        new { key = "ReceiveStatus", label = "สถานะ", type = "text", sortable = true },
                        new { key = "ReceivedBy", label = "ผู้รับ", type = "text", sortable = true }
                    },
                    Data = receiveData,
                    Pagination = new
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                        HasPrevious = page > 1,
                        HasNext = page < (int)Math.Ceiling(totalCount / (double)pageSize)
                    },
                    Summary = new
                    {
                        TotalRecords = summaryData.Count,
                        TotalIssued = summaryData.Sum(x => x.QuantityIssued ?? 0),
                        TotalReceived = summaryData.Sum(x => x.QuantityReceived ?? 0),
                        TotalDifference = summaryData.Sum(x => (x.QuantityReceived ?? 0) - (x.QuantityIssued ?? 0)),
                        CompletedCount = summaryData.Count(x => x.ReceiveStatus == "completed"),
                        PendingCount = summaryData.Count(x => x.ReceiveStatus == "pending")
                    },
                    Period = new { Start = start, End = end }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาด",
                    error = ex.Message
                });
            }
        }

        // 2. Report งาน Hold
        [HttpGet("reports/hold")]
        public async Task<IActionResult> GetHoldReport(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? itemCode,
            [FromQuery] string? holdStatus,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today.AddDays(1);
                var now = DateTime.Now;

                var query = _postcontext.ItemHoldDetails.AsQueryable();

                // Filters
                if (!string.IsNullOrWhiteSpace(itemCode))
                    query = query.Where(h => h.ItemCode.Contains(itemCode));

                if (!string.IsNullOrWhiteSpace(holdStatus))
                    query = query.Where(h => h.HoldStatus == holdStatus);
                else
                    query = query.Where(h => h.HoldStatus == "active");

                query = query.Where(h => h.HoldDate >= start && h.HoldDate < end);

                // Get total count
                var totalCount = await query.CountAsync();

                // Get paginated data
                var holdDataRaw = await query
                    .OrderByDescending(h => h.HoldDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(h => new
                    {
                        h.IssueId,
                        h.ItemCode,
                        h.ItemName,
                        h.LotNumber,
                        QuantityHold = h.QuantityHold ?? 0,
                        h.Unit,
                        h.SubunitQty,
                        h.Subunit,
                        h.HoldStatus,
                        h.HoldDate,
                        h.HoldUntil,
                        h.HoldBy,
                        h.ReleasedDate,
                        h.ReleasedBy,
                        h.ProcessId,
                        h.Remarks
                    })
                    .ToListAsync();

                var holdData = holdDataRaw.Select(h => new
                {
                    h.IssueId,
                    h.ItemCode,
                    h.ItemName,
                    h.LotNumber,
                    h.QuantityHold,
                    h.Unit,
                    h.SubunitQty,
                    h.Subunit,
                    h.HoldStatus,
                    h.HoldDate,
                    h.HoldUntil,
                    h.HoldBy,
                    h.ReleasedDate,
                    h.ReleasedBy,
                    h.ProcessId,
                    DaysHeld = 0,
                    IsExpired = h.HoldUntil.HasValue && h.HoldUntil.Value < now && h.HoldStatus == "active",
                    h.Remarks
                }).ToList();

                // Get summary from all data
                var summaryQuery = _postcontext.ItemHoldDetails
                    .Where(h => h.HoldDate >= start && h.HoldDate < end);

                if (!string.IsNullOrWhiteSpace(itemCode))
                    summaryQuery = summaryQuery.Where(h => h.ItemCode.Contains(itemCode));

                if (!string.IsNullOrWhiteSpace(holdStatus))
                    summaryQuery = summaryQuery.Where(h => h.HoldStatus == holdStatus);
                else
                    summaryQuery = summaryQuery.Where(h => h.HoldStatus == "active");

                var summaryData = await summaryQuery.ToListAsync();

                var response = new
                {
                    Headers = new[]
                    {
                new { key = "HoldDate", label = "วันที่จอง", type = "datetime", sortable = true },
                new { key = "IssueId", label = "เลขที่เบิก", type = "text", sortable = true },
                new { key = "ItemCode", label = "รหัสสินค้า", type = "text", sortable = true },
                new { key = "ItemName", label = "ชื่อสินค้า", type = "text", sortable = true },
                new { key = "LotNumber", label = "Lot Number", type = "text", sortable = true },
                new { key = "QuantityHold", label = "จำนวนจอง", type = "number", sortable = true },
                new { key = "Unit", label = "หน่วย", type = "text", sortable = false },
                new { key = "HoldStatus", label = "สถานะ", type = "text", sortable = true },
                new { key = "HoldUntil", label = "จองถึง", type = "datetime", sortable = true },
                new { key = "DaysHeld", label = "จำนวนวัน", type = "number", sortable = true },
                new { key = "IsExpired", label = "หมดอายุ", type = "boolean", sortable = true },
                new { key = "HoldBy", label = "ผู้จอง", type = "text", sortable = true }
            },
                    Data = holdData,
                    Pagination = new
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                        HasPrevious = page > 1,
                        HasNext = page < (int)Math.Ceiling(totalCount / (double)pageSize)
                    },
                    Summary = new
                    {
                        TotalRecords = summaryData.Count,
                        TotalQuantityHold = summaryData.Sum(x => x.QuantityHold ?? 0),
                        ActiveCount = summaryData.Count(x => x.HoldStatus == "active"),
                        ReleasedCount = summaryData.Count(x => x.HoldStatus == "released"),
                        CancelledCount = summaryData.Count(x => x.HoldStatus == "cancelled"),
                        ConvertedCount = summaryData.Count(x => x.HoldStatus == "converted"),
                        ExpiredCount = summaryData.Count(x => x.HoldUntil.HasValue && x.HoldUntil.Value < now && x.HoldStatus == "active")
                    },
                    Period = new { Start = start, End = end }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาด",
                    error = ex.Message
                });
            }
        }
        // 3. Report การใช้งาน (Usage Report)
        [HttpGet("reports/usage")]
        public async Task<IActionResult> GetUsageReport(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? itemCode,
            [FromQuery] string? transactionType,
            [FromQuery] string? sectionId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today.AddDays(1);

                var query = _postcontext.InventoryTransactions.AsQueryable();

                // Filters
                if (!string.IsNullOrWhiteSpace(itemCode))
                    query = query.Where(t => t.ItemCode.Contains(itemCode));

                if (!string.IsNullOrWhiteSpace(transactionType))
                    query = query.Where(t => t.TransactionType == transactionType);

                if (!string.IsNullOrWhiteSpace(sectionId))
                    query = query.Where(t => t.SectionId == sectionId);

                query = query.Where(t => t.TransactionDate >= start && t.TransactionDate < end);

                // Get total count
                var totalCount = await query.CountAsync();

                // Get paginated transactions
                var transactions = await query
                    .OrderByDescending(t => t.TransactionDate)
                    .ThenByDescending(t => t.DailySequence)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new
                    {
                        t.TransactionId,
                        t.TransactionDate,
                        t.ItemCode,
                        t.ItemName,
                        t.LotNumber,
                        t.TransactionType,
                        t.TransactionSubtype,
                        Quantity = t.Quantity,
                        t.Unit,
                        BalanceAfter = t.BalanceAfter ?? 0,
                        t.IssueId,
                        t.ReferenceId,
                        t.ReferenceType,
                        t.SectionId,
                        t.SectionName,
                        t.ProcessId,
                        t.PerformedBy,
                        t.DailySequence,
                        t.Remarks
                    })
                    .ToListAsync();

                // Get summary from all data
                var allTransactions = await _postcontext.InventoryTransactions
                    .Where(t => t.TransactionDate >= start && t.TransactionDate < end)
                    .ToListAsync();

                if (!string.IsNullOrWhiteSpace(itemCode))
                    allTransactions = allTransactions.Where(t => t.ItemCode.Contains(itemCode)).ToList();

                if (!string.IsNullOrWhiteSpace(transactionType))
                    allTransactions = allTransactions.Where(t => t.TransactionType == transactionType).ToList();

                if (!string.IsNullOrWhiteSpace(sectionId))
                    allTransactions = allTransactions.Where(t => t.SectionId == sectionId).ToList();

                // Item summary
                var itemSummary = allTransactions
                    .GroupBy(t => new { t.ItemCode, t.ItemName, t.Unit })
                    .Select(g => new
                    {
                        g.Key.ItemCode,
                        g.Key.ItemName,
                        g.Key.Unit,
                        TotalIn = g.Where(t => t.TransactionType == "IN" || t.TransactionType == "RECEIVE")
                            .Sum(t => t.Quantity),
                        TotalOut = g.Where(t => t.TransactionType == "OUT" || t.TransactionType == "ISSUE")
                            .Sum(t => t.Quantity),
                        TotalHold = g.Where(t => t.TransactionType == "HOLD")
                            .Sum(t => t.Quantity),
                        TotalAdjust = g.Where(t => t.TransactionType == "ADJUST")
                            .Sum(t => t.Quantity),
                        TransactionCount = g.Count(),
                        LatestBalance = g.OrderByDescending(t => t.TransactionDate)
                            .ThenByDescending(t => t.DailySequence)
                            .FirstOrDefault()?.BalanceAfter ?? 0
                    })
                    .OrderBy(x => x.ItemCode)
                    .ToList();

                // Type summary
                var typeSummary = allTransactions
                    .GroupBy(t => t.TransactionType)
                    .Select(g => new
                    {
                        TransactionType = g.Key,
                        Count = g.Count(),
                        TotalQuantity = g.Sum(t => t.Quantity)
                    })
                    .ToList();

                var response = new
                {
                    Headers = new[]
                    {
                new { key = "TransactionDate", label = "วันที่", type = "datetime", sortable = true },
                new { key = "TransactionId", label = "เลขที่ธุรกรรม", type = "text", sortable = true },
                new { key = "ItemCode", label = "รหัสสินค้า", type = "text", sortable = true },
                new { key = "ItemName", label = "ชื่อสินค้า", type = "text", sortable = true },
                new { key = "TransactionType", label = "ประเภท", type = "text", sortable = true },
                new { key = "TransactionSubtype", label = "ประเภทย่อย", type = "text", sortable = true },
                new { key = "Quantity", label = "จำนวน", type = "number", sortable = true },
                new { key = "Unit", label = "หน่วย", type = "text", sortable = false },
                new { key = "BalanceAfter", label = "คงเหลือ", type = "number", sortable = true },
                new { key = "IssueId", label = "เลขที่เบิก", type = "text", sortable = true },
                new { key = "SectionName", label = "แผนก", type = "text", sortable = true },
                new { key = "PerformedBy", label = "ผู้ทำรายการ", type = "text", sortable = true },
                new { key = "Remarks", label = "หมายเหตุ", type = "text", sortable = false }
            },
                    Data = transactions,
                    Pagination = new
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                        HasPrevious = page > 1,
                        HasNext = page < (int)Math.Ceiling(totalCount / (double)pageSize)
                    },
                    Summary = new
                    {
                        TotalRecords = allTransactions.Count,
                        TotalQuantity = allTransactions.Sum(x => x.Quantity),
                        TotalIn = allTransactions.Where(x => x.TransactionType == "IN" || x.TransactionType == "RECEIVE")
                            .Sum(x => x.Quantity),
                        TotalOut = allTransactions.Where(x => x.TransactionType == "OUT" || x.TransactionType == "ISSUE")
                            .Sum(x => x.Quantity),
                        TotalHold = allTransactions.Where(x => x.TransactionType == "HOLD")
                            .Sum(x => x.Quantity),
                        TotalAdjust = allTransactions.Where(x => x.TransactionType == "ADJUST")
                            .Sum(x => x.Quantity),
                        ByType = typeSummary
                    },
                    ItemSummary = itemSummary,
                    Period = new { Start = start, End = end }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาด",
                    error = ex.Message
                });
            }
        }
        // 4. Report สรุปรวม (Combined Summary Report)
        [HttpGet("reports/summary")]
        public async Task<IActionResult> GetSummaryReport(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? itemCode)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today.AddDays(1);

                // Get receive data
                var receiveQuery = _postcontext.ItemReceiveDetails
                    .Where(r => r.ReceivedDate >= start && r.ReceivedDate < end);

                if (!string.IsNullOrWhiteSpace(itemCode))
                    receiveQuery = receiveQuery.Where(r => r.ItemCode == itemCode);

                var receiveData = await receiveQuery
                    .GroupBy(r => new { r.ItemCode, r.ItemName, r.Unit })
                    .Select(g => new
                    {
                        g.Key.ItemCode,
                        g.Key.ItemName,
                        g.Key.Unit,
                        TotalIssued = g.Sum(r => r.QuantityIssued ?? 0),
                        TotalReceived = g.Sum(r => r.QuantityReceived ?? 0),
                        ReceiveCount = g.Count()
                    })
                    .ToListAsync();

                // Get hold data
                var holdQuery = _postcontext.ItemHoldDetails
                    .Where(h => h.HoldDate >= start && h.HoldDate < end);

                if (!string.IsNullOrWhiteSpace(itemCode))
                    holdQuery = holdQuery.Where(h => h.ItemCode == itemCode);

                var holdData = await holdQuery
                    .GroupBy(h => new { h.ItemCode, h.ItemName, h.Unit })
                    .Select(g => new
                    {
                        g.Key.ItemCode,
                        g.Key.ItemName,
                        g.Key.Unit,
                        TotalHold = g.Sum(h => h.QuantityHold ?? 0),
                        ActiveHold = g.Where(h => h.HoldStatus == "active").Sum(h => h.QuantityHold ?? 0),
                        HoldCount = g.Count()
                    })
                    .ToListAsync();

                // Get transaction data
                var transQuery = _postcontext.InventoryTransactions
                    .Where(t => t.TransactionDate >= start && t.TransactionDate < end);

                if (!string.IsNullOrWhiteSpace(itemCode))
                    transQuery = transQuery.Where(t => t.ItemCode == itemCode);

                var transData = await transQuery
                    .GroupBy(t => new { t.ItemCode, t.ItemName, t.Unit })
                    .Select(g => new
                    {
                        g.Key.ItemCode,
                        g.Key.ItemName,
                        g.Key.Unit,
                        TotalIn = g.Where(t => t.TransactionType == "IN" || t.TransactionType == "RECEIVE")
                            .Sum(t => t.Quantity),
                        TotalOut = g.Where(t => t.TransactionType == "OUT" || t.TransactionType == "ISSUE")
                            .Sum(t => t.Quantity),
                        TransactionCount = g.Count()
                    })
                    .ToListAsync();

                // Get current status
                var itemCodes = receiveData.Select(r => r.ItemCode)
                    .Union(holdData.Select(h => h.ItemCode))
                    .Union(transData.Select(t => t.ItemCode))
                    .Distinct()
                    .ToList();

                var currentStatus = await _postcontext.VwItemCurrentStatuses
                    .Where(v => itemCodes.Contains(v.ItemCode))
                    .ToDictionaryAsync(v => v.ItemCode);

                // Combine all data
                var summaryData = itemCodes.Select(code =>
                {
                    var receive = receiveData.FirstOrDefault(r => r.ItemCode == code);
                    var hold = holdData.FirstOrDefault(h => h.ItemCode == code);
                    var trans = transData.FirstOrDefault(t => t.ItemCode == code);
                    var status = currentStatus.ContainsKey(code) ? currentStatus[code] : null;

                    return new
                    {
                        ItemCode = code,
                        ItemName = receive?.ItemName ?? hold?.ItemName ?? trans?.ItemName ?? "",
                        Unit = receive?.Unit ?? hold?.Unit ?? trans?.Unit ?? "",

                        // Receive data
                        TotalIssued = receive?.TotalIssued ?? 0,
                        TotalReceived = receive?.TotalReceived ?? 0,
                        ReceiveCount = receive?.ReceiveCount ?? 0,

                        // Hold data
                        TotalHold = hold?.TotalHold ?? 0,
                        ActiveHold = hold?.ActiveHold ?? 0,
                        HoldCount = hold?.HoldCount ?? 0,

                        // Transaction data
                        TotalIn = trans?.TotalIn ?? 0,
                        TotalOut = trans?.TotalOut ?? 0,
                        TransactionCount = trans?.TransactionCount ?? 0,

                        // Current status
                        CurrentBalance = status?.CurrentBalance ?? 0,
                        AvailableQuantity = status?.AvailableQuantity ?? 0,
                        LastUpdate = status?.LastTransactionDate
                    };
                })
                .OrderBy(x => x.ItemCode)
                .ToList();

                var response = new
                {
                    Headers = new[]
                    {
                new { key = "ItemCode", label = "รหัสสินค้า", type = "text", sortable = true },
                new { key = "ItemName", label = "ชื่อสินค้า", type = "text", sortable = true },
                new { key = "TotalReceived", label = "รับเข้า", type = "number", sortable = true },
                new { key = "TotalOut", label = "เบิกออก", type = "number", sortable = true },
                new { key = "ActiveHold", label = "จองอยู่", type = "number", sortable = true },
                new { key = "CurrentBalance", label = "คงเหลือ", type = "number", sortable = true },
                new { key = "AvailableQuantity", label = "พร้อมใช้", type = "number", sortable = true },
                new { key = "Unit", label = "หน่วย", type = "text", sortable = false },
                new { key = "TransactionCount", label = "จำนวนรายการ", type = "number", sortable = true },
                new { key = "LastUpdate", label = "อัปเดตล่าสุด", type = "datetime", sortable = true }
            },
                    Data = summaryData,
                    Summary = new
                    {
                        TotalItems = summaryData.Count,
                        TotalReceived = summaryData.Sum(x => x.TotalReceived),
                        TotalOut = summaryData.Sum(x => x.TotalOut),
                        TotalHold = summaryData.Sum(x => x.ActiveHold),
                        TotalBalance = summaryData.Sum(x => x.CurrentBalance),
                        TotalAvailable = summaryData.Sum(x => x.AvailableQuantity),
                        TotalTransactions = summaryData.Sum(x => x.TransactionCount)
                    },
                    Period = new { Start = start, End = end }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาด",
                    error = ex.Message
                });
            }
        }
        private decimal ConvertToDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            if (decimal.TryParse(value, out decimal result))
                return result;

            return 0;
        }


        // Request Model
        private async Task CreateInventoryTransaction(
            string transactionId,
            string itemCode,
            string itemName,
            string lotNumber,
            string transactionType,
            string transactionSubtype,
            decimal quantity,
            string unit,
            string issueId,
            string referenceId,
            string referenceType,
            string sectionId,
            string sectionName,
            string processId,
            string performedBy,
            string remarks)
        {
            // Get current balance
            var lastTransaction = await _postcontext.InventoryTransactions
                .Where(t => t.ItemCode == itemCode)
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.DailySequence)
                .FirstOrDefaultAsync();

            var currentBalance = lastTransaction?.BalanceAfter ?? 0;

            // Calculate new balance
            var newBalance = transactionType switch
            {
                "IN" or "RECEIVE" => currentBalance + quantity,
                "OUT" or "ISSUE" => currentBalance - quantity,
                "ADJUST" => currentBalance + quantity,
                _ => currentBalance
            };

            // Get daily sequence
            var dailySeq = await _postcontext.InventoryTransactions
                .Where(t => t.ItemCode == itemCode &&
                       t.TransactionDate.Date == DateTime.Today)
                .MaxAsync(t => (int?)t.DailySequence) ?? 0;

            var transactionEntity = new InventoryTransaction
            {
                Id = Guid.NewGuid(),
                TransactionId = transactionId,
                ItemCode = itemCode,
                ItemName = itemName,
                LotNumber = lotNumber,
                TransactionType = transactionType,
                TransactionSubtype = transactionSubtype,
                Quantity = quantity,
                Unit = unit,
                BalanceAfter = newBalance,
                IssueId = issueId,
                ReferenceId = referenceId,
                ReferenceType = referenceType,
                SectionId = sectionId,
                SectionName = sectionName,
                ProcessId = processId,
                PerformedBy = performedBy,
                TransactionDate = DateTime.Now,
                Remarks = remarks,
                DailySequence = dailySeq + 1,
                CreatedAt = DateTime.Now
            };

            _postcontext.InventoryTransactions.Add(transactionEntity);
        }

        public class BarcodeRequest
        {
            public string Barcode { get; set; } = string.Empty;
        }
        public class BarcodeDataRequest
        {
            public string Barcode { get; set; } = string.Empty;
            public string? UserId { get; set; }
            public List<BarcodeItem> Items { get; set; } = new List<BarcodeItem>();
        }

        public class BarcodeItem
        {
            public string Section { get; set; } = string.Empty;
            public string ItemCode { get; set; } = string.Empty;
            public string ItemName { get; set; } = string.Empty;
            public string LotNumber { get; set; } = string.Empty;
            public string QuantityWH { get; set; } = string.Empty;
            public string Unit { get; set; } = string.Empty;
            public string SubunitQty { get; set; } = string.Empty;
            public string Subunit { get; set; } = string.Empty;
        }
        public class PaginationParams
        {
            private const int MaxPageSize = 100;
            private int _pageSize = 20;

            public int Page { get; set; } = 1;

            public int PageSize
            {
                get => _pageSize;
                set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
            }
        }

        public class PagedResult<T>
        {
            public List<T> Data { get; set; } = new();
            public int CurrentPage { get; set; }
            public int PageSize { get; set; }
            public int TotalCount { get; set; }
            public int TotalPages { get; set; }
            public bool HasPrevious => CurrentPage > 1;
            public bool HasNext => CurrentPage < TotalPages;
        }

    }
}