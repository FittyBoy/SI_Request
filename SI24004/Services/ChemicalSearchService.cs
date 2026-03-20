using Microsoft.Extensions.Logging;
using SI24004.Controllers;
using SI24004.Models.PostgreSQL;
using SI24004.Models.DTOs;
using SI24004.Repositories.Interfaces;
using SI24004.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace SI24004.Services
{
    public class ChemicalSearchService : IChemicalSearchService
    {
        private readonly IChemicalSearchRepository _repository;
        private readonly ILogger<ChemicalSearchService> _logger;
        private const int DEFAULT_BATCH_CONCURRENCY = 3; // ลดเหลือ 3 เพื่อความเสถียร
        private const int MAX_BATCH_SIZE = 1000;

        public ChemicalSearchService(
            IChemicalSearchRepository repository,
            ILogger<ChemicalSearchService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ChemicalSearchResponse> SearchChemicals(ChemicalSearchRequest request)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                var detectedType = DetectSearchType(request.Query);
                var actualSearchType = IsAutoDetectMode(request.SearchType) ? detectedType : request.SearchType.ToLower();

                var searchOptions = new Repositories.Interfaces.SearchOptions
                {
                    ExactMatch = request.ExactMatch,
                    CaseSensitive = request.CaseSensitive,
                    MaxResults = 10000
                };

                // Search both databases
                var svhcResults = await _repository.SearchQaSubstances(request.Query, actualSearchType, searchOptions);

                var mappedSearchType = MapSearchTypeForRegular(actualSearchType);
                var regularResults = await _repository.SearchRegularSubstands(request.Query, mappedSearchType, searchOptions);

                // Apply pagination
                var (pagedSvhc, pagedRegular, totalCount) = ApplyPagination(
                    svhcResults, regularResults, request.Page, request.PageSize);

                return new ChemicalSearchResponse
                {
                    Svhc = pagedSvhc.Select(MapToQaSubstanceDto).ToList(),
                    Regular = pagedRegular.Select(MapToRegularSubstandDto).ToList(),
                    Total = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    SearchType = actualSearchType,
                    DetectedType = detectedType,
                    SearchTime = (DateTime.UtcNow - startTime).TotalMilliseconds,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchChemicals service");
                throw;
            }
        }

        public async Task<BatchSearchResponse> BatchSearchChemicals(BatchSearchRequest request)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                // Validate batch size
                if (request.Items == null || !request.Items.Any())
                {
                    throw new ArgumentException("Batch items cannot be empty");
                }

                if (request.Items.Count > MAX_BATCH_SIZE)
                {
                    throw new ArgumentException($"Batch size exceeds maximum limit of {MAX_BATCH_SIZE}");
                }

                _logger.LogInformation("Starting batch search with {Count} items", request.Items.Count);

                var batchResults = new BatchSearchResponse
                {
                    Results = new List<BatchSearchResult>(),
                    Summary = new BatchSummary()
                };

                // Filter and deduplicate items
                var processedItems = PreprocessBatchItems(request.Items, request.SkipInvalid);

                if (!processedItems.Any())
                {
                    _logger.LogWarning("No valid items to process after preprocessing");
                    batchResults.Success = true;
                    batchResults.SearchTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    batchResults.Timestamp = DateTime.UtcNow;
                    return batchResults;
                }

                // Use lower concurrency level for stability
                int concurrencyLevel = DEFAULT_BATCH_CONCURRENCY;

                _logger.LogInformation(
                    "Processing {Total} items with concurrency level {Concurrency}",
                    processedItems.Count,
                    concurrencyLevel
                );

                // Process items sequentially to ensure all results are captured
                var results = await ProcessBatchItemsSequentially(processedItems, request);

                _logger.LogInformation("Batch processing completed. Got {Count} results", results.Count);

                // Aggregate results
                foreach (var result in results.OrderBy(r => r.SearchItem))
                {
                    batchResults.Results.Add(result);
                    batchResults.Summary.TotalSearched++;

                    _logger.LogDebug(
                        "Result for {Item}: Found={Found}, SVHC={SvhcCount}, Regular={RegularCount}, Error={Error}",
                        result.SearchItem, result.Found,
                        result.QaSubstances?.Count ?? 0,
                        result.RegularSubstands?.Count ?? 0,
                        result.Error ?? "none"
                    );

                    if (result.Found)
                    {
                        batchResults.Summary.Found++;
                    }
                    else if (!string.IsNullOrEmpty(result.Error))
                    {
                        batchResults.Summary.Errors.Add(new BatchError
                        {
                            Item = result.SearchItem,
                            Message = result.Error
                        });
                    }
                    else
                    {
                        batchResults.Summary.NotFound.Add(result.SearchItem);
                    }
                }

                batchResults.SearchTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                batchResults.Success = true;
                batchResults.Timestamp = DateTime.UtcNow;

                _logger.LogInformation(
                    "Batch search completed: {Total} searched, {Found} found, {NotFound} not found, {Errors} errors in {Time}ms",
                    batchResults.Summary.TotalSearched,
                    batchResults.Summary.Found,
                    batchResults.Summary.NotFound.Count,
                    batchResults.Summary.Errors.Count,
                    batchResults.SearchTime
                );

                return batchResults;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BatchSearchChemicals service");
                throw;
            }
        }

        #region Private Helper Methods - Batch Processing

        private List<BatchItem> PreprocessBatchItems(List<BatchItem> items, bool skipInvalid)
        {
            var validItems = new List<BatchItem>();
            var seenValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in items)
            {
                // Skip null or empty values
                if (string.IsNullOrWhiteSpace(item.Value))
                {
                    if (!skipInvalid)
                    {
                        _logger.LogWarning("Empty item found in batch");
                    }
                    continue;
                }

                // Trim and normalize
                var normalizedValue = item.Value.Trim();

                // Skip duplicates
                if (seenValues.Contains(normalizedValue))
                {
                    _logger.LogDebug("Duplicate item skipped: {Item}", normalizedValue);
                    continue;
                }

                // Validate format
                var detectedType = DetectSearchType(normalizedValue);
                if (skipInvalid && detectedType == "invalid_format")
                {
                    _logger.LogDebug("Invalid format skipped: {Item}", normalizedValue);
                    continue;
                }

                seenValues.Add(normalizedValue);
                validItems.Add(new BatchItem
                {
                    Value = normalizedValue,
                    Type = item.Type
                });

                _logger.LogDebug("Added item: {Item}, Type: {Type}, Detected: {Detected}",
                    normalizedValue, item.Type, detectedType);
            }

            _logger.LogInformation(
                "Preprocessed {Original} items to {Valid} valid unique items",
                items.Count,
                validItems.Count
            );

            return validItems;
        }

        // 🔥 แก้ไขหลัก: ใช้ Sequential Processing แทน Parallel
        private async Task<List<BatchSearchResult>> ProcessBatchItemsSequentially(
            List<BatchItem> items,
            BatchSearchRequest request)
        {
            var results = new List<BatchSearchResult>();
            var processedCount = 0;

            _logger.LogInformation("Starting sequential processing of {Count} items", items.Count);

            foreach (var item in items)
            {
                try
                {
                    processedCount++;
                    _logger.LogDebug("Processing item {Current}/{Total}: {Item}",
                        processedCount, items.Count, item.Value);

                    var result = await ProcessSingleBatchItemWithRetry(item, request);
                    results.Add(result);

                    _logger.LogInformation(
                        "Item {Current}/{Total} completed: {Item} - Found: {Found}, SVHC: {Svhc}, Regular: {Regular}",
                        processedCount, items.Count, item.Value, result.Found,
                        result.QaSubstances?.Count ?? 0,
                        result.RegularSubstands?.Count ?? 0
                    );

                    // Small delay to prevent overwhelming database
                    if (processedCount < items.Count)
                    {
                        await Task.Delay(50);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process item {Current}/{Total}: {Item}",
                        processedCount, items.Count, item.Value);

                    results.Add(new BatchSearchResult
                    {
                        SearchItem = item.Value,
                        ItemType = item.Type,
                        QaSubstances = new List<QaSubstance>(),
                        RegularSubstands = new List<RegularSubstand>(),
                        Found = false,
                        Error = $"Processing error: {ex.Message}"
                    });
                }
            }

            _logger.LogInformation("Sequential processing completed: {Count} results", results.Count);
            return results;
        }

        private async Task<BatchSearchResult> ProcessSingleBatchItemWithRetry(
            BatchItem item,
            BatchSearchRequest request,
            int maxRetries = 3)
        {
            Exception lastException = null;

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    if (attempt > 0)
                    {
                        var delay = 200 * attempt; // 200ms, 400ms, 600ms
                        _logger.LogWarning("Retry attempt {Attempt}/{Max} for item: {Item} (waiting {Delay}ms)",
                            attempt, maxRetries, item.Value, delay);
                        await Task.Delay(delay);
                    }

                    var result = await ProcessSingleBatchItem(item, request);

                    if (attempt > 0)
                    {
                        _logger.LogInformation("Retry successful for item: {Item} on attempt {Attempt}",
                            item.Value, attempt + 1);
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger.LogWarning(ex, "Attempt {Attempt}/{Max} failed for item: {Item}",
                        attempt + 1, maxRetries + 1, item.Value);
                }
            }

            _logger.LogError(lastException, "All {Total} retry attempts failed for item: {Item}",
                maxRetries + 1, item.Value);

            return new BatchSearchResult
            {
                SearchItem = item.Value,
                ItemType = item.Type,
                QaSubstances = new List<QaSubstance>(),
                RegularSubstands = new List<RegularSubstand>(),
                Found = false,
                Error = $"Failed after {maxRetries + 1} attempts: {lastException?.Message}"
            };
        }

        private async Task<BatchSearchResult> ProcessSingleBatchItem(BatchItem item, BatchSearchRequest request)
        {
            var itemStartTime = DateTime.UtcNow;

            try
            {
                // Detect search type
                var detectedType = DetectSearchType(item.Value);
                var requestedType = string.IsNullOrEmpty(item.Type) ? "all" : item.Type;
                var actualSearchType = IsAutoDetectMode(requestedType) ? detectedType : requestedType.ToLower();

                _logger.LogDebug(
                    "Item: {Item} | Requested: {Requested} | Detected: {Detected} | Actual: {Actual}",
                    item.Value, requestedType, detectedType, actualSearchType
                );

                // สร้าง search options โดยใช้ค่าเดียวกับ single search
                var searchOptions = new Repositories.Interfaces.SearchOptions
                {
                    ExactMatch = request.ExactMatch,
                    CaseSensitive = request.CaseSensitive,
                    MaxResults = 10000
                };

                _logger.LogDebug(
                    "Search options: ExactMatch={ExactMatch}, CaseSensitive={CaseSensitive}, MaxResults={MaxResults}",
                    searchOptions.ExactMatch, searchOptions.CaseSensitive, searchOptions.MaxResults
                );

                // ค้นหา SVHC database
                List<QaSubstance> svhcResults = new List<QaSubstance>();
                var svhcStartTime = DateTime.UtcNow;

                try
                {
                    _logger.LogDebug("Searching SVHC for: {Item} with type: {Type}", item.Value, actualSearchType);
                    svhcResults = await _repository.SearchQaSubstances(item.Value, actualSearchType, searchOptions);
                    var svhcTime = (DateTime.UtcNow - svhcStartTime).TotalMilliseconds;
                    _logger.LogInformation(
                        "SVHC search completed: {Item} | Type: {Type} | Results: {Count} | Time: {Time}ms",
                        item.Value, actualSearchType, svhcResults.Count, svhcTime
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "SVHC search failed for: {Item}", item.Value);
                }

                // เพิ่ม delay เล็กน้อยระหว่าง queries
                await Task.Delay(10);

                // ค้นหา Regular database
                List<RegularSubstand> regularResults = new List<RegularSubstand>();
                var regularStartTime = DateTime.UtcNow;

                try
                {
                    var mappedSearchType = MapSearchTypeForRegular(actualSearchType);
                    _logger.LogDebug(
                        "Searching Regular for: {Item} with type: {Type} (mapped from {Original})",
                        item.Value, mappedSearchType, actualSearchType
                    );

                    regularResults = await _repository.SearchRegularSubstands(item.Value, mappedSearchType, searchOptions);
                    var regularTime = (DateTime.UtcNow - regularStartTime).TotalMilliseconds;
                    _logger.LogInformation(
                        "Regular search completed: {Item} | Type: {Type} | Results: {Count} | Time: {Time}ms",
                        item.Value, mappedSearchType, regularResults.Count, regularTime
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Regular search failed for: {Item}", item.Value);
                }

                var totalTime = (DateTime.UtcNow - itemStartTime).TotalMilliseconds;
                var totalFound = svhcResults.Count + regularResults.Count;

                _logger.LogInformation(
                    "Item completed: {Item} | SVHC: {Svhc} | Regular: {Regular} | Total: {Total} | Time: {Time}ms",
                    item.Value, svhcResults.Count, regularResults.Count, totalFound, totalTime
                );

                // สร้าง result
                var result = new BatchSearchResult
                {
                    SearchItem = item.Value,
                    ItemType = actualSearchType,
                    QaSubstances = svhcResults.Select(MapToQaSubstanceDto).ToList(),
                    RegularSubstands = regularResults.Select(MapToRegularSubstandDto).ToList(),
                    Found = svhcResults.Any() || regularResults.Any(),
                };

                // Log ถ้าไม่เจอ
                if (!result.Found)
                {
                    _logger.LogWarning(
                        "NO RESULTS FOUND: {Item} | Type: {Type} | SVHC: 0 | Regular: 0",
                        item.Value, actualSearchType
                    );
                }
                else
                {
                    _logger.LogInformation(
                        "RESULTS FOUND: {Item} | Type: {Type} | SVHC: {Svhc} | Regular: {Regular}",
                        item.Value, actualSearchType, result.QaSubstances.Count, result.RegularSubstands.Count
                    );
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error processing item: {Item}", item.Value);
                throw;
            }
        }

        #endregion

        #region Private Helper Methods - Pagination and Mapping

        private (List<QaSubstance> pagedSvhc, List<RegularSubstand> pagedRegular, int totalCount) ApplyPagination(
            List<QaSubstance> svhcResults, List<RegularSubstand> regularResults, int page, int pageSize)
        {
            var totalResults = svhcResults.Count + regularResults.Count;

            if (page == 1 && pageSize >= totalResults)
            {
                return (svhcResults, regularResults, totalResults);
            }

            var allResults = new List<object>();
            allResults.AddRange(svhcResults);
            allResults.AddRange(regularResults);

            var skip = (page - 1) * pageSize;
            var pagedResults = allResults.Skip(skip).Take(pageSize).ToList();

            return (
                pagedResults.OfType<QaSubstance>().ToList(),
                pagedResults.OfType<RegularSubstand>().ToList(),
                totalResults
            );
        }

        private string MapSearchTypeForRegular(string searchType)
        {
            var mapped = searchType switch
            {
                "cas_no" => "cas_no",
                "ec_no" => "skip",
                "chemical_name" => "chemical",
                "substance_name" => "identifier",
                "all" => "all",
                _ => "all"
            };

            _logger.LogDebug("Mapped search type: {Original} -> {Mapped}", searchType, mapped);
            return mapped;
        }

        private string DetectSearchType(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return "all";

            query = query.Trim();

            if (Regex.IsMatch(query, @"^[0-9]{3}-[0-9]{3}-[0-9]$"))
                return "ec_no";

            if (Regex.IsMatch(query, @"^[0-9]{7}$"))
                return "ec_no";

            if (Regex.IsMatch(query, @"^[0-9]{3}-[0-9]{1,3}$") || Regex.IsMatch(query, @"^[0-9]{3}-[0-9]{3}$"))
                return "ec_no";

            if (Regex.IsMatch(query, @"^[0-9]{1,7}-[0-9]{2}-[0-9]$"))
                return "cas_no";

            if (Regex.IsMatch(query, @"^[0-9\-]+$"))
            {
                var digitCount = Regex.Matches(query, @"\d").Count;
                if (digitCount == 7)
                    return "ec_no";

                return "invalid_format";
            }

            return "chemical_name";
        }

        private bool IsAutoDetectMode(string searchType) =>
            string.IsNullOrEmpty(searchType) || searchType.ToLower() == "all";

        private QaSubstance MapToQaSubstanceDto(QaSubstance entity) => new()
        {
            Id = entity.Id,
            CasNo = entity.CasNo ?? string.Empty,
            EcNo = entity.EcNo ?? string.Empty,
            SubstanceName = entity.SubstanceName ?? string.Empty,
            ReasonForInclusion = entity.ReasonForInclusion ?? string.Empty,
            Uses = entity.Uses ?? string.Empty,
            SvhcCandidate = entity.SvhcCandidate
        };

        private RegularSubstand MapToRegularSubstandDto(RegularSubstand entity) => new()
        {
            Id = entity.Id,
            SubstanceChemical = entity.SubstanceChemical ?? string.Empty,
            SubstanceIdentifier = entity.SubstanceIdentifier ?? string.Empty,
            SubstanceCasNo = entity.SubstanceCasNo ?? string.Empty,
            SubstanceThresholdLimit = entity.SubstanceThresholdLimit ?? string.Empty,
            SubstanceScope = entity.SubstanceScope ?? string.Empty,
            SubstanceExamples = entity.SubstanceExamples ?? string.Empty,
            SubstanceReferences = entity.SubstanceReferences ?? string.Empty
        };

        #endregion
    }
}