namespace Bibliotheca.Server.Indexer.AzureSearch.Core.DataTransferObjects
{
    public class FilterDto
    {
        public string Query { get; set; }

        public int Page { get; set; }

        public int Limit { get; set; }
    }
}