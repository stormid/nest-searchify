using System.Linq;
using Nest;
using Nest.Searchify;
using Nest.Searchify.Extensions;
using Nest.Searchify.Queries;
using static Nest.Searchify.Queries.SearchQueryHelpers;

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
                GeoDistanceFilter<PersonDocument>(p => p.Location, parameters.Location, parameters.Radius.HasValue ? Distance.Miles(parameters.Radius.Value) : null)
                &&
                TermFilterFor<PersonDocument, string>(f => f.Country.Value, parameters.Country)
                &&
                MultiTermOrFilterFor<PersonDocument, string>(f => f.Tags.First().Value, parameters.Tags)
                ;
        }
        
        protected override AggregationContainerDescriptor<PersonDocument> ApplyAggregations(AggregationContainerDescriptor<PersonDocument> descriptor, PersonSearchParameters parameters)
        {
            return
                descriptor
                    .Terms(nameof(PersonSearchParameters.Tags), t => t
                        .AsSearchifyFilter(c => c.WithDisplayName("Search Tags"))
                        .Field(f => f.Tags.First().Key)

                    )
                    .Terms(nameof(PersonSearchParameters.Country), t => t
                        .AsSearchifyFilter()
                        .Field(f => f.Country.Key)
                    )
                    .Range(PersonSearchParameters.AgeRangeParameter, r => r
                        .AsSearchifyFilter(c => c.WithDisplayName("Age Range"))
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
                    .GeoDistance(nameof(PersonSearchParameters.LocationRange), g => g
                    .AsSearchifyFilter()    
                    .Field(f => f.Location)
                        .Origin(parameters.Location)
                        .Unit(DistanceUnit.Miles)
                        .Ranges(
                            r => r.Key("1||up to 10 miles").To(10),
                            r => r.Key("2||up to 20 miles").To(20),
                            r => r.Key("3||up to 30 miles").To(30),
                            r => r.Key("4||up to 40 miles").To(40),
                            r => r.Key("5||up to 50 miles").To(50),
                            r => r.Key("6||up to 100 miles").To(100),
                            r => r.Key("7||more than 100 miles").From(100)
                        ));
        }
    }
}