using System;
using System.Collections.Generic;
using System.Linq;
using Nest.Searchify.Abstractions;
using Newtonsoft.Json;

namespace Nest.Searchify
{
    internal static class SearchAggregationParser
    {
        public static IDictionary<string, IAggregation> Parse(IDictionary<string, IAggregation> aggregations)
        {
            if (aggregations == null || !aggregations.Any()) return null;
            return ParseCore(aggregations).ToDictionary(k => k.Key, v => v.Value);
        }

        private static IEnumerable<KeyValuePair<string, IAggregation>> ParseCore(IDictionary<string, IAggregation> aggregations)
        {
            // TODO : yes, this is a mess
            foreach (var agg in aggregations)
            {
                if (agg.Value is Bucket) // if its a bucket
                {
                    var bucket = agg.Value as Bucket;
                    if (bucket.Items.All(x => x.GetType() == typeof(KeyItem)))
                    {
                        yield return TermBucket.Create(agg);
                    }
                    else if (bucket.Items.All(x => x.GetType() == typeof (RangeItem)))
                    {
                        yield return new KeyValuePair<string, IAggregation>(agg.Key, new RangeBucket(bucket.Items));
                    }
                    else if (bucket.Items.All(x => x.GetType() == typeof (SignificantTermItem)))
                    {
                        yield return new KeyValuePair<string, IAggregation>(agg.Key, new SignificantTermBucket(bucket.Items));
                    }
                }
                else
                {
                    yield return agg;
                }
            }
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

    public class TermKeyItem : KeyItem
    {
        public string Value { get; }
        public string Text { get; }

        public TermKeyItem(KeyItem item)
        {
            DocCount = item.DocCount;
            Key = Text = Value = item.Key;
            KeyAsString = item.KeyAsString;
            Aggregations = SearchAggregationParser.Parse(item.Aggregations);

            var splitKey = Key.Split(new[] { FilterField.DefaultDelimiter }, StringSplitOptions.RemoveEmptyEntries);
            if (splitKey.Length == 2)
            {
                Value = splitKey[0];
                Text = splitKey[1];
            }
        }
    }
    
    public class SignificantTermKeyItem : SignificantTermItem
    {
        public string Value { get; }
        public string Text { get; }

        public SignificantTermKeyItem(SignificantTermItem item)
        {
            DocCount = item.DocCount;
            BgCount = item.BgCount;
            Score = item.Score;

            Key = Text = Value = item.Key;
            Aggregations = SearchAggregationParser.Parse(item.Aggregations);

            var splitKey = Key.Split(new[] { FilterField.DefaultDelimiter }, StringSplitOptions.RemoveEmptyEntries);
            if (splitKey.Length == 2)
            {
                Value = splitKey[0];
                Text = splitKey[1];
            }
        }
    }

    public class RangeKeyItem : RangeItem
    {
        public string Value { get; }
        public string Text { get; }

        public RangeKeyItem(RangeItem item)
        {
            DocCount = item.DocCount;
            From = item.From;
            To = item.To;
            FromAsString = item.FromAsString;
            ToAsString = item.ToAsString;
            Key = Text = Value = item.Key;
            Aggregations = SearchAggregationParser.Parse(item.Aggregations);

            var splitKey = Key.Split(new[] { FilterField.DefaultDelimiter }, StringSplitOptions.RemoveEmptyEntries);
            if (splitKey.Length == 2)
            {
                Value = splitKey[0];
                Text = splitKey[1];
            }
        }
    }

    public class SignificantTermBucket : BucketWithDocCount<SignificantTermKeyItem>
    {
        public string Type => "SignificantTerm";
        public SignificantTermBucket()
        {
        }

        public SignificantTermBucket(IEnumerable<IAggregation> items)
        {
            Items = items.OfType<SignificantTermItem>().Select(x => new SignificantTermKeyItem(x)).ToList();
        }

        public SignificantTermBucket(IDictionary<string, IAggregation> aggregations) : base(aggregations)
        {
        }
    }

    public class TermBucket : Bucket<TermKeyItem>
    {
        public string Type => "Term";
        public TermBucket()
        {
        }

        public static KeyValuePair<string, IAggregation> Create(KeyValuePair<string, IAggregation> aggregation)
        {
            return new KeyValuePair<string, IAggregation>(aggregation.Key, new TermBucket(aggregation.Value));
        }

        private static IEnumerable<IAggregation> GetItems(IAggregation aggregation)
        {
            if (aggregation is Bucket)
            {
                var b = aggregation as Bucket;
                return b.Items;
            }
            return Enumerable.Empty<IAggregation>();
        }

        public TermBucket(IAggregation aggregation) : this(GetItems(aggregation))
        {
            
        }

        public TermBucket(IEnumerable<IAggregation> items)
        {
            Items = items.OfType<KeyItem>().Select(x => new TermKeyItem(x)).ToList();
        }

        public TermBucket(IDictionary<string, IAggregation> aggregations) : base(aggregations)
        {
        }
    }

    public class RangeBucket : Bucket<RangeKeyItem>
    {
        public string Type { get { return "Range"; } }

        public RangeBucket()
        {
        }

        public RangeBucket(IEnumerable<IAggregation> items)
        {
            Items = items.OfType<RangeItem>().Select(x => new RangeKeyItem(x)).ToList();
        }

        public RangeBucket(IDictionary<string, IAggregation> aggregations) : base(aggregations)
        {
        }
    }
    
    public class SearchResult<TEntity, TOutputEntity, TParameters> : SearchResultBase<TParameters> where TEntity : class
		where TOutputEntity : class
		where TParameters : ICommonParameters
	{

		[JsonProperty("documents", NullValueHandling = NullValueHandling.Ignore)]
		public virtual IEnumerable<TOutputEntity> Documents { get; protected set; }

		protected ISearchResponse<TEntity> Response { get; }

		#region Aggregations

		[JsonIgnore]
		public AggregationsHelper AggregationHelper => Response.Aggs;

        [JsonProperty("aggregations")]
        public IDictionary<string, IAggregation> Aggregations { get; } 

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