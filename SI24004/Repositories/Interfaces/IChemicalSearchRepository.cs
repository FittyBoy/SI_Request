using SI24004.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SI24004.Repositories.Interfaces
{
    public interface IChemicalSearchRepository
    {
        Task<List<QaSubstance>> SearchQaSubstances(string query, string searchType, SearchOptions options);
        Task<List<RegularSubstand>> SearchRegularSubstands(string query, string searchType, SearchOptions options);
        Task<(int qaCount, int regularCount)> GetDatabaseCounts();
    }

    public class SearchOptions
    {
        public bool ExactMatch { get; set; }
        public bool CaseSensitive { get; set; }
        public int MaxResults { get; set; } = 10000;
    }
}
