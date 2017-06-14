using System.Collections.Generic;
using Nest;
using Nest.Searchify.SearchResults;

namespace SearchifyCoreSample
{
    public class PersonSearchResult : SearchResult<PersonSearchParameters, PersonDocument>
    {
        public PersonSearchResult(PersonSearchParameters parameters, ISearchResponse<PersonDocument> response) : base(parameters, response)
        {
        }

        protected override IReadOnlyDictionary<string, IAggregate> AlterAggregations(IReadOnlyDictionary<string, IAggregate> aggregations)
        {
            return base.AlterAggregations(aggregations);
        }
    }
}