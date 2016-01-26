using System.Collections.Generic;
using System.Linq;
using Nest.Searchify.Abstractions;
using Nest.Searchify.SearchAggregations;
using Newtonsoft.Json;

namespace Nest.Searchify.SearchResults
{
    public abstract class SearchResult<TParameters, TDocument, TOutputEntity> : SearchResultBase<TParameters>, ISearchResult<TParameters, TOutputEntity>
        where TDocument : class
        where TOutputEntity : class
        where TParameters : class, IPagingParameters, ISortingParameters, new()
    {
        protected SearchResult(TParameters parameters) : base(parameters) { }

        [JsonProperty("documents", NullValueHandling = NullValueHandling.Ignore)]
        public virtual IEnumerable<TOutputEntity> Documents { get; protected set; }

        protected ISearchResponse<TDocument> Response { get; }

        #region Aggregations

        [JsonIgnore]
        public AggregationsHelper AggregationHelper => Response.Aggs;

        [JsonProperty("aggregations")]
        public IDictionary<string, IAggregation> Aggregations { get; }

        #endregion

        protected SearchResult(TParameters parameters, ISearchResponse<TDocument> response) : base(parameters)
        {
            Response = response;
            SetDocuments();
            Aggregations = SearchAggregationParser.Parse(Response.Aggregations);
        }

        private void SetDocuments()
        {
            Documents = TransformResultCore(Response);
        }

        private IEnumerable<TOutputEntity> TransformResultCore(ISearchResponse<TDocument> response)
        {
            return TransformResult(response.Documents);
        }

        protected virtual IEnumerable<TOutputEntity> TransformResult(IEnumerable<TDocument> entities)
        {
            return Response.Documents.Select(TransformEntity).Where(x => x != null);
        }

        protected abstract TOutputEntity TransformEntity(TDocument entity);

        protected override int GetResponseTimeTaken()
        {
            return Response.ElapsedMilliseconds;
        }

        protected override long GetSearchResultTotal()
        {
            return Response.Total;
        }
    }

    public class SearchResult<TParameters, TDocument> : SearchResult<TParameters, TDocument, TDocument>
        where TDocument : class
		where TParameters : class, IPagingParameters, ISortingParameters, new()
    {
		public SearchResult(TParameters parameters, ISearchResponse<TDocument> response) : base(parameters, response)
		{
        }

		protected override TDocument TransformEntity(TDocument entity)
		{
		    return entity;
		}

		protected override int GetResponseTimeTaken()
		{
			return Response.ElapsedMilliseconds;
		}

		protected override long GetSearchResultTotal()
		{
			return Response.Total;
		}
	}
}