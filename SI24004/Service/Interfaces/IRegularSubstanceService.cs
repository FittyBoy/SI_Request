using SI24004.Models;
using SI24004.Models.Requests;
using SI24004.Models.Responses;
using System;
using System.Threading.Tasks;

namespace SI24004.Service.Interfaces
{
    public interface IRegularSubstanceService
    {
        Task<SubstanceListResponse> GetAllSubstances(int page, int pageSize);
        Task<List<RegularSubstand>> GetAllSubstancesNoPagination();
        Task<RegularSubstand> GetSubstanceById(Guid id);
        Task<RegularSubstand> CreateSubstance(RegularSubstanceRequest request);
        Task<RegularSubstand> UpdateSubstance(Guid id, RegularSubstanceRequest request);
        Task<bool> DeleteSubstance(Guid id);
    }
}
