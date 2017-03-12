using System.Collections.Generic;
using System.Threading.Tasks;
using Bibliotheca.Server.Indexer.AzureSearch.Core.DataTransferObjects;

namespace Bibliotheca.Server.Indexer.AzureSearch.Core.Services
{
    public interface ISearchService
    {
        Task CreateOrUpdateIndexAsync();

        Task UploadDocumentsAsync(string projectId, string branchName, IEnumerable<DocumentIndexDto> documentDtos);

        Task DeleteDocumemntsAsync(string projectId, string branchName);

        Task<DocumentSearchResultDto<DocumentIndexDto>> SearchAsync(FilterDto filter);

        Task<DocumentSearchResultDto<DocumentIndexDto>> SearchAsync(FilterDto filter, string projectId, string branchName);
    }
}