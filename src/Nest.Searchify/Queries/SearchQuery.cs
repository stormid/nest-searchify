using System.Threading.Tasks;
using Nest.Queryify.Abstractions.Queries;

namespace Nest.Searchify.Queries
{
    public abstract class SearchQuery<TDocument> : ElasticClientQueryObject<ISearchResponse<TDocument>> 
        where TDocument : class
    {
        protected sealed override ISearchResponse<TDocument> ExecuteCore(IElasticClient client, string index)
        {
            return client.Search<TDocument, TDocument>(desc => BuildQuery(desc).Index(index));
        }

        protected sealed override async Task<ISearchResponse<TDocument>> ExecuteCoreAsync(IElasticClient client, string index)
        {
            return await client.SearchAsync<TDocument, TDocument>(desc => BuildQuery(desc).Index(index));
        }

        protected abstract SearchDescriptor<TDocument> BuildQuery(SearchDescriptor<TDocument> descriptor);
    }
}