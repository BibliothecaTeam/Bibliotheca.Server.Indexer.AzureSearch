namespace Bibliotheca.Server.Indexer.AzureSearch.Core.Parameters
{
    public class ApplicationParameters
    {
        public string AzureSearchServiceName { get; set; }

        public string AzureSearchIndexName { get; set; }

        public string AzureSearchApiKey { get; set; }

        public string SecurityToken { get; set; }

        public string OAuthAuthority { get; set; }

        public string OAuthAudience { get; set; }

        public ServiceDiscovery ServiceDiscovery { get; set; }
    }
}