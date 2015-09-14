using System;
using System.Reflection;
using System.Threading.Tasks;
using Nest.Queryify.Abstractions.Queries;
using Nest.Searchify.Abstractions;
using Nest.Searchify.SearchResults;

namespace Nest.Searchify.Queries
{
    public abstract class SearchResultQuery<TDocument, TReturnDocument, TSearchParameters, TSearchResult> : ElasticClientQueryObject<TSearchResult>, ISearchResultQuery<TSearchParameters> 
        where TDocument : class
        where TReturnDocument : class
        where TSearchParameters : class, ICommonParameters
        where TSearchResult : class, ISearchResult<TSearchParameters, TDocument>
    {
        public TSearchParameters Parameters { get; }

        protected SearchResultQuery(TSearchParameters parameters)
        {
            Parameters = parameters;
        }

        protected override TSearchResult ExecuteCore(IElasticClient client, string index)
        {
            var response = client.Search<TDocument, TReturnDocument>(desc => BuildQuery(desc).Index(index));
            return ToSearchResultCore(response, Parameters);
        }

        protected override Task<TSearchResult> ExecuteCoreAsync(IElasticClient client, string index)
        {
            return client
                .SearchAsync<TDocument, TReturnDocument>(desc => BuildQuery(desc).Index(index))
                .ContinueWith(r => ToSearchResultCore(r.Result, Parameters));
        }

        protected virtual TSearchResult ToSearchResultCore(ISearchResponse<TReturnDocument> response, TSearchParameters parameters)
        {
            return ToSearchResult(response, parameters);
        }

        protected virtual TSearchResult ToSearchResult(ISearchResponse<TReturnDocument> response, TSearchParameters parameters)
        {
            return (TSearchResult)Activator.CreateInstance(typeof(TSearchResult), parameters, response);
        }

        protected abstract SearchDescriptor<TDocument> BuildQuery(SearchDescriptor<TDocument> descriptor);
    }

    public abstract class SearchResultQuery<TDocument, TSearchParameters> :
        SearchResultQuery<TDocument, TSearchParameters, ISearchResult<TSearchParameters, TDocument>>
        where TDocument : class
        where TSearchParameters : class, ICommonParameters
    {
        protected SearchResultQuery(TSearchParameters parameters) : base(parameters)
        {
        }
    }

    public abstract class SearchResultQuery<TDocument, TSearchParameters, TSearchResult> : ElasticClientQueryObject<TSearchResult>, ISearchResultQuery<TSearchParameters> 
        where TDocument : class
        where TSearchParameters : class, ICommonParameters
        where TSearchResult : class, ISearchResult<TSearchParameters, TDocument>
    {
        public TSearchParameters Parameters { get; }

        protected SearchResultQuery(TSearchParameters parameters)
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

        protected virtual TSearchResult ToSearchResultCore(ISearchResponse<TDocument> response, TSearchParameters parameters)
        {
            return ToSearchResult(response, parameters);
        }

        protected virtual TSearchResult ToSearchResult(ISearchResponse<TDocument> response, TSearchParameters parameters)
        {
            return (TSearchResult)Activator.CreateInstance(typeof(TSearchResult), BindingFlags.CreateInstance, new object[] { parameters , response });
        }

        protected abstract SearchDescriptor<TDocument> BuildQuery(SearchDescriptor<TDocument> descriptor);
    }
}