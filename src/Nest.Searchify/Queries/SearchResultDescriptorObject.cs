using System;
using System.Reflection;
using System.Threading.Tasks;
using Nest.Queryify.Abstractions.Queries;
using Nest.Searchify.Abstractions;
using Nest.Searchify.SearchResults;

namespace Nest.Searchify.Queries
{
    public abstract class SearchResultDescriptorObject<TDocument, TReturnDocument, TSearchParameters, TSearchResult> : ElasticClientQueryObject<TSearchResult>
        where TDocument : class
        where TReturnDocument : class
        where TSearchParameters : ICommonParameters
        where TSearchResult : SearchResult<TDocument, TReturnDocument, TSearchParameters>
    {
        private readonly TSearchParameters _parameters;

        public TSearchParameters Parameters { get { return _parameters; } }

        protected SearchResultDescriptorObject(TSearchParameters parameters)
        {
            _parameters = parameters;
        }

        protected override TSearchResult ExecuteCore(IElasticClient client, string index)
        {
            var response = client.Search<TDocument, TReturnDocument>(desc => BuildQuery(desc).Index(index));
            return ToSearchResultCore(response, _parameters);
        }

        protected override Task<TSearchResult> ExecuteCoreAsync(IElasticClient client, string index)
        {
            return client
                .SearchAsync<TDocument, TReturnDocument>(desc => BuildQuery(desc).Index(index))
                .ContinueWith(r => ToSearchResultCore(r.Result, _parameters));
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

    public abstract class SearchResultDescriptorObject<TDocument, TSearchParameters, TSearchResult> : ElasticClientQueryObject<TSearchResult> 
        where TDocument : class
        where TSearchParameters : ICommonParameters
        where TSearchResult : SearchResult<TDocument, TSearchParameters>
    {
        private readonly TSearchParameters _parameters;

        protected SearchResultDescriptorObject(TSearchParameters parameters)
        {
            _parameters = parameters;
        }

        protected override TSearchResult ExecuteCore(IElasticClient client, string index)
        {
            var response = client.Search<TDocument, TDocument>(desc => BuildQuery(desc).Index(index));
            return ToSearchResultCore(response, _parameters);
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