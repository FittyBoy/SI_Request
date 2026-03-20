using SI24004.Controllers;
using SI24004.Models.PostgreSQL;
using SI24004.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SI24004.Services.Interfaces
{
    public interface IChemicalSearchService
    {
        Task<ChemicalSearchResponse> SearchChemicals(ChemicalSearchRequest request);
        Task<BatchSearchResponse> BatchSearchChemicals(BatchSearchRequest request);
    }
}
