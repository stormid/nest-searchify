using System.Collections.Generic;
using System.Linq;
using Nest.Searchify.Abstractions;
using Nest.Searchify.SearchAggregations;
using Newtonsoft.Json;

namespace Nest.Searchify.SearchResults
{
    public class SearchResult<TDocument> : SearchResult<ICommonParameters, TDocument, TDocument>
        where TDocument : class
    {
        public SearchResult(ICommonParameters parameters, ISearchResponse<TDocument> response) : base(parameters, response)
        {
        }
    }

    public class SearchResult<TParameters, TDocument> : SearchResult<TParameters, TDocument, TDocument>
		where TDocument : class
		where TParameters : class, ICommonParameters
	{
		public SearchResult(TParameters parameters, ISearchResponse<TDocument> response) : base(parameters, response)
		{
		}
	}

    public class SearchResult<TParameters, TDocument, TReturnAs> : SearchResultBase<TParameters>, ISearchResult<TParameters, TReturnAs> 
        where TDocument : class
		where TReturnAs : class
		where TParameters : class, ICommonParameters
	{

		[JsonProperty("documents", NullValueHandling = NullValueHandling.Ignore)]
		public virtual IEnumerable<TReturnAs> Documents { get; protected set; }

		protected ISearchResponse<TDocument> Response { get; }

		#region Aggregations

        [JsonIgnore]
        public AggregationsHelper AggregationHelper => Response.Aggs;

        [JsonProperty("aggregations")]
        public IDictionary<string, IAggregation> Aggregations { get; private set; } 

        #endregion

		public SearchResult(TParameters parameters, ISearchResponse<TDocument> response) : base(parameters)
		{
			Response = response;
		    Documents = TransformResultCore(response);
            Aggregations = SearchAggregationParser.Parse(Response.Aggregations);
        }

	    private IEnumerable<TReturnAs> TransformResultCore(ISearchResponse<TDocument> response)
	    {
	        return TransformResult(response.Documents);
        }

		protected virtual IEnumerable<TReturnAs> TransformResult(IEnumerable<TDocument> entities)
		{
			return Response.Documents.Select(TransformEntity).Where(x => x != null);
		}

		protected virtual TReturnAs TransformEntity(TDocument entity)
		{
			return entity as TReturnAs;
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