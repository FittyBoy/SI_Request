using SI24004.Controllers;
using SI24004.Models;
using SI24004.Models.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SI24004.Service.Interfaces
{
    public interface IChemicalSearchService
    {
        Task<ChemicalSearchResponse> SearchChemicals(ChemicalSearchRequest request);
        Task<BatchSearchResponse> BatchSearchChemicals(BatchSearchRequest request);
    }
}
