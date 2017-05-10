using System.Linq;
using Nest;
using Nest.Searchify;
using Nest.Searchify.Extensions;
using Nest.Searchify.Queries;

namespace SearchifyCoreSample
{
    public class SampleSearchQuery : SearchParametersQuery<PersonSearchParameters, PersonDocument, PersonSearchResult>
    {
        public SampleSearchQuery(string queryTerm, int page = 1, int size = 10) : this(new PersonSearchParameters(size, page) {Query = queryTerm})
        {
            
        }

        public SampleSearchQuery(PersonSearchParameters parameters) : base(parameters)
        {
        }

        protected override QueryContainer WithQuery(IQueryContainer query, string queryTerm)
        {
            return Query<PersonDocument>.Match(f => f.Field(fld => fld.Name).Query(queryTerm));
        }

        protected override QueryContainer WithFilters(IQueryContainer query, PersonSearchParameters parameters)
        {
            return 
                Query<PersonDocument>.Term(t => t.Field(f => f.Country.Value).Value(parameters.Country))
                &&
                Query<PersonDocument>.Terms(t => t.Field(f => f.Tags.First().Value).Terms(parameters.Tags))
                ;
        }
        
        protected override AggregationContainerDescriptor<PersonDocument> ApplyAggregations(AggregationContainerDescriptor<PersonDocument> descriptor, PersonSearchParameters parameters)
        {
            return
                descriptor
                    .Terms(nameof(PersonSearchParameters.Tags), t => t
                        //.AsSearchifyFilter(c => c.WithDisplayName("Search Tags"))
                        .Field(f => f.Tags.First().Key)

                    )
                    .Terms(nameof(PersonSearchParameters.Country), t => t
                        //.AsSearchifyFilter()
                        .Field(f => f.Country.Key)
                    )
                    .Range(PersonSearchParameters.AgeRangeParameter, r => r
                        //.AsSearchifyFilter(c => c.WithDisplayName("Age Range"))
                        .Field(f => f.Age)
                        .Ranges(
                            rng => rng.Key(FilterField.Create(AgeRangeEnum.Young))
                                .From(0)
                                .To(20),
                            rng => rng.Key(FilterField.Create(AgeRangeEnum.MiddleAge))
                                .From(21)
                                .To(40),
                            rng => rng.Key(FilterField.Create(AgeRangeEnum.Older)).From(41)
                        )
                    )
                ;
        }
    }
}