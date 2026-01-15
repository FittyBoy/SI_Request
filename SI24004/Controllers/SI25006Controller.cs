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

        // POST: api/SI25006/initialize-data
        [HttpPost("initialize-data")]
        public async Task<ActionResult> InitializeData()
        {
            try
            {
                // Check if data already exists
                var hasData = await _context.Materalinventories.AnyAsync();
                if (hasData)
                {
                    return Ok(new { message = "Data already initialized" });
                }

                // Initialize sample shifts
                var shifts = new[]
                {
                    new Shiftmaster { Id = Guid.NewGuid(), Shiftcode = "A", Shiftname = "Morning Shift", Starttime = new TimeOnly(7, 0), Endtime = new TimeOnly(15, 0) },
                    new Shiftmaster { Id = Guid.NewGuid(), Shiftcode = "B", Shiftname = "Afternoon Shift", Starttime = new TimeOnly(15, 0), Endtime = new TimeOnly(23, 0) },
                    new Shiftmaster { Id = Guid.NewGuid(), Shiftcode = "C", Shiftname = "Night Shift", Starttime = new TimeOnly(23, 0), Endtime = new TimeOnly(7, 0) }
                };
                _context.Shiftmasters.AddRange(shifts);

                // Initialize sample products
                var products = new[]
                {
                    new Productmaster { Id = Guid.NewGuid(), Productcode = "MOB01", Productname = "Mobile 0.1", Description = "Mobile Device Screen 0.1mm" },
                    new Productmaster { Id = Guid.NewGuid(), Productcode = "MOB02", Productname = "Mobile 0.2", Description = "Mobile Device Screen 0.2mm" },
                    new Productmaster { Id = Guid.NewGuid(), Productcode = "MOB03", Productname = "Mobile 0.3", Description = "Mobile Device Screen 0.3mm" }
                };
                _context.Productmasters.AddRange(products);

                // Initialize sample suppliers
                var suppliers = new[]
                {
                    new Suppliermaster { Id = Guid.NewGuid(), Suppliercode = "SUP001", Suppliername = "Supplier A", Contactperson = "John Doe", Phone = "02-1234567" },
                    new Suppliermaster { Id = Guid.NewGuid(), Suppliercode = "SUP002", Suppliername = "Supplier B", Contactperson = "Jane Smith", Phone = "02-7654321" },
                    new Suppliermaster { Id = Guid.NewGuid(), Suppliercode = "INT001", Suppliername = "Internal Production", Contactperson = "Production Team", Phone = "02-1111111" }
                };
                _context.Suppliermasters.AddRange(suppliers);

                // Initialize sample material types
                var materialTypes = new[]
                {
                    new Materialtype { Id = Guid.NewGuid(), Typename = "Raw Material", Description = "Raw materials for production" },
                    new Materialtype { Id = Guid.NewGuid(), Typename = "Chemical", Description = "Chemical substances" },
                    new Materialtype { Id = Guid.NewGuid(), Typename = "Packaging", Description = "Packaging materials" },
                    new Materialtype { Id = Guid.NewGuid(), Typename = "Additive", Description = "Production additives" }
                };
                _context.Materialtypes.AddRange(materialTypes);

                // Initialize sample locations
                var locations = new[]
                {
                    new Locationmaster { Id = Guid.NewGuid(), Locationcode = "A-01", Locationname = "Zone A Shelf 1", Zone = "A", Maxcapacity = 100, Currentcapacity = 0, Isactive = true },
                    new Locationmaster { Id = Guid.NewGuid(), Locationcode = "A-02", Locationname = "Zone A Shelf 2", Zone = "A", Maxcapacity = 100, Currentcapacity = 0, Isactive = true },
                    new Locationmaster { Id = Guid.NewGuid(), Locationcode = "B-01", Locationname = "Zone B Shelf 1", Zone = "B", Maxcapacity = 100, Currentcapacity = 0, Isactive = true }
                };
                _context.Locationmasters.AddRange(locations);

                // Initialize sample employees
                var employees = new[]
                {
                    new Employeemaster { Id = Guid.NewGuid(), Employeeid = "12345", Employeename = "John Doe", Department = "Production", Position = "Operator", Shift = shifts[0].Id, Isactive = true },
                    new Employeemaster { Id = Guid.NewGuid(), Employeeid = "12346", Employeename = "Jane Smith", Department = "Production", Position = "Supervisor", Shift = shifts[1].Id, Isactive = true }
                };
                _context.Employeemasters.AddRange(employees);

                await _context.SaveChangesAsync();

                // Initialize sample materials (ต้อง save ก่อนเพื่อให้มี FK)
                var materials = new[]
                {
                    new Materalinventory
                    {
                        Id = Guid.NewGuid(),
                        Matname = "FO1500",
                        Matquantity = 150,
                        Mattypeid = materialTypes[0].Id,
                        Case = "Box",
                        Location = "A-01",
                        Shift = shifts[0].Id,
                        Product = products[0].Id,
                        Supplier = suppliers[0].Id,
                        Empid = employees[0].Id,
                        Insertdate = DateOnly.FromDateTime(DateTime.Now)
                    },
                    new Materalinventory
                    {
                        Id = Guid.NewGuid(),
                        Matname = "Aluminum Oxide",
                        Matquantity = 75,
                        Mattypeid = materialTypes[1].Id,
                        Case = "Drum",
                        Location = "A-02",
                        Shift = shifts[0].Id,
                        Product = products[1].Id,
                        Supplier = suppliers[1].Id,
                        Empid = employees[0].Id,
                        Insertdate = DateOnly.FromDateTime(DateTime.Now)
                    }
                };
                _context.Materalinventories.AddRange(materials);

                await _context.SaveChangesAsync();

                return Ok(new { message = "Sample data initialized successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error initializing data: {ex.Message}\n{ex.InnerException?.Message}");
            }
        }

        // GET: api/SI25006
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetMaterials()
        {
            try
            {
                var materials = await _context.Materalinventories
                    .Include(m => m.ShiftNavigation)
                    .Include(m => m.ProductNavigation)
                    .Include(m => m.SupplierNavigation)
                    .Include(m => m.Emp)
                    .Include(m => m.Mattype)
                    .Select(m => new
                    {
                        Id = m.Id.ToString(),
                        Matname = m.Matname ?? "",
                        Matquantity = m.Matquantity ?? 0,
                        matUnit = "kg",
                        matTypeId = m.Mattypeid.ToString(),
                        @case = m.Case ?? "",
                        expDate = m.Expdate,
                        insertDate = m.Insertdate,
                        empId = m.Empid.ToString(),
                        shift = m.Shift.ToString(),
                        product = m.Product.ToString(),
                        supplier = m.Supplier.ToString(),
                        lotNumber = m.Lotnumber ?? "",
                        Location = m.Location ?? "",
                        // Additional info for display
                        shiftName = m.ShiftNavigation != null ? m.ShiftNavigation.Shiftcode : "",
                        productName = m.ProductNavigation != null ? m.ProductNavigation.Productname : "",
                        supplierName = m.SupplierNavigation != null ? m.SupplierNavigation.Suppliername : "",
                        employeeName = m.Emp != null ? m.Emp.Employeename : "",
                        materialTypename = m.Mattype != null ? m.Mattype.Typename : ""
                    })
                    .OrderBy(m => m.Matname)
                    .ToListAsync();

                return Ok(materials);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/SI25006/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetMaterial(string id)
        {
            try
            {
                if (!Guid.TryParse(id, out Guid guidId))
                {
                    return BadRequest("Invalid ID format");
                }

                var material = await _context.Materalinventories
                    .Include(m => m.ShiftNavigation)
                    .Include(m => m.ProductNavigation)
                    .Include(m => m.SupplierNavigation)
                    .Include(m => m.Emp)
                    .Include(m => m.Mattype)
                    .Where(m => m.Id == guidId)
                    .Select(m => new
                    {
                        Id = m.Id.ToString(),
                        Matname = m.Matname ?? "",
                        Matquantity = m.Matquantity ?? 0,
                        matUnit = "kg",
                        matTypeId = m.Mattypeid.ToString(),
                        @case = m.Case ?? "",
                        expDate = m.Expdate,
                        insertDate = m.Insertdate,
                        empId = m.Empid.ToString(),
                        shift = m.Shift.ToString(),
                        product = m.Product.ToString(),
                        supplier = m.Supplier.ToString(),
                        lotNumber = m.Lotnumber ?? "",
                        Location = m.Location ?? "",
                        shiftName = m.ShiftNavigation != null ? m.ShiftNavigation.Shiftcode : "",
                        productName = m.ProductNavigation != null ? m.ProductNavigation.Productname : "",
                        supplierName = m.SupplierNavigation != null ? m.SupplierNavigation.Suppliername : "",
                        employeeName = m.Emp != null ? m.Emp.Employeename : ""
                    })
                    .FirstOrDefaultAsync();

                if (material == null)
                {
                    return NotFound();
                }

                return Ok(material);
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
                var shifts = await _context.Shiftmasters
                    .Select(s => new
                    {
                        id = s.Id.ToString(),
                        shift = s.Shiftcode,
                        shiftName = s.Shiftname,
                        startTime = s.Starttime,
                        endTime = s.Endtime,
                        count = _context.Materialreceiverecords.Count(r => r.Shift == s.Id)
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

        // GET: api/SI25006/products
        [HttpGet("products")]
        public async Task<ActionResult<IEnumerable<object>>> GetProducts()
        {
            try
            {
                var products = await _context.Productmasters
                    .Select(p => new
                    {
                        id = p.Id.ToString(),
                        product = p.Productname,
                        productCode = p.Productcode,
                        description = p.Description,
                        count = _context.Materialreceiverecords.Count(r => r.Product == p.Id)
                    })
                    .OrderBy(x => x.product)
                    .ToListAsync();

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/SI25006/suppliers
        [HttpGet("suppliers")]
        public async Task<ActionResult<IEnumerable<object>>> GetSuppliers()
        {
            try
            {
                var suppliers = await _context.Suppliermasters
                    .Select(s => new
                    {
                        id = s.Id.ToString(),
                        supplier = s.Suppliername,
                        supplierCode = s.Suppliercode,
                        contactPerson = s.Contactperson,
                        phone = s.Phone,
                        count = _context.Materialreceiverecords.Count(r => r.Supplier == s.Id),
                        lastUsed = _context.Materialreceiverecords
                            .Where(r => r.Supplier == s.Id)
                            .Max(r => (DateOnly?)r.Insertdate)
                    })
                    .OrderByDescending(x => x.count)
                    .ToListAsync();

                return Ok(suppliers);
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
                        location = l.Locationcode,
                        locationName = l.Locationname,
                        zone = l.Zone,
                        isActive = l.Isactive,
                        currentCapacity = l.Currentcapacity ?? 0,
                        maxCapacity = l.Maxcapacity ?? 100
                    })
                    .OrderBy(l => l.location)
                    .ToListAsync();

                return Ok(locations);
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
                var types = await _context.Materialtypes
                    .Select(mt => new
                    {
                        id = mt.Id.ToString(),
                        name = mt.Typename,
                        description = mt.Description ?? ""
                    })
                    .OrderBy(mt => mt.name)
                    .ToListAsync();

                return Ok(types);
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
                var caseTypes = await _context.Materialreceiverecords
                    .Where(r => !string.IsNullOrEmpty(r.Case))
                    .GroupBy(r => r.Case)
                    .Select(g => new
                    {
                        caseType = g.Key,
                        count = g.Count()
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

        // GET: api/SI25006/employees
        [HttpGet("employees")]
        public async Task<ActionResult<IEnumerable<object>>> GetEmployees()
        {
            try
            {
                var employees = await _context.Employeemasters
                    .Include(e => e.ShiftNavigation)
                    .Where(e => e.Isactive == true)
                    .Select(e => new
                    {
                        id = e.Id.ToString(),
                        empId = e.Employeeid,
                        empName = e.Employeename,
                        employeename = e.Employeename,
                        department = e.Department,
                        position = e.Position,
                        shift = e.Shift.ToString(),
                        shiftCode = e.ShiftNavigation != null ? e.ShiftNavigation.Shiftcode : "",
                        isActive = e.Isactive
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

        // GET: api/SI25006/receive/recent
        [HttpGet("receive/recent")]
        public async Task<ActionResult<IEnumerable<object>>> GetRecentReceipts([FromQuery] int count = 10)
        {
            try
            {
                var receipts = await _context.Materialreceiverecords
                    .Include(r => r.ShiftNavigation)
                    .Include(r => r.ProductNavigation)
                    .Include(r => r.SupplierNavigation)
                    .Include(r => r.Empid)
                    .Where(r => r.Status == "in")
                    .OrderByDescending(r => r.Insertdate)
                    .ThenByDescending(r => r.Createdat)
                    .Take(count)
                    .Select(r => new
                    {
                        Id = r.Id.ToString(),
                        Matname = r.Matname,
                        Matquantity = r.Matquantity,
                        Mattypeid = r.Mattypeid.ToString(),
                        Case = r.Case,
                        Expdate = r.Expdate,
                        Empid = r.Empid.ToString(),
                        Shift = r.ShiftNavigation != null ? r.ShiftNavigation.Shiftcode : "",
                        Product = r.ProductNavigation != null ? r.ProductNavigation.Productname : "",
                        Supplier = r.SupplierNavigation != null ? r.SupplierNavigation.Suppliername : "",
                        Lotnumber = r.Lotnumber,
                        Location = r.Location,
                        Insertdate = r.Insertdate
                    })
                    .ToListAsync();

                return Ok(receipts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/SI25006/receive-and-update
        [HttpPost("receive-and-update")]
        public async Task<ActionResult<object>> ReceiveAndUpdate(MaterialReceiveUpdateRequest request)
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

                    // Validate foreign keys
                    if (!Guid.TryParse(request.EmpId, out Guid empGuid))
                    {
                        return BadRequest("Invalid Employee ID format.");
                    }

                    if (!Guid.TryParse(request.Shift, out Guid shiftGuid))
                    {
                        return BadRequest("Invalid Shift ID format.");
                    }

                    Guid? productGuid = null;
                    if (!string.IsNullOrEmpty(request.Product) && Guid.TryParse(request.Product, out Guid pGuid))
                    {
                        productGuid = pGuid;
                    }

                    Guid? supplierGuid = null;
                    if (!string.IsNullOrEmpty(request.Supplier) && Guid.TryParse(request.Supplier, out Guid sGuid))
                    {
                        supplierGuid = sGuid;
                    }

                    Guid? matTypeGuid = null;
                    if (!string.IsNullOrEmpty(request.MatTypeId) && Guid.TryParse(request.MatTypeId, out Guid mtGuid))
                    {
                        matTypeGuid = mtGuid;
                    }

                    // Create receive record
                    var receiveRecord = new Materialreceiverecord
                    {
                        Id = Guid.NewGuid(),
                        Matname = request.MatName,
                        Matquantity = request.MatQuantity,
                        Mattypeid = matTypeGuid,
                        Case = request.Case,
                        Expdate = request.ExpDate,
                        Empid = empGuid,
                        Shift = shiftGuid,
                        Product = productGuid,
                        Supplier = supplierGuid,
                        Lotnumber = request.LotNumber,
                        Location = request.Location,
                        Status = "in",
                        Insertdate = DateOnly.FromDateTime(DateTime.Now),
                        Createdat = DateTime.UtcNow
                    };

                    _context.Materialreceiverecords.Add(receiveRecord);

                    // Update or create inventory
                    var inventory = await _context.Materalinventories
                        .FirstOrDefaultAsync(m => m.Matname == request.MatName &&
                                                   m.Location == request.Location);

                    if (inventory != null)
                    {
                        inventory.Matquantity = (inventory.Matquantity ?? 0) + request.MatQuantity;
                        inventory.Lotnumber = request.LotNumber;
                        if (request.ExpDate.HasValue)
                        {
                            inventory.Expdate = request.ExpDate;
                        }
                        inventory.Shift = shiftGuid;
                        inventory.Product = productGuid;
                        inventory.Supplier = supplierGuid;
                        inventory.Empid = empGuid;
                        _context.Entry(inventory).State = EntityState.Modified;
                    }
                    else
                    {
                        var newInventory = new Materalinventory
                        {
                            Id = Guid.NewGuid(),
                            Matname = request.MatName,
                            Matquantity = request.MatQuantity,
                            Mattypeid = matTypeGuid,
                            Case = request.Case,
                            Expdate = request.ExpDate,
                            Empid = empGuid,
                            Shift = shiftGuid,
                            Product = productGuid,
                            Supplier = supplierGuid,
                            Lotnumber = request.LotNumber,
                            Location = request.Location,
                            Insertdate = DateOnly.FromDateTime(DateTime.Now)
                        };
                        _context.Materalinventories.Add(newInventory);
                        inventory = newInventory;
                    }

                    // Update location capacity
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
                    return StatusCode(500, $"Internal server error: {ex.Message}\n{ex.InnerException?.Message}");
                }
            });
        }

        // PATCH: api/SI25006/{id}/quantity
        [HttpPatch("{id}/quantity")]
        public async Task<ActionResult> UpdateQuantity(string id, [FromBody] int newQuantity)
        {
            try
            {
                if (!Guid.TryParse(id, out Guid guidId))
                {
                    return BadRequest("Invalid ID format");
                }

                var material = await _context.Materalinventories.FindAsync(guidId);
                if (material == null)
                {
                    return NotFound();
                }

                material.Matquantity = newQuantity;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}