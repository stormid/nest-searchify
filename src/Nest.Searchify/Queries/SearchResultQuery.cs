using System;
using System.Reflection;
using System.Threading.Tasks;
using Nest.Queryify.Abstractions.Queries;
using Nest.Searchify.Abstractions;
using Nest.Searchify.SearchResults;

namespace Nest.Searchify.Queries
{
    public abstract class SearchResultQuery<TParameters, TDocument, TSearchResult, TReturnAs> : ElasticClientQueryObject<TSearchResult>, ISearchResultQuery<TParameters> 
        where TDocument : class
        where TReturnAs : class
        where TParameters : class, ICommonParameters
        where TSearchResult : SearchResult<TParameters, TDocument>
    {
        public TParameters Parameters { get; }

        protected SearchResultQuery(TParameters parameters)
        {
            Parameters = parameters;
        }

        protected override TSearchResult ExecuteCore(IElasticClient client, string index)
        {
            var response = client.Search<TDocument, TReturnAs>(desc => BuildQuery(desc).Index(index));
            return ToSearchResultCore(response, Parameters);
        }

        protected override Task<TSearchResult> ExecuteCoreAsync(IElasticClient client, string index)
        {
            return client
                .SearchAsync<TDocument, TReturnAs>(desc => BuildQuery(desc).Index(index))
                .ContinueWith(r => ToSearchResultCore(r.Result, Parameters));
        }

        protected virtual TSearchResult ToSearchResultCore(ISearchResponse<TReturnAs> response, TParameters parameters)
        {
            return ToSearchResult(response, parameters);
        }

        protected virtual TSearchResult ToSearchResult(ISearchResponse<TReturnAs> response, TParameters parameters)
        {
            return (TSearchResult)Activator.CreateInstance(typeof(TSearchResult), parameters, response);
        }

        protected abstract SearchDescriptor<TDocument> BuildQuery(SearchDescriptor<TDocument> descriptor);
    }

    public abstract class SearchResultQuery<TSearchParameters, TDocument> :
        SearchResultQuery<TSearchParameters, TDocument, ISearchResult<TSearchParameters, TDocument>>
        where TDocument : class
        where TSearchParameters : class, ICommonParameters
    {
        protected SearchResultQuery(TSearchParameters parameters) : base(parameters)
        {
        }
    }

    public abstract class SearchResultQuery<TParameters, TDocument, TSearchResult> : ElasticClientQueryObject<TSearchResult>, ISearchResultQuery<TParameters> 
        where TDocument : class
        where TParameters : class, ICommonParameters
        where TSearchResult : class, ISearchResult<TParameters, TDocument>
    {
        public TParameters Parameters { get; }

        protected SearchResultQuery(TParameters parameters)
        {
            Parameters = parameters;
        }

        protected override TSearchResult ExecuteCore(IElasticClient client, string index)
        {
            var response = client.Search<TDocument, TDocument>(desc => BuildQuery(desc).Index(index));
            return ToSearchResultCore(response, Parameters);
        }

        protected override async Task<TSearchResult> ExecuteCoreAsync(IElasticClient client, string index)
        {
            var response = await client.SearchAsync<TDocument, TDocument>(desc => BuildQuery(desc).Index(index));
            return ToSearchResultCore(response, Parameters);
        }

        protected virtual TSearchResult ToSearchResultCore(ISearchResponse<TDocument> response, TParameters parameters)
        {
            return ToSearchResult(response, parameters);
        }

        protected virtual TSearchResult ToSearchResult(ISearchResponse<TDocument> response, TParameters parameters)
        {
            return (TSearchResult)Activator.CreateInstance(typeof(TSearchResult), BindingFlags.CreateInstance, new object[] { parameters , response });
        }

        protected abstract SearchDescriptor<TDocument> BuildQuery(SearchDescriptor<TDocument> descriptor);
    }
}