using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using SI24004.Models.PostgreSQL;
using SI24004.Models.DTOs;
using SI24004.Services;
using SI24004.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SI24004.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SI25011Controller : ControllerBase
    {
        private readonly IChemicalSearchService _searchService;
        private readonly IRegularSubstanceService _regularSubstanceService;
        private readonly ILogger<SI25011Controller> _logger;
        private readonly ISvhcSubstanceService _svhcSubstanceService;

        public SI25011Controller(
            IChemicalSearchService searchService,
            IRegularSubstanceService regularSubstanceService,
            ISvhcSubstanceService svhcSubstanceService,
            ILogger<SI25011Controller> logger)
        {
            _searchService = searchService;
            _regularSubstanceService = regularSubstanceService;
            _logger = logger;
            _svhcSubstanceService = svhcSubstanceService;
        }

        #region Search Operations

        /// <summary>
        /// Search for chemicals in both SVHC and Regular substances databases
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchChemicals([FromQuery] ChemicalSearchRequest request)
        {
            try
            {
                var validationResult = ValidateSearchRequest(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = validationResult.Errors,
                        timestamp = DateTime.UtcNow
                    });
                }

                var response = await _searchService.SearchChemicals(request);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation during search");
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid search operation",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Database timeout during search");
                return StatusCode(408, new
                {
                    success = false,
                    message = "Search request timed out. Please try again.",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during chemical search");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred while searching chemicals",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Batch search for multiple chemicals
        /// </summary>
        [HttpPost("batch-search")]
        public async Task<IActionResult> BatchSearchChemicals([FromBody] BatchSearchRequest request)
        {
            try
            {
                if (request.Items == null || !request.Items.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No items provided for batch search",
                        timestamp = DateTime.UtcNow
                    });
                }

                if (request.Items.Count > 1000)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Batch size too large. Maximum 1000 items allowed.",
                        timestamp = DateTime.UtcNow
                    });
                }

                var response = await _searchService.BatchSearchChemicals(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch search");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred during batch search",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        #endregion

        #region CRUD Operations - Regular Substance

        /// <summary>
        /// Get all regular substances with pagination
        /// </summary>
        [HttpGet("substances")]
        public async Task<IActionResult> GetAllSubstances([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                if (page < 1 || pageSize < 1 || pageSize > 1000)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid pagination parameters",
                        timestamp = DateTime.UtcNow
                    });
                }

                var response = await _regularSubstanceService.GetAllSubstances(page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving substances");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving substances",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
        /// <summary>
        /// Get all regular substances without pagination (for client-side filtering)
        /// </summary>
        [HttpGet("substances/all")]
        public async Task<IActionResult> GetAllSubstancesNoPagination()
        {
            try
            {
                var substances = await _regularSubstanceService.GetAllSubstancesNoPagination();
                return Ok(new
                {
                    success = true,
                    data = substances,
                    totalRecords = substances.Count,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all substances");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving all substances",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
        /// <summary>
        /// Get a specific regular substance by ID
        /// </summary>
        [HttpGet("substances/{id}")]
        public async Task<IActionResult> GetSubstanceById(Guid id)
        {
            try
            {
                var substance = await _regularSubstanceService.GetSubstanceById(id);

                if (substance == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Substance with ID {id} not found",
                        timestamp = DateTime.UtcNow
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = substance,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving substance by ID: {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving the substance",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Create a new regular substance
        /// </summary>
        [HttpPost("substances")]
        public async Task<IActionResult> CreateSubstance([FromBody] RegularSubstanceRequest request)
        {
            try
            {
                var validationResult = ValidateSubstanceRequest(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = validationResult.Errors,
                        timestamp = DateTime.UtcNow
                    });
                }

                var createdSubstance = await _regularSubstanceService.CreateSubstance(request);

                return CreatedAtAction(
                    nameof(GetSubstanceById),
                    new { id = createdSubstance.Id },
                    new
                    {
                        success = true,
                        message = "Substance created successfully",
                        data = createdSubstance,
                        timestamp = DateTime.UtcNow
                    });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation during substance creation");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating substance");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while creating the substance",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Update an existing regular substance
        /// </summary>
        [HttpPut("substances/{id}")]
        public async Task<IActionResult> UpdateSubstance(Guid id, [FromBody] RegularSubstanceRequest request)
        {
            try
            {
                var validationResult = ValidateSubstanceRequest(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = validationResult.Errors,
                        timestamp = DateTime.UtcNow
                    });
                }

                var updatedSubstance = await _regularSubstanceService.UpdateSubstance(id, request);

                if (updatedSubstance == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Substance with ID {id} not found",
                        timestamp = DateTime.UtcNow
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Substance updated successfully",
                    data = updatedSubstance,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation during substance update");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating substance with ID: {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating the substance",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Delete a regular substance
        /// </summary>
        [HttpDelete("substances/{id}")]
        public async Task<IActionResult> DeleteSubstance(Guid id)
        {
            try
            {
                var result = await _regularSubstanceService.DeleteSubstance(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Substance with ID {id} not found",
                        timestamp = DateTime.UtcNow
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Substance deleted successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation during substance deletion");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting substance with ID: {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting the substance",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        #endregion

        #region Validation

        private ValidationResult ValidateSearchRequest(ChemicalSearchRequest request)
        {
            var errors = new List<string>();

            if (request == null)
            {
                errors.Add("Request cannot be null");
                return new ValidationResult { IsValid = false, Errors = errors };
            }

            if (string.IsNullOrWhiteSpace(request.Query))
            {
                errors.Add("Search query is required and cannot be empty");
            }

            if (request.Query?.Length > 500)
            {
                errors.Add("Search query too long. Maximum 500 characters allowed.");
            }

            if (request.Page < 1)
            {
                errors.Add("Page must be greater than 0");
            }

            if (request.PageSize < 1 || request.PageSize > 1000)
            {
                errors.Add("PageSize must be between 1 and 1000");
            }

            var validSearchTypes = new[] { "all", "cas_no", "ec_no", "chemical_name", "substance_name" };
            if (!string.IsNullOrEmpty(request.SearchType) && !validSearchTypes.Contains(request.SearchType.ToLower()))
            {
                errors.Add($"Invalid search type. Valid types are: {string.Join(", ", validSearchTypes)}");
            }

            return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
        }

        private ValidationResult ValidateSubstanceRequest(RegularSubstanceRequest request)
        {
            var errors = new List<string>();

            if (request == null)
            {
                errors.Add("Request cannot be null");
                return new ValidationResult { IsValid = false, Errors = errors };
            }

            if (string.IsNullOrWhiteSpace(request.SubstanceChemical))
            {
                errors.Add("Substance chemical name is required");
            }

            if (request.SubstanceChemical?.Length > 500)
            {
                errors.Add("Substance chemical name too long. Maximum 500 characters.");
            }

            if (string.IsNullOrWhiteSpace(request.SubstanceIdentifier))
            {
                errors.Add("Substance identifier is required");
            }

            if (request.SubstanceIdentifier?.Length > 100)
            {
                errors.Add("Substance identifier too long. Maximum 100 characters.");
            }

            if (string.IsNullOrWhiteSpace(request.SubstanceCasNo))
            {
                errors.Add("CAS number is required");
            }

            if (request.SubstanceCasNo?.Length > 50)
            {
                errors.Add("CAS number too long. Maximum 50 characters.");
            }

            return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
        }

        #endregion

        #region CRUD Operations - SVHC Substance

        /// <summary>
        /// Get all SVHC substances with pagination
        /// </summary>
        /// <summary>
        /// Get all SVHC substances with pagination
        /// </summary>
        [HttpGet("svhc")]
        public async Task<IActionResult> GetAllSvhcSubstances([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                if (page < 1 || pageSize < 1 || pageSize > 1000)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid pagination parameters",
                        timestamp = DateTime.UtcNow
                    });
                }

                var response = await _svhcSubstanceService.GetAllSubstances(page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving SVHC substances");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving SVHC substances",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Get all SVHC substances without pagination (for client-side filtering)
        /// </summary>
        [HttpGet("svhc/all")]
        public async Task<IActionResult> GetAllSvhcSubstancesNoPagination()
        {
            try
            {
                var substances = await _svhcSubstanceService.GetAllSubstancesNoPagination();
                return Ok(new
                {
                    success = true,
                    data = substances,
                    totalRecords = substances.Count,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all SVHC substances");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving all SVHC substances",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Get a specific SVHC substance by ID
        /// </summary>
        [HttpGet("svhc/{id}")]
        public async Task<IActionResult> GetSvhcSubstanceById(Guid id)
        {
            try
            {
                var substance = await _svhcSubstanceService.GetSubstanceById(id);

                if (substance == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"SVHC substance with ID {id} not found",
                        timestamp = DateTime.UtcNow
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = substance,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving SVHC substance by ID: {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving the SVHC substance",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Create a new SVHC substance
        /// </summary>
        [HttpPost("svhc")]
        public async Task<IActionResult> CreateSvhcSubstance([FromBody] SvhcSubstanceRequest request)
        {
            try
            {
                var validationResult = ValidateSvhcSubstanceRequest(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = validationResult.Errors,
                        timestamp = DateTime.UtcNow
                    });
                }

                var createdSubstance = await _svhcSubstanceService.CreateSubstance(request);

                return CreatedAtAction(
                    nameof(GetSvhcSubstanceById),
                    new { id = createdSubstance.Id },
                    new
                    {
                        success = true,
                        message = "SVHC substance created successfully",
                        data = createdSubstance,
                        timestamp = DateTime.UtcNow
                    });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation during SVHC substance creation");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating SVHC substance");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while creating the SVHC substance",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Update an existing SVHC substance
        /// </summary>
        [HttpPut("svhc/{id}")]
        public async Task<IActionResult> UpdateSvhcSubstance(Guid id, [FromBody] SvhcSubstanceRequest request)
        {
            try
            {
                var validationResult = ValidateSvhcSubstanceRequest(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = validationResult.Errors,
                        timestamp = DateTime.UtcNow
                    });
                }

                var updatedSubstance = await _svhcSubstanceService.UpdateSubstance(id, request);

                if (updatedSubstance == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"SVHC substance with ID {id} not found",
                        timestamp = DateTime.UtcNow
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "SVHC substance updated successfully",
                    data = updatedSubstance,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation during SVHC substance update");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating SVHC substance with ID: {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating the SVHC substance",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Delete a SVHC substance
        /// </summary>
        [HttpDelete("svhc/{id}")]
        public async Task<IActionResult> DeleteSvhcSubstance(Guid id)
        {
            try
            {
                var result = await _svhcSubstanceService.DeleteSubstance(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"SVHC substance with ID {id} not found",
                        timestamp = DateTime.UtcNow
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "SVHC substance deleted successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation during SVHC substance deletion");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting SVHC substance with ID: {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting the SVHC substance",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        #endregion

        #region SVHC Validation

        private ValidationResult ValidateSvhcSubstanceRequest(SvhcSubstanceRequest request)
        {
            var errors = new List<string>();

            if (request == null)
            {
                errors.Add("Request cannot be null");
                return new ValidationResult { IsValid = false, Errors = errors };
            }

            if (string.IsNullOrWhiteSpace(request.SubstanceName))
            {
                errors.Add("Substance name is required");
            }

            if (request.SubstanceName?.Length > 500)
            {
                errors.Add("Substance name too long. Maximum 500 characters.");
            }

            if (string.IsNullOrWhiteSpace(request.CasNo))
            {
                errors.Add("CAS number is required");
            }

            if (request.CasNo?.Length > 50)
            {
                errors.Add("CAS number too long. Maximum 50 characters.");
            }

            if (!string.IsNullOrWhiteSpace(request.EcNo) && request.EcNo.Length > 50)
            {
                errors.Add("EC number too long. Maximum 50 characters.");
            }

            if (!string.IsNullOrWhiteSpace(request.ReasonForInclusion) && request.ReasonForInclusion.Length > 500)
            {
                errors.Add("Reason for inclusion too long. Maximum 500 characters.");
            }

            if (!string.IsNullOrWhiteSpace(request.Uses) && request.Uses.Length > 1000)
            {
                errors.Add("Uses too long. Maximum 1000 characters.");
            }

            return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
        }

        #endregion
    }
}