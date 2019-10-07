using System;
using System.Reflection;
using System.Threading.Tasks;
using Nest.Queryify.Abstractions.Queries;
using Nest.Searchify.Abstractions;
using Nest.Searchify.SearchResults;

namespace Nest.Searchify.Queries
{
    public abstract class SearchResultQuery<TParameters, TDocument, TSearchResult> : SearchResultQuery<TParameters, TDocument, TSearchResult, TDocument>
        where TDocument : class
        where TParameters : class, IPagingParameters, ISortingParameters, new()
        where TSearchResult : SearchResult<TParameters, TDocument>
    {
        protected SearchResultQuery(TParameters parameters) : base(parameters)
        {
        }
    }

    public abstract class SearchResultQuery<TParameters, TDocument, TSearchResult, TOutputEntity> : ElasticClientQueryObject<TSearchResult>, ISearchResultQuery<TParameters>
        where TDocument : class
        where TOutputEntity : class
        where TParameters : class, IPagingParameters, ISortingParameters, new()
        where TSearchResult : SearchResult<TParameters, TDocument, TOutputEntity>
    {
        public TParameters Parameters { get; }

        protected SearchResultQuery(TParameters parameters)
        {
            Parameters = parameters;
        }

        protected override TSearchResult ExecuteCore(IElasticClient client, string index)
        {
            var response = client.Search<TDocument, TDocument>(desc => BuildQuery(desc.TypedKeys(false)).Index(index));
            return ToSearchResultCore(response, Parameters);
        }

        protected override async Task<TSearchResult> ExecuteCoreAsync(IElasticClient client, string index)
        {
            var response = await client.SearchAsync<TDocument, TDocument>(desc => BuildQuery(desc.TypedKeys(false)).Index(index));
            return ToSearchResultCore(response, Parameters);
        }

        protected TSearchResult ToSearchResultCore(ISearchResponse<TDocument> response, TParameters parameters)
        {
            return ToSearchResult(response, parameters);
        }

        protected virtual TSearchResult ToSearchResult(ISearchResponse<TDocument> response, TParameters parameters)
        {
            if(typeof(TSearchResult).GetTypeInfo().IsAbstract) throw new NotSupportedException("When using a query with a custom transform from TDocument to TOutputEntity you must derive a custom SearchResult from SearchResult<TParameters, TDocument, TOutputEntity> or override ToSearchResult within a Query");
            if (typeof(TSearchResult).GetTypeInfo().IsInterface) throw new InvalidCastException("Cant create instance of interface, please override ToSearchResult");

            return (TSearchResult)Activator.CreateInstance(typeof(TSearchResult), parameters, response);
        }

        protected abstract SearchDescriptor<TDocument> BuildQuery(SearchDescriptor<TDocument> descriptor);
    }
}