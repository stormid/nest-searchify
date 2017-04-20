using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Nest.Searchify.Abstractions;
using Newtonsoft.Json;

namespace Nest.Searchify.SearchResults
{
    public struct SearchifyKey
    {
        public string Key { get; }
        public string Value { get; }
        public string Text { get; }

        public SearchifyKey(string key, string delimiter = "||")
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var values = key.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
            Key = key;
            Value = values.ElementAtOrDefault(0);
            Text = values.ElementAtOrDefault(1);
        }
    }

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
        public IReadOnlyDictionary<string, IAggregate> Aggregations { get; private set; }

        #endregion

        protected SearchResult(TParameters parameters, ISearchResponse<TDocument> response) : base(parameters)
        {
            Response = response;
            SetDocuments();
            SetAggregations();
        }

        private void SetAggregations()
        {
            Aggregations = AlterAggregations(Response.Aggregations);
        }

        private void SetDocuments()
        {
            Documents = TransformResultCore(Response);
        }

        protected IReadOnlyDictionary<string, IAggregate> AlterAggregationsCore(IReadOnlyDictionary<string, IAggregate> aggregations)
        {
            return AlterAggregations(aggregations);
        }

        protected virtual IReadOnlyDictionary<string, IAggregate> AlterAggregations(IReadOnlyDictionary<string, IAggregate> aggregations)
        {
            return aggregations;
        }

        protected virtual IEnumerable<TDocument> ResponseToDocuments(ISearchResponse<TDocument> response)
        {
            return response.Documents;
        }

        private IEnumerable<TOutputEntity> TransformResultCore(ISearchResponse<TDocument> response)
        {
            return TransformResult(ResponseToDocuments(response)).Where(x => x != null);
        }

        protected abstract IEnumerable<TOutputEntity> TransformResult(IEnumerable<TDocument> entities);

        protected override long GetResponseTimeTaken()
        {
            return Response.Took;
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

        protected override IEnumerable<TDocument> TransformResult(IEnumerable<TDocument> entities)
        {
            return entities;
        }

        protected override long GetResponseTimeTaken()
		{
			return Response.Took;
		}

		protected override long GetSearchResultTotal()
		{
			return Response.Total;
		}
	}
}