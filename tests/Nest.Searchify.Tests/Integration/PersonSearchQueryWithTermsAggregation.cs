using System.Collections.Specialized;
using System.Threading.Tasks;
using Nest.Queryify.Abstractions.Queries;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Queries;

namespace Nest.Searchify.Tests.Integration
{
    public class TestSearchQuery :
        CommonParametersQuery<SearchDataCollection.CustomSearchQuery.PersonParameters, Person>
    {
        public TestSearchQuery(SearchDataCollection.CustomSearchQuery.PersonParameters parameters) : base(parameters)
        {
        }
    }

    public class PersonSearchQueryWithTermsAggregation : CommonParametersQuery<SearchDataCollection.CustomSearchQuery.PersonParameters, Person, SearchDataCollection.CustomSearchQuery.PersonSearchResults>
    {
        protected override AggregationDescriptor<Person> ApplyAggregations(AggregationDescriptor<Person> descriptor, SearchDataCollection.CustomSearchQuery.PersonParameters parameters)
        {
            return descriptor
                .Terms("country", t => t
                    .Field(f => f.Country.Key)
                    .Aggregations(a => a.SignificantTerms("sigTerms", s => s.Field(f => f.City.Key)))
                )
                .Range("age", r => r.Field(f => f.Age)
                    .Ranges(
                        range => range.Key("18-30").From(18).To(30),
                        range => range.Key("31-45").From(31).To(45),
                        range => range.Key("46-65").From(46).To(65)
                    )
                )
                .Average("average_age", a => a.Field(f => f.Age))
                ;
        }

        public PersonSearchQueryWithTermsAggregation(SearchDataCollection.CustomSearchQuery.PersonParameters parameters) : base(parameters) { }
    }
}