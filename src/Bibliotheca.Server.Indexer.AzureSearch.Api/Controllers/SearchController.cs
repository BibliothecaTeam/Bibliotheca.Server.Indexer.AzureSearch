using System.Collections.Generic;
using System.Threading.Tasks;
using Bibliotheca.Server.Indexer.AzureSearch.Core.DataTransferObjects;
using Bibliotheca.Server.Indexer.AzureSearch.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bibliotheca.Server.Indexer.AzureSearch.Api.Controllers
{
    /// <summary>
    /// Controller which manages search index.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/search")]
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="searchService">Search service.</param>
        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        /// <summary>
        /// Search by specific query (filter).
        /// </summary>
        /// <remarks>
        /// Searching is done by connected search service (for example Azure search). 
        /// </remarks>
        /// <param name="filter">Filter which contains query parameters.</param>
        /// <returns>Documents from search service.</returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(DocumentSearchResultDto<DocumentIndexDto>))]
        public async Task<DocumentSearchResultDto<DocumentIndexDto>> Get([FromQuery] FilterDto filter)
        {
            return await _searchService.SearchAsync(filter);
        }

        /// <summary>
        /// Search by specific query (filter) in specific project's branch.
        /// </summary>
        /// <remarks>
        /// Searching is done by connected search service (for example Azure search). Serach is executed only in contect
        /// of specific project and branch.
        /// </remarks>
        /// <param name="filter">Filter which contains query parameters.</param>
        /// <param name="projectId">Project id.</param>
        /// <param name="branchName">Branch name.</param>
        /// <returns>Documents from search service.</returns>
        [HttpGet("projects/{projectId}/branches/{branchName}")]
        [ProducesResponseType(200, Type = typeof(DocumentSearchResultDto<DocumentIndexDto>))]
        public async Task<DocumentSearchResultDto<DocumentIndexDto>> Get([FromQuery] FilterDto filter, string projectId, string branchName)
        {
            return await _searchService.SearchAsync(filter, projectId, branchName);
        }

        /// <summary>
        /// Add a new documentation information to search index.
        /// </summary>
        /// <remarks>
        /// Endpoint is used to adding new documentation files to search index. It's batch operation so you can send multiple files at once.
        /// </remarks>
        /// <param name="projectId">Projetc id.</param>
        /// <param name="branchName">Branch name.</param>
        /// <param name="documentIndexDtos">List of documentations which should be added to search index.</param>
        /// <returns>If all documentas was successfully added endpoint returns 200 (Ok).</returns>
        [HttpPost("projects/{projectId}/branches/{branchName}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Post(string projectId, string branchName, [FromBody] IEnumerable<DocumentIndexDto> documentIndexDtos)
        {
            await _searchService.UploadDocumentsAsync(projectId, branchName, documentIndexDtos);
            return Ok();
        }

        /// <summary>
        /// Delete search index created for branch in specific project.
        /// </summary>
        /// <remarks>
        /// Endpoint removes from search index information about branch and project. Searching shouldn't return 
        /// any documents from that branch and project.
        /// </remarks>
        /// <param name="projectId">Project id.</param>
        /// <param name="branchName">Branch name.</param>
        /// <returns>If deleted successfully endpoint returns 200 (Ok).</returns>
        [HttpDelete("projects/{projectId}/branches/{branchName}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Delete(string projectId, string branchName)
        {
            await _searchService.DeleteDocumemntsAsync(projectId, branchName);
            return Ok();
        }
    }
}
