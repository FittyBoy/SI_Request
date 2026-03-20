using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using SI24004.Models.PostgreSQL;
using SI24004.Models.DTOs;
using SI24004.Models.Responses;
using SI24004.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SI24004.Services
{
    public class RegularSubstanceService : IRegularSubstanceService
    {
        private readonly PostgrestContext _context;
        private readonly ILogger<RegularSubstanceService> _logger;

        public RegularSubstanceService(
            PostgrestContext context,
            ILogger<RegularSubstanceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SubstanceListResponse> GetAllSubstances(int page, int pageSize)
        {
            try
            {
                var query = _context.RegularSubstands.AsQueryable();

                var totalRecords = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

                var substances = await query
                    .OrderBy(s => s.SubstanceChemical)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new SubstanceListResponse
                {
                    Success = true,
                    Data = substances,
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalRecords = totalRecords,
                        TotalPages = totalPages
                    },
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving substances list");
                throw;
            }
        }
        public async Task<List<RegularSubstand>> GetAllSubstancesNoPagination()
        {
            try
            {
                return await _context.RegularSubstands
                    .OrderBy(s => s.SubstanceChemical)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all substances");
                throw;
            }
        }


        public async Task<RegularSubstand> GetSubstanceById(Guid id)
        {
            try
            {
                return await _context.RegularSubstands.FirstOrDefaultAsync(s => s.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving substance by ID: {Id}", id);
                throw;
            }
        }

        public async Task<RegularSubstand> CreateSubstance(RegularSubstanceRequest request)
        {
            try
            {
                // Check if CAS number already exists
                var existingSubstance = await _context.RegularSubstands
                    .FirstOrDefaultAsync(s => s.SubstanceCasNo == request.SubstanceCasNo);

                if (existingSubstance != null)
                {
                    throw new InvalidOperationException($"A substance with CAS number {request.SubstanceCasNo} already exists");
                }

                var newSubstance = new RegularSubstand
                {
                    Id = Guid.NewGuid(),
                    SubstanceChemical = request.SubstanceChemical,
                    SubstanceIdentifier = request.SubstanceIdentifier,
                    SubstanceCasNo = request.SubstanceCasNo,
                    SubstanceThresholdLimit = request.SubstanceThresholdLimit,
                    SubstanceScope = request.SubstanceScope,
                    SubstanceExamples = request.SubstanceExamples,
                    SubstanceReferences = request.SubstanceReferences
                };

                _context.RegularSubstands.Add(newSubstance);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new substance with ID: {Id}", newSubstance.Id);
                return newSubstance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating substance");
                throw;
            }
        }

        public async Task<RegularSubstand> UpdateSubstance(Guid id, RegularSubstanceRequest request)
        {
            try
            {
                var substance = await _context.RegularSubstands
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (substance == null)
                {
                    return null;
                }

                // Check if CAS number is being changed and if new CAS number already exists
                if (substance.SubstanceCasNo != request.SubstanceCasNo)
                {
                    var existingSubstance = await _context.RegularSubstands
                        .FirstOrDefaultAsync(s => s.SubstanceCasNo == request.SubstanceCasNo && s.Id != id);

                    if (existingSubstance != null)
                    {
                        throw new InvalidOperationException($"A substance with CAS number {request.SubstanceCasNo} already exists");
                    }
                }

                substance.SubstanceChemical = request.SubstanceChemical;
                substance.SubstanceIdentifier = request.SubstanceIdentifier;
                substance.SubstanceCasNo = request.SubstanceCasNo;
                substance.SubstanceThresholdLimit = request.SubstanceThresholdLimit;
                substance.SubstanceScope = request.SubstanceScope;
                substance.SubstanceExamples = request.SubstanceExamples;
                substance.SubstanceReferences = request.SubstanceReferences;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated substance with ID: {Id}", id);
                return substance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating substance with ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteSubstance(Guid id)
        {
            try
            {
                var substance = await _context.RegularSubstands
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (substance == null)
                {
                    return false;
                }

                _context.RegularSubstands.Remove(substance);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted substance with ID: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting substance with ID: {Id}", id);
                throw;
            }
        }
    }
}
