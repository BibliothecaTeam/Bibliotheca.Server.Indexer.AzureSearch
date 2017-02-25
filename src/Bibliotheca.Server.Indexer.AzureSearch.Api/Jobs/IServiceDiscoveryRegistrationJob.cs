using System.Threading.Tasks;
using Hangfire.Server;

namespace Bibliotheca.Server.Indexer.AzureSearch.Jobs
{
    public interface IServiceDiscoveryRegistrationJob
    {
        Task RegisterServiceAsync(PerformContext context);
    }
}