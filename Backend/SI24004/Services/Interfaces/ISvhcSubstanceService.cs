using SI24004.Models.PostgreSQL;
using SI24004.Models.DTOs;
using System;
using System.Threading.Tasks;

namespace SI24004.Services.Interfaces
{
    public interface ISvhcSubstanceService
    {
        Task<SvhcSubstanceResponse> GetAllSubstances(int page, int pageSize);
        Task<List<QaSubstance>> GetAllSubstancesNoPagination();
        Task<QaSubstance> GetSubstanceById(Guid id);
        Task<QaSubstance> CreateSubstance(SvhcSubstanceRequest request);
        Task<QaSubstance> UpdateSubstance(Guid id, SvhcSubstanceRequest request);
        Task<bool> DeleteSubstance(Guid id);
    }
}