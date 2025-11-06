using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SI24004.Models;
using SI24004.Models.Requests;

namespace SI24004.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SI25006Controller : ControllerBase
    {
        private readonly PostgrestContext _context;

        public SI25006Controller(PostgrestContext context)
        {
            _context = context;
        }

        // GET: api/SI25006
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetMaterials()
        {
            try
            {
                var materials = await _context.MateralInventories
                    .Select(m => new
                    {
                        m.Id,
                        m.MatName,
                        m.MatQuantity,
                        m.MatTypeId,
                        m.Case,
                        m.ExpDate,
                        m.EmpId,
                        m.Shift,
                        m.Product,
                        m.LotNumber,
                        m.Location,
                        m.InsertDate,
                        MaterialTypeName = _context.MaterialTypes
                            .Where(mt => mt.Id == m.MatTypeId)
                            .Select(mt => mt.TypeName)
                            .FirstOrDefault(),
                        EmployeeName = _context.Employeemasters
                            .Where(e => e.Id == m.EmpId)
                            .Select(e => e.Employeename)
                            .FirstOrDefault(),
                        LocationName = _context.Locationmasters
                            .Where(l => l.Locationcode == m.Location)
                            .Select(l => l.Locationname)
                            .FirstOrDefault()
                    })
                    .OrderBy(m => m.MatName)
                    .ToListAsync();

                return Ok(materials);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/SI25006/stock-summary
        [HttpGet("stock-summary")]
        public async Task<ActionResult<IEnumerable<object>>> GetStockSummary()
        {
            try
            {
                var stockSummary = await _context.MateralInventories
                    .GroupBy(m => new { m.MatName, m.MatTypeId })
                    .Select(g => new
                    {
                        MaterialName = g.Key.MatName,
                        MaterialTypeId = g.Key.MatTypeId,
                        TotalQuantity = g.Sum(m => m.MatQuantity ?? 0),
                        LocationCount = g.Select(m => m.Location).Distinct().Count(),
                        LastUpdate = g.Max(m => m.InsertDate),
                        MaterialTypeName = _context.MaterialTypes
                            .Where(mt => mt.Id == g.Key.MatTypeId)
                            .Select(mt => mt.TypeName)
                            .FirstOrDefault()
                    })
                    .OrderBy(s => s.MaterialName)
                    .ToListAsync();

                return Ok(stockSummary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        // GET: api/SI25006/materials/master
        [HttpGet("materials/master")]
        public async Task<ActionResult<IEnumerable<object>>> GetMaterialsMaster()
        {
            try
            {
                var materials = await _context.MateralInventories
                    .Select(m => new
                    {
                        materialName = m.MatName,
                        materialTypeId = m.MatTypeId,
                        materialTypeName = _context.MaterialTypes
                            .Where(mt => mt.Id == m.MatTypeId)
                            .Select(mt => mt.TypeName)
                            .FirstOrDefault(),
                        currentStock = _context.MateralInventories
                            .Where(inv => inv.MatName == m.MatName)
                            .Sum(inv => inv.MatQuantity ?? 0)
                    })
                    .OrderBy(m => m.materialName)
                    .ToListAsync();

                return Ok(materials);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/SI25006/locations
        [HttpGet("locations")]
        public async Task<ActionResult<IEnumerable<object>>> GetLocations()
        {
            try
            {
                var locations = await _context.Locationmasters
                    .Where(l => l.Isactive == true)
                    .Select(l => new
                    {
                        locationCode = l.Locationcode,
                        locationName = l.Locationname,
                        zone = l.Zone,
                        isActive = l.Isactive,
                        currentCapacity = l.Currentcapacity,
                        maxCapacity = l.Maxcapacity,
                        materialCount = _context.MateralInventories
                            .Count(m => m.Location == l.Locationcode)
                    })
                    .OrderBy(l => l.locationCode)
                    .ToListAsync();

                return Ok(locations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/SI25006/employees
        [HttpGet("employees")]
        public async Task<ActionResult<IEnumerable<object>>> GetEmployees()
        {
            try
            {
                var employees = await _context.Employeemasters
                    .Where(e => e.Isactive == true)
                    .Select(e => new
                    {
                        empId = e.Employeeid,
                        empName = e.Employeename,
                        department = e.Department,
                        position = e.Position,
                        shift = e.Shift,
                        isActive = e.Isactive,
                        totalTransactions = _context.MaterialReceiveRecords
                            .Count(r => r.EmpId == e.Id)
                    })
                    .OrderBy(e => e.empId)
                    .ToListAsync();

                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/SI25006/material-types
        [HttpGet("material-types")]
        public async Task<ActionResult<IEnumerable<object>>> GetMaterialTypes()
        {
            try
            {
                var types = await _context.MaterialTypes
                    .Select(mt => new
                    {
                        id = mt.Id,
                        name = mt.TypeName,
                        description = mt.Description,
                        materialCount = _context.MateralInventories.Count(m => m.MatTypeId == mt.Id)
                    })
                    .OrderBy(mt => mt.id)
                    .ToListAsync();

                return Ok(types);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/SI25006/shifts
        [HttpGet("shifts")]
        public async Task<ActionResult<IEnumerable<object>>> GetShifts()
        {
            try
            {
                var shifts = await _context.Employeemasters
                    .Where(e => e.Isactive == true && !string.IsNullOrEmpty(e.Shift))
                    .GroupBy(e => e.Shift)
                    .Select(g => new
                    {
                        shift = g.Key,
                        employeeCount = g.Count(),
                        transactionCount = _context.MaterialReceiveRecords
                            .Count(r => r.Shift == g.Key)
                    })
                    .OrderBy(x => x.shift)
                    .ToListAsync();

                return Ok(shifts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/SI25006/case-types
        [HttpGet("case-types")]
        public async Task<ActionResult<IEnumerable<object>>> GetCaseTypes()
        {
            try
            {
                var caseTypes = await _context.MaterialReceiveRecords
                    .Where(r => !string.IsNullOrEmpty(r.Case))
                    .GroupBy(r => r.Case)
                    .Select(g => new
                    {
                        caseType = g.Key,
                        count = g.Count(),
                        lastUsed = g.Max(x => x.InsertDate)
                    })
                    .OrderByDescending(x => x.count)
                    .ToListAsync();

                return Ok(caseTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/SI25006/transactions
        [HttpGet("transactions")]
        public async Task<ActionResult<object>> GetTransactions(
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var query = _context.MaterialReceiveRecords.AsQueryable();

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(r => r.Status.ToLower() == status.ToLower());
                }

                var totalCount = await query.CountAsync();

                var transactions = await query
                    .OrderByDescending(r => r.InsertDate)
                    .ThenByDescending(r => r.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new
                    {
                        r.Id,
                        r.MatName,
                        r.MatQuantity,
                        r.MatTypeId,
                        r.Case,
                        r.ExpDate,
                        r.EmpId,
                        r.Shift,
                        r.Product,
                        r.LotNumber,
                        r.Location,
                        r.Status,
                        r.InsertDate,
                        r.CreatedAt,
                        //EmployeeName = _context.Employeemasters
                        //    .Where(e => e.Employeeid == r.EmpId)
                        //    .Select(e => e.Employeename)
                        //    .FirstOrDefault(),
                        //MaterialTypeName = _context.MaterialTypes
                        //    .Where(mt => mt.Id == r.MatTypeId)
                        //    .Select(mt => mt.Typename)
                        //    .FirstOrDefault()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    data = transactions,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/SI25006/transactions/recent
        [HttpGet("transactions/recent")]
        public async Task<ActionResult<IEnumerable<object>>> GetRecentTransactions([FromQuery] int count = 10)
        {
            try
            {
                var transactions = await _context.MaterialReceiveRecords
                    .OrderByDescending(r => r.InsertDate)
                    .ThenByDescending(r => r.CreatedAt)
                    .Take(count)
                    .Select(r => new
                    {
                        r.Id,
                        r.MatName,
                        r.MatQuantity,
                        r.Status,
                        r.InsertDate,
                        r.EmpId,
                        EmployeeName = _context.Employeemasters
                            .Where(e => e.Id == r.EmpId)
                            .Select(e => e.Employeename)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/SI25006/receive
        [HttpPost("receive")]
        public async Task<ActionResult<object>> ReceiveMaterial(MaterialReceiveUpdateRequest request)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    if (string.IsNullOrEmpty(request.MatName))
                    {
                        return BadRequest("Material name is required.");
                    }

                    if (request.MatQuantity <= 0)
                    {
                        return BadRequest("Quantity must be greater than 0.");
                    }

                    var receiveRecord = new MaterialReceiveRecord
                    {
                        Id = Guid.NewGuid(),
                        MatName = request.MatName,
                        MatQuantity = request.MatQuantity,
                        //MatTypeId = request.MatTypeId,
                        Case = request.Case,
                        ExpDate = request.ExpDate,
                        //EmpId = request.EmpId,
                        Shift = request.Shift,
                        //Product = request.Product,
                        LotNumber = request.LotNumber,
                        Location = request.Location,
                        Status = "in",
                        InsertDate = DateOnly.FromDateTime(DateTime.Now)
                    };

                    _context.MaterialReceiveRecords.Add(receiveRecord);

                    var inventory = await _context.MateralInventories
                        .FirstOrDefaultAsync(m => m.MatName == request.MatName &&
                                                   m.Location == request.Location);

                    if (inventory != null)
                    {
                        inventory.MatQuantity = (inventory.MatQuantity ?? 0) + request.MatQuantity;
                        inventory.LotNumber = request.LotNumber;
                        if (request.ExpDate.HasValue)
                        {
                            inventory.ExpDate = request.ExpDate;
                        }
                        _context.Entry(inventory).State = EntityState.Modified;
                    }
                    else
                    {
                        var newInventory = new MateralInventory
                        {
                            Id = Guid.NewGuid(),
                            MatName = request.MatName,
                            MatQuantity = request.MatQuantity,
                            //MatTypeId = request.MatTypeId,
                            Case = request.Case,
                            ExpDate = request.ExpDate,
                            //EmpId = request.EmpId,
                            Shift = request.Shift,
                            Product = request.Product,
                            //Supplier = request.Supplier,
                            LotNumber = request.LotNumber,
                            Location = request.Location,
                            InsertDate = DateOnly.FromDateTime(DateTime.Now)
                        };
                        _context.MateralInventories.Add(newInventory);
                        inventory = newInventory;
                    }

                    var location = await _context.Locationmasters
                        .FirstOrDefaultAsync(l => l.Locationcode == request.Location);

                    if (location != null)
                    {
                        location.Currentcapacity = (location.Currentcapacity ?? 0) + 1;
                        _context.Entry(location).State = EntityState.Modified;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new
                    {
                        receiveRecord,
                        updatedInventory = inventory,
                        message = $"Successfully received {request.MatQuantity} units of {request.MatName}"
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            });
        }

        // POST: api/SI25006/issue
        [HttpPost("issue")]
        public async Task<ActionResult<object>> IssueMaterial(MaterialReceiveUpdateRequest request)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    if (string.IsNullOrEmpty(request.MatName))
                    {
                        return BadRequest("Material name is required.");
                    }

                    if (request.MatQuantity <= 0)
                    {
                        return BadRequest("Quantity must be greater than 0.");
                    }

                    var currentStock = await _context.MateralInventories
                        .Where(m => m.MatName == request.MatName)
                        .SumAsync(m => m.MatQuantity ?? 0);

                    if (currentStock < request.MatQuantity)
                    {
                        return BadRequest($"Insufficient stock. Available: {currentStock}, Requested: {request.MatQuantity}");
                    }

                    var issueRecord = new MaterialReceiveRecord
                    {
                        Id = Guid.NewGuid(),
                        MatName = request.MatName,
                        MatQuantity = request.MatQuantity,
                        //MatTypeId = request.MatTypeId,
                        Case = request.Case,
                        ExpDate = request.ExpDate,
                        //EmpId = request.EmpId,
                        Shift = request.Shift,
                        //Product = request.Product,
                        LotNumber = request.LotNumber,
                        Location = request.Location,
                        Status = "out",
                        InsertDate = DateOnly.FromDateTime(DateTime.Now)
                    };

                    _context.MaterialReceiveRecords.Add(issueRecord);

                    var inventory = await _context.MateralInventories
                        .Where(m => m.MatName == request.MatName && m.MatQuantity > 0)
                        .OrderBy(m => m.ExpDate)
                        .FirstOrDefaultAsync();

                    if (inventory != null)
                    {
                        inventory.MatQuantity = (inventory.MatQuantity ?? 0) - request.MatQuantity;

                        if (inventory.MatQuantity <= 0)
                        {
                            _context.MateralInventories.Remove(inventory);
                        }
                        else
                        {
                            _context.Entry(inventory).State = EntityState.Modified;
                        }
                    }

                    var location = await _context.Locationmasters
                        .FirstOrDefaultAsync(l => l.Locationcode == request.Location);

                    if (location != null && location.Currentcapacity > 0)
                    {
                        location.Currentcapacity = (location.Currentcapacity ?? 0) - 1;
                        _context.Entry(location).State = EntityState.Modified;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new
                    {
                        issueRecord,
                        updatedInventory = inventory,
                        message = $"Successfully issued {request.MatQuantity} units of {request.MatName}"
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            });
        }

        // GET: api/SI25006/stock-report
        [HttpGet("stock-report")]
        public async Task<ActionResult<object>> GetStockReport()
        {
            try
            {
                var stockReport = await _context.MateralInventories
                    .Select(m => new
                    {
                        materialName = m.MatName,
                        materialType = m.MatTypeId,
                        materialTypeName = _context.MaterialTypes
                            .Where(mt => mt.Id == m.MatTypeId)
                            .Select(mt => mt.TypeName)
                            .FirstOrDefault(),
                        currentStock = _context.MateralInventories
                            .Where(inv => inv.MatName == m.MatName)
                            .Sum(inv => inv.MatQuantity ?? 0)
                    })
                    .OrderBy(m => m.materialName)
                    .ToListAsync();

                return Ok(new
                {
                    reportDate = DateTime.Now,
                    totalMaterials = stockReport.Count,
                    materials = stockReport
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/SI25006/material/{materialName}/history
        [HttpGet("material/{materialName}/history")]
        public async Task<ActionResult<object>> GetMaterialHistory(string materialName, [FromQuery] int days = 30)
        {
            try
            {
                var startDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-days));

                var history = await _context.MaterialReceiveRecords
                    .Where(r => r.MatName == materialName && r.InsertDate >= startDate)
                    .OrderByDescending(r => r.InsertDate)
                    .ThenByDescending(r => r.CreatedAt)
                    .Select(r => new
                    {
                        r.Id,
                        r.MatName,
                        r.MatQuantity,
                        r.Status,
                        r.InsertDate,
                        r.EmpId,
                        //employeeName = _context.Employeemasters
                        //    .Where(e => e.Employeeid == r.EmpId)
                        //    .Select(e => e.Employeename)
                        //    .FirstOrDefault(),
                        r.Shift,
                        r.Location,
                        r.LotNumber,
                        r.Product
                    })
                    .ToListAsync();

                var currentStock = await _context.MateralInventories
                    .Where(m => m.MatName == materialName)
                    .SumAsync(m => m.MatQuantity ?? 0);

                return Ok(new
                {
                    materialName,
                    currentStock,
                    periodDays = days,
                    summary = new
                    {
                        transactionCount = history.Count
                    },
                    history
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}