using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SI24004.Models;
using SI24004.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SI24004.Repositories
{
    public class ChemicalSearchRepository : IChemicalSearchRepository
    {
        private readonly PostgrestContext _context;
        private readonly ILogger<ChemicalSearchRepository> _logger;

        public ChemicalSearchRepository(PostgrestContext context, ILogger<ChemicalSearchRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<QaSubstance>> SearchQaSubstances(string query, string searchType, SearchOptions options)
        {
            try
            {
                var queryBuilder = _context.QaSubstances.AsQueryable();
                queryBuilder = ApplyQaSubstanceFilters(queryBuilder, query, searchType, options);

                var results = await queryBuilder
                    .Take(options.MaxResults)
                    .ToListAsync();

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching QaSubstances");
                return new List<QaSubstance>();
            }
        }

        public async Task<List<RegularSubstand>> SearchRegularSubstands(string query, string searchType, SearchOptions options)
        {
            try
            {
                if (searchType == "skip" || searchType == "ec_no")
                {
                    return new List<RegularSubstand>();
                }

                var queryBuilder = _context.RegularSubstands.AsQueryable();
                queryBuilder = ApplyRegularSubstandFilters(queryBuilder, query, searchType, options);

                var results = await queryBuilder
                    .Take(options.MaxResults)
                    .ToListAsync();

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching RegularSubstands");
                return new List<RegularSubstand>();
            }
        }

        public async Task<(int qaCount, int regularCount)> GetDatabaseCounts()
        {
            try
            {
                var qaCount = await _context.QaSubstances.CountAsync();
                var regularCount = await _context.RegularSubstands.CountAsync();
                return (qaCount, regularCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database counts");
                throw new InvalidOperationException("Database is not accessible", ex);
            }
        }

        #region Private Filter Methods

        private IQueryable<QaSubstance> ApplyQaSubstanceFilters(
            IQueryable<QaSubstance> query, string searchQuery, string searchType, SearchOptions options)
        {
            var queryLower = options.CaseSensitive ? searchQuery : searchQuery.ToLower();

            return searchType switch
            {
                "cas_no" => ApplyCasNoFilter(query, searchQuery, queryLower, options),
                "ec_no" => ApplyEcNoFilter(query, searchQuery, queryLower, options),
                "chemical_name" or "substance_name" => ApplySubstanceNameFilter(query, searchQuery, queryLower, options),
                _ => ApplyAllFieldsFilterQa(query, searchQuery, queryLower, options)
            };
        }

        private IQueryable<RegularSubstand> ApplyRegularSubstandFilters(
            IQueryable<RegularSubstand> query, string searchQuery, string searchType, SearchOptions options)
        {
            var queryLower = options.CaseSensitive ? searchQuery : searchQuery.ToLower();

            return searchType switch
            {
                "cas_no" => ApplyCasNoFilterRegular(query, searchQuery, queryLower, options),
                "chemical" => ApplyChemicalNameFilter(query, searchQuery, queryLower, options),
                "identifier" => ApplyIdentifierFilter(query, searchQuery, queryLower, options),
                _ => ApplyAllFieldsFilterRegular(query, searchQuery, queryLower, options)
            };
        }

        // QaSubstance Filters
        private IQueryable<QaSubstance> ApplyCasNoFilter(
            IQueryable<QaSubstance> query, string original, string lower, SearchOptions options)
        {
            var cleanOriginal = original.Trim();
            var cleanLower = cleanOriginal.ToLower();
            var withoutDashes = cleanOriginal.Replace("-", "");
            var withoutDashesLower = withoutDashes.ToLower();

            if (options.ExactMatch)
            {
                return options.CaseSensitive
                    ? query.Where(x => x.CasNo != null && (
                        x.CasNo == cleanOriginal ||
                        x.CasNo.Replace("-", "") == withoutDashes))
                    : query.Where(x => x.CasNo != null && (
                        x.CasNo.ToLower() == cleanLower ||
                        x.CasNo.ToLower().Replace("-", "") == withoutDashesLower));
            }
            else
            {
                return options.CaseSensitive
                    ? query.Where(x => x.CasNo != null && (
                        x.CasNo.Contains(cleanOriginal) ||
                        x.CasNo.Replace("-", "").Contains(withoutDashes)))
                    : query.Where(x => x.CasNo != null && (
                        x.CasNo.ToLower().Contains(cleanLower) ||
                        x.CasNo.ToLower().Replace("-", "").Contains(withoutDashesLower)));
            }
        }

        private IQueryable<QaSubstance> ApplyEcNoFilter(
            IQueryable<QaSubstance> query, string original, string lower, SearchOptions options)
        {
            var cleanOriginal = original.Trim();
            var cleanLower = cleanOriginal.ToLower();
            var withoutDashes = cleanOriginal.Replace("-", "");
            var withoutDashesLower = withoutDashes.ToLower();

            var standardFormat = "";
            if (!cleanOriginal.Contains("-") && cleanOriginal.Length == 7)
            {
                standardFormat = $"{cleanOriginal.Substring(0, 3)}-{cleanOriginal.Substring(3, 3)}-{cleanOriginal.Substring(6, 1)}";
            }

            if (options.ExactMatch)
            {
                if (options.CaseSensitive)
                {
                    return query.Where(x => x.EcNo != null && (
                        x.EcNo == cleanOriginal ||
                        x.EcNo.Replace("-", "") == withoutDashes ||
                        (!string.IsNullOrEmpty(standardFormat) && x.EcNo == standardFormat)));
                }
                else
                {
                    return query.Where(x => x.EcNo != null && (
                        x.EcNo.ToLower() == cleanLower ||
                        x.EcNo.ToLower().Replace("-", "") == withoutDashesLower ||
                        (!string.IsNullOrEmpty(standardFormat) && x.EcNo.ToLower() == standardFormat.ToLower())));
                }
            }
            else
            {
                if (options.CaseSensitive)
                {
                    return query.Where(x => x.EcNo != null && (
                        x.EcNo.Contains(cleanOriginal) ||
                        x.EcNo.Replace("-", "").Contains(withoutDashes) ||
                        (!string.IsNullOrEmpty(standardFormat) && x.EcNo.Contains(standardFormat))));
                }
                else
                {
                    return query.Where(x => x.EcNo != null && (
                        x.EcNo.ToLower().Contains(cleanLower) ||
                        x.EcNo.ToLower().Replace("-", "").Contains(withoutDashesLower) ||
                        (!string.IsNullOrEmpty(standardFormat) && x.EcNo.ToLower().Contains(standardFormat.ToLower()))));
                }
            }
        }

        private IQueryable<QaSubstance> ApplySubstanceNameFilter(
            IQueryable<QaSubstance> query, string original, string lower, SearchOptions options)
        {
            if (options.ExactMatch)
            {
                return options.CaseSensitive
                    ? query.Where(x => x.SubstanceName != null && x.SubstanceName == original)
                    : query.Where(x => x.SubstanceName != null && x.SubstanceName.ToLower() == lower);
            }
            else
            {
                return options.CaseSensitive
                    ? query.Where(x => x.SubstanceName != null && x.SubstanceName.Contains(original))
                    : query.Where(x => x.SubstanceName != null && x.SubstanceName.ToLower().Contains(lower));
            }
        }

        private IQueryable<QaSubstance> ApplyAllFieldsFilterQa(
            IQueryable<QaSubstance> query, string original, string lower, SearchOptions options)
        {
            var cleanOriginal = original.Trim();
            var cleanLower = cleanOriginal.ToLower();
            var withoutDashes = cleanOriginal.Replace("-", "");
            var withoutDashesLower = withoutDashes.ToLower();

            if (options.ExactMatch)
            {
                return options.CaseSensitive
                    ? query.Where(x =>
                        (x.CasNo != null && (x.CasNo == cleanOriginal || x.CasNo.Replace("-", "") == withoutDashes)) ||
                        (x.EcNo != null && (x.EcNo == cleanOriginal || x.EcNo.Replace("-", "") == withoutDashes)) ||
                        (x.SubstanceName != null && x.SubstanceName == cleanOriginal))
                    : query.Where(x =>
                        (x.CasNo != null && (x.CasNo.ToLower() == cleanLower || x.CasNo.ToLower().Replace("-", "") == withoutDashesLower)) ||
                        (x.EcNo != null && (x.EcNo.ToLower() == cleanLower || x.EcNo.ToLower().Replace("-", "") == withoutDashesLower)) ||
                        (x.SubstanceName != null && x.SubstanceName.ToLower() == cleanLower));
            }
            else
            {
                return options.CaseSensitive
                    ? query.Where(x =>
                        (x.CasNo != null && (x.CasNo.Contains(cleanOriginal) || x.CasNo.Replace("-", "").Contains(withoutDashes))) ||
                        (x.EcNo != null && (x.EcNo.Contains(cleanOriginal) || x.EcNo.Replace("-", "").Contains(withoutDashes))) ||
                        (x.SubstanceName != null && x.SubstanceName.Contains(cleanOriginal)))
                    : query.Where(x =>
                        (x.CasNo != null && (x.CasNo.ToLower().Contains(cleanLower) || x.CasNo.ToLower().Replace("-", "").Contains(withoutDashesLower))) ||
                        (x.EcNo != null && (x.EcNo.ToLower().Contains(cleanLower) || x.EcNo.ToLower().Replace("-", "").Contains(withoutDashesLower))) ||
                        (x.SubstanceName != null && x.SubstanceName.ToLower().Contains(cleanLower)));
            }
        }

        // RegularSubstand Filters
        private IQueryable<RegularSubstand> ApplyCasNoFilterRegular(
            IQueryable<RegularSubstand> query, string original, string lower, SearchOptions options)
        {
            var cleanOriginal = original.Trim();
            var cleanLower = cleanOriginal.ToLower();
            var withoutDashes = cleanOriginal.Replace("-", "");
            var withoutDashesLower = withoutDashes.ToLower();

            if (options.ExactMatch)
            {
                return options.CaseSensitive
                    ? query.Where(x => x.SubstanceCasNo != null && (
                        x.SubstanceCasNo == cleanOriginal ||
                        x.SubstanceCasNo.Replace("-", "") == withoutDashes))
                    : query.Where(x => x.SubstanceCasNo != null && (
                        x.SubstanceCasNo.ToLower() == cleanLower ||
                        x.SubstanceCasNo.ToLower().Replace("-", "") == withoutDashesLower));
            }
            else
            {
                return options.CaseSensitive
                    ? query.Where(x => x.SubstanceCasNo != null && (
                        x.SubstanceCasNo.Contains(cleanOriginal) ||
                        x.SubstanceCasNo.Replace("-", "").Contains(withoutDashes)))
                    : query.Where(x => x.SubstanceCasNo != null && (
                        x.SubstanceCasNo.ToLower().Contains(cleanLower) ||
                        x.SubstanceCasNo.ToLower().Replace("-", "").Contains(withoutDashesLower)));
            }
        }

        private IQueryable<RegularSubstand> ApplyChemicalNameFilter(
            IQueryable<RegularSubstand> query, string original, string lower, SearchOptions options)
        {
            if (options.ExactMatch)
            {
                return options.CaseSensitive
                    ? query.Where(x => x.SubstanceChemical != null && x.SubstanceChemical == original)
                    : query.Where(x => x.SubstanceChemical != null && x.SubstanceChemical.ToLower() == lower);
            }
            else
            {
                return options.CaseSensitive
                    ? query.Where(x => x.SubstanceChemical != null && x.SubstanceChemical.Contains(original))
                    : query.Where(x => x.SubstanceChemical != null && x.SubstanceChemical.ToLower().Contains(lower));
            }
        }

        private IQueryable<RegularSubstand> ApplyIdentifierFilter(
            IQueryable<RegularSubstand> query, string original, string lower, SearchOptions options)
        {
            if (options.ExactMatch)
            {
                return options.CaseSensitive
                    ? query.Where(x => x.SubstanceIdentifier != null && x.SubstanceIdentifier == original)
                    : query.Where(x => x.SubstanceIdentifier != null && x.SubstanceIdentifier.ToLower() == lower);
            }
            else
            {
                return options.CaseSensitive
                    ? query.Where(x => x.SubstanceIdentifier != null && x.SubstanceIdentifier.Contains(original))
                    : query.Where(x => x.SubstanceIdentifier != null && x.SubstanceIdentifier.ToLower().Contains(lower));
            }
        }

        private IQueryable<RegularSubstand> ApplyAllFieldsFilterRegular(
            IQueryable<RegularSubstand> query, string original, string lower, SearchOptions options)
        {
            var cleanOriginal = original.Trim();
            var cleanLower = cleanOriginal.ToLower();
            var withoutDashes = cleanOriginal.Replace("-", "");
            var withoutDashesLower = withoutDashes.ToLower();

            if (options.ExactMatch)
            {
                return options.CaseSensitive
                    ? query.Where(x =>
                        (x.SubstanceCasNo != null && (x.SubstanceCasNo == cleanOriginal || x.SubstanceCasNo.Replace("-", "") == withoutDashes)) ||
                        (x.SubstanceChemical != null && x.SubstanceChemical == cleanOriginal) ||
                        (x.SubstanceIdentifier != null && x.SubstanceIdentifier == cleanOriginal))
                    : query.Where(x =>
                        (x.SubstanceCasNo != null && (x.SubstanceCasNo.ToLower() == cleanLower || x.SubstanceCasNo.ToLower().Replace("-", "") == withoutDashesLower)) ||
                        (x.SubstanceChemical != null && x.SubstanceChemical.ToLower() == cleanLower) ||
                        (x.SubstanceIdentifier != null && x.SubstanceIdentifier.ToLower() == cleanLower));
            }
            else
            {
                return options.CaseSensitive
                    ? query.Where(x =>
                        (x.SubstanceCasNo != null && (x.SubstanceCasNo.Contains(cleanOriginal) || x.SubstanceCasNo.Replace("-", "").Contains(withoutDashes))) ||
                        (x.SubstanceChemical != null && x.SubstanceChemical.Contains(cleanOriginal)) ||
                        (x.SubstanceIdentifier != null && x.SubstanceIdentifier.Contains(cleanOriginal)))
                    : query.Where(x =>
                        (x.SubstanceCasNo != null && (x.SubstanceCasNo.ToLower().Contains(cleanLower) || x.SubstanceCasNo.ToLower().Replace("-", "").Contains(withoutDashesLower))) ||
                        (x.SubstanceChemical != null && x.SubstanceChemical.ToLower().Contains(cleanLower)) ||
                        (x.SubstanceIdentifier != null && x.SubstanceIdentifier.ToLower().Contains(cleanLower)));
            }
        }

        #endregion
    }
}
