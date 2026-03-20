using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SI24004.Models.PostgreSQL;
using SI24004.Models.DTOs;
using SI24004.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace SI24004.Services
{
    public class SvhcSubstanceService : ISvhcSubstanceService
    {
        private readonly PostgrestContext _dbContext;
        private readonly ILogger<SvhcSubstanceService> _logger;

        public SvhcSubstanceService(
            PostgrestContext dbContext,
            ILogger<SvhcSubstanceService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<SvhcSubstanceResponse> GetAllSubstances(int page, int pageSize)
        {
            try
            {
                var query = _dbContext.Set<QaSubstance>().AsQueryable();

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var items = await query
                    .OrderByDescending(s => s.SvhcCandidate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new SvhcSubstanceResponse
                {
                    Success = true,
                    Message = "SVHC substances retrieved successfully",
                    Data = new SvhcSubstanceData
                    {
                        Items = items,
                        TotalCount = totalCount,
                        Page = page,
                        PageSize = pageSize,
                        TotalPages = totalPages
                    },
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving SVHC substances");
                throw;
            }
        }

        public async Task<List<QaSubstance>> GetAllSubstancesNoPagination()
        {
            try
            {
                return await _dbContext.Set<QaSubstance>()
                    .OrderByDescending(s => s.SvhcCandidate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all SVHC substances");
                throw;
            }
        }

        public async Task<QaSubstance> GetSubstanceById(Guid id)
        {
            try
            {
                return await _dbContext.Set<QaSubstance>()
                    .FirstOrDefaultAsync(s => s.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving SVHC substance by ID: {Id}", id);
                throw;
            }
        }

        public async Task<QaSubstance> CreateSubstance(SvhcSubstanceRequest request)
        {
            try
            {
                // Check for duplicate CAS number
                var existingSubstance = await _dbContext.Set<QaSubstance>()
                    .FirstOrDefaultAsync(s => s.CasNo == request.CasNo);

                if (existingSubstance != null)
                {
                    throw new InvalidOperationException(
                        $"SVHC substance with CAS No {request.CasNo} already exists");
                }

                var substance = new QaSubstance
                {
                    Id = Guid.NewGuid(),
                    SubstanceName = request.SubstanceName,
                    CasNo = request.CasNo,
                    EcNo = request.EcNo,
                    ReasonForInclusion = request.ReasonForInclusion,
                    Uses = request.Uses,
                    SvhcCandidate = request.SvhcCandidate,

                };

                _dbContext.Set<QaSubstance>().Add(substance);
                await _dbContext.SaveChangesAsync();

                return substance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating SVHC substance");
                throw;
            }
        }

        public async Task<QaSubstance> UpdateSubstance(Guid id, SvhcSubstanceRequest request)
        {
            try
            {
                var substance = await _dbContext.Set<QaSubstance>()
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (substance == null)
                {
                    return null;
                }

                // Check if CAS number is being changed and if it conflicts
                if (substance.CasNo != request.CasNo)
                {
                    var duplicate = await _dbContext.Set<QaSubstance>()
                        .FirstOrDefaultAsync(s => s.CasNo == request.CasNo && s.Id != id);

                    if (duplicate != null)
                    {
                        throw new InvalidOperationException(
                            $"Another SVHC substance with CAS No {request.CasNo} already exists");
                    }
                }

                substance.SubstanceName = request.SubstanceName;
                substance.CasNo = request.CasNo;
                substance.EcNo = request.EcNo;
                substance.ReasonForInclusion = request.ReasonForInclusion;
                substance.Uses = request.Uses;
                substance.SvhcCandidate = request.SvhcCandidate;

                await _dbContext.SaveChangesAsync();

                return substance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating SVHC substance with ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteSubstance(Guid id)
        {
            try
            {
                var substance = await _dbContext.Set<QaSubstance>()
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (substance == null)
                {
                    return false;
                }

                _dbContext.Set<QaSubstance>().Remove(substance);
                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting SVHC substance with ID: {Id}", id);
                throw;
            }
        }
    }
}