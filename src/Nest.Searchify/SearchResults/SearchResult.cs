using System.Collections.Generic;
using System.Linq;
using Nest.Searchify.Abstractions;
using Nest.Searchify.SearchAggregations;
using Newtonsoft.Json;

namespace Nest.Searchify.SearchResults
{
    public class SearchResult<TEntity> : SearchResult<TEntity, TEntity, ICommonParameters>
        where TEntity : class
    {
        public SearchResult(ICommonParameters parameters, ISearchResponse<TEntity> response) : base(parameters, response)
        {
        }
    }

    public class SearchResult<TEntity, TParameters> : SearchResult<TEntity, TEntity, TParameters>
		where TEntity : class
		where TParameters : ICommonParameters
	{
		public SearchResult(TParameters parameters, ISearchResponse<TEntity> response) : base(parameters, response)
		{
		}
	}

    public class SearchResult<TEntity, TOutputEntity, TParameters> : SearchResultBase<TParameters> where TEntity : class
		where TOutputEntity : class
		where TParameters : ICommonParameters
	{

		[JsonProperty("documents", NullValueHandling = NullValueHandling.Ignore)]
		public virtual IEnumerable<TOutputEntity> Documents { get; protected set; }

		protected ISearchResponse<TEntity> Response { get; private set; }

		#region Aggregations

        [JsonIgnore]
        public AggregationsHelper AggregationHelper
        {
            get { return Response.Aggs; }
        }

        [JsonProperty("aggregations")]
        public IDictionary<string, IAggregation> Aggregations { get; private set; } 

        #endregion

		public SearchResult(TParameters parameters, ISearchResponse<TEntity> response) : base(parameters)
		{
			Response = response;
		    Documents = TransformResultCore(response);
            Aggregations = SearchAggregationParser.Parse(Response.Aggregations);
        }

	    private IEnumerable<TOutputEntity> TransformResultCore(ISearchResponse<TEntity> response)
	    {
	        return TransformResult(response.Documents);
        }

		protected virtual IEnumerable<TOutputEntity> TransformResult(IEnumerable<TEntity> entities)
		{
			return Response.Documents.Select(TransformEntity).Where(x => x != null);
		}

		protected virtual TOutputEntity TransformEntity(TEntity entity)
		{
			return entity as TOutputEntity;
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