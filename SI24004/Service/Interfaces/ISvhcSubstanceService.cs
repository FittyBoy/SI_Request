using SI24004.Models;
using SI24004.Models.Requests;
using System;
using System.Threading.Tasks;

namespace SI24004.Service.Interfaces
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