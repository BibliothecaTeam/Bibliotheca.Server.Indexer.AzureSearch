using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bibliotheca.Server.Indexer.Abstractions.DataTransferObjects;
using Bibliotheca.Server.Indexer.AzureSearch.Core.Parameters;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Options;

namespace Bibliotheca.Server.Indexer.AzureSearch.Core.Services
{
    public class SearchService : ISearchService
    {
        private const string IndexName = "documents";
        private readonly IOptions<ApplicationParameters> _applicationParameters;

        public SearchService(IOptions<ApplicationParameters> applicationParameters)
        {
            _applicationParameters = applicationParameters;
        }

        public async Task CreateOrUpdateIndexAsync()
        {
            SearchServiceClient serviceClient = GetSearchServiceClient();
            await CreateOrUpdateIndexAsync(serviceClient);
        }

        public async Task UploadDocumentsAsync(string projectId, string branchName, IEnumerable<DocumentIndexDto> documentDtos)
        {
            if(!IsSearchIndexEnabled())
            {
                return;
            }

            SearchIndexClient indexClient = GetSearchIndexClient();
            await UploadDocumentsAsync(indexClient, documentDtos);
        }

        public async Task DeleteDocumemntsAsync(string projectId, string branchName)
        {
            if (!IsSearchIndexEnabled())
            {
                return;
            }

            SearchIndexClient indexClient = GetSearchIndexClient();
            await DeleteDocumemntsAsync(indexClient, projectId, branchName);
        }


        public async Task<DocumentSearchResultDto<DocumentIndexDto>> SearchAsync(FilterDto filter)
        {
            return await SearchAsync(filter, null, null);
        }

        public async Task<DocumentSearchResultDto<DocumentIndexDto>> SearchAsync(FilterDto filter, string projectId, string branchName)
        {
            SearchIndexClient indexClient = GetSearchIndexClient();

            int? skip = null;
            int? top = null;
            if (filter.Limit > 0)
            {
                skip = filter.Page * filter.Limit;
                top = filter.Limit;
            }

            string branchFilter = null;
            if (!string.IsNullOrWhiteSpace(projectId) && !string.IsNullOrWhiteSpace(branchName))
            {
                branchFilter = $"projectId eq '{projectId}' and branchName eq '{branchName}'";
            }

            return await SearchDocumentsAsync(indexClient, filter.Query, skip, top, branchFilter);
        }

        private async Task<DocumentSearchResultDto<DocumentIndexDto>> SearchDocumentsAsync(SearchIndexClient indexClient, string query, int? skip = null, int? top = null, string filter = null)
        {
            var searchParameters = new SearchParameters();
            searchParameters.HighlightFields = new[] { "content" };
            searchParameters.QueryType = QueryType.Full;
            searchParameters.IncludeTotalResultCount = true;
            searchParameters.Skip = skip;
            searchParameters.Top = top;
            searchParameters.SearchMode = SearchMode.All;

            if (!string.IsNullOrEmpty(filter))
            {
                searchParameters.Filter = filter;
            }

            DocumentSearchResult<DocumentIndexDto> response = await indexClient.Documents.SearchAsync<DocumentIndexDto>(query, searchParameters);

            var documentSearchResultDto = ChangeToDto(response);
            return documentSearchResultDto;
        }

        private DocumentSearchResultDto<DocumentIndexDto> ChangeToDto(DocumentSearchResult<DocumentIndexDto> response)
        {
            var documentSearchResultDto = new DocumentSearchResultDto<DocumentIndexDto>();
            documentSearchResultDto.NumberOfResults = 0;
            if (response.Count.HasValue)
            {
                documentSearchResultDto.NumberOfResults = Convert.ToInt32(response.Count.Value);
            }

            foreach (var item in response.Results)
            {
                var searchResultDto = new SearchResultDto<DocumentIndexDto>
                {
                    Score = item.Score,
                    Document = item.Document
                };

                foreach (var highlight in item.Highlights)
                {
                    searchResultDto.Highlights.Add(highlight.Key, highlight.Value);
                }

                documentSearchResultDto.Results.Add(searchResultDto);
            }

            return documentSearchResultDto;
        }

        private async Task DeleteDocumemntsAsync(SearchIndexClient indexClient, string projectId, string branchName)
        {
            var documents = await SearchDocumentsAsync(indexClient, "*", null, null, $"projectId eq '{projectId}' and branchName eq '{branchName}'");
            if (documents != null && documents.Results != null && documents.Results.Count > 0)
            {
                var keyValues = documents.Results.Select(x => x.Document.Id);

                try
                {
                    var batch = IndexBatch.Delete("id", keyValues);
                    await indexClient.Documents.IndexAsync(batch);
                }
                catch (IndexBatchException e)
                {
                    Console.WriteLine("Failed to index some of the documents: {0}",
                        string.Join(", ", e.IndexingResults.Where(r => !r.Succeeded).Select(r => r.Key)));
                }
            }
        }

        private async Task UploadDocumentsAsync(SearchIndexClient indexClient, IEnumerable<DocumentIndexDto> documents)
        {
            try
            {
                var batch = IndexBatch.MergeOrUpload(documents);
                await indexClient.Documents.IndexAsync(batch);
            }
            catch (IndexBatchException e)
            {
                Console.WriteLine("Failed to index some of the documents: {0}",
                    string.Join(", ", e.IndexingResults.Where(r => !r.Succeeded).Select(r => r.Key)));
            }
        }

        private async Task CreateOrUpdateIndexAsync(SearchServiceClient serviceClient)
        {
            var definition = new Index()
            {
                Name = IndexName,
                Fields = new[]
                {
                    new Field("id", DataType.String) { IsKey = true },
                    new Field("url", DataType.String) { IsRetrievable = true },
                    new Field("title", DataType.String) { IsRetrievable = true },
                    new Field("projectId", DataType.String) { IsRetrievable = true, IsFilterable = true },
                    new Field("projectName", DataType.String) { IsRetrievable = true, IsFilterable = true },
                    new Field("branchName", DataType.String) { IsRetrievable = true, IsFilterable = true },
                    new Field("tags", DataType.Collection(DataType.String)) { IsRetrievable = true, IsFilterable = true, IsFacetable = true },
                    new Field("content", DataType.String) { IsSearchable = true }
                }
            };

            await serviceClient.Indexes.CreateOrUpdateAsync(definition);
        }

        private SearchIndexClient GetSearchIndexClient()
        {
            SearchServiceClient serviceClient = GetSearchServiceClient();
            SearchIndexClient indexClient = serviceClient.Indexes.GetClient(IndexName);
            return indexClient;
        }

        private SearchServiceClient GetSearchServiceClient()
        {
            string searchServiceName = _applicationParameters.Value.AzureSearchServiceName;
            string apiKey = _applicationParameters.Value.AzureSearchApiKey;

            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(apiKey));
            return serviceClient;
        }

        private bool IsSearchIndexEnabled()
        {
            if(!string.IsNullOrWhiteSpace(_applicationParameters.Value.AzureSearchServiceName) &&
                !string.IsNullOrWhiteSpace(_applicationParameters.Value.AzureSearchApiKey))
            {
                return true;
            }

            return false;
        }
    }
}