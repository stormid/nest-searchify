using System.Collections.Generic;
using System.Linq;
using Nest.Searchify.Abstractions;
using Nest.Searchify.SearchAggregations;
using Newtonsoft.Json;

namespace Nest.Searchify.SearchResults
{
    public class SearchResult<TParameters, TDocument> : SearchResultBase<TParameters>, ISearchResult<TParameters, TDocument> 
        where TDocument : class
		where TParameters : class, IParameters
	{

		[JsonProperty("documents", NullValueHandling = NullValueHandling.Ignore)]
		public virtual IEnumerable<TDocument> Documents { get; protected set; }

		protected ISearchResponse<TDocument> Response { get; }

		#region Aggregations

        [JsonIgnore]
        public AggregationsHelper AggregationHelper => Response.Aggs;

        [JsonProperty("aggregations")]
        public IDictionary<string, IAggregation> Aggregations { get; } 

        #endregion

		public SearchResult(TParameters parameters, ISearchResponse<TDocument> response) : base(parameters)
		{
			Response = response;
		    SetDocuments();
            Aggregations = SearchAggregationParser.Parse(Response.Aggregations);
        }

        private void SetDocuments()
        {
            Documents = TransformResultCore(Response);
        }

	    private IEnumerable<TDocument> TransformResultCore(ISearchResponse<TDocument> response)
	    {
	        return TransformResult(response.Documents);
        }

		protected virtual IEnumerable<TDocument> TransformResult(IEnumerable<TDocument> entities)
		{
			return Response.Documents.Select(TransformEntity).Where(x => x != null);
		}

		protected virtual TDocument TransformEntity(TDocument entity)
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