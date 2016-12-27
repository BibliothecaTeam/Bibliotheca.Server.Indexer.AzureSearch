using System.Collections.Generic;
using System.Threading.Tasks;
using Bibliotheca.Server.Indexer.Abstractions;
using Bibliotheca.Server.Indexer.Abstractions.DataTransferObjects;
using Bibliotheca.Server.Indexer.AzureSearch.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bibliotheca.Server.Indexer.AzureSearch.Api.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/search")]
    public class SearchController : Controller, ISearchController
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet]
        public async Task<DocumentSearchResultDto<DocumentIndexDto>> Get([FromQuery] FilterDto filter)
        {
            return await _searchService.SearchAsync(filter);
        }

        
        [HttpGet("projects/{projectId}/branches/{branchName}")]
        public async Task<DocumentSearchResultDto<DocumentIndexDto>> Get([FromQuery] FilterDto filter, string projectId, string branchName)
        {
            return await _searchService.SearchAsync(filter, projectId, branchName);
        }

        [HttpPost("projects/{projectId}/branches/{branchName}")]
        public async Task<IActionResult> Post(string projectId, string branchName, [FromBody] IEnumerable<DocumentIndexDto> documentIndexDtos)
        {
            await _searchService.UploadDocumentsAsync(projectId, branchName, documentIndexDtos);
            return Ok();
        }

        [HttpDelete("projects/{projectId}/branches/{branchName}")]
        public async Task<IActionResult> Delete(string projectId, string branchName)
        {
            await _searchService.DeleteDocumemntsAsync(projectId, branchName);
            return Ok();
        }
    }
}
