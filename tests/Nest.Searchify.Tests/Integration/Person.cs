using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Queries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nest.Searchify.Tests.Integration
{
    public class PersonSearchQueryWithTermsAggregation : CommonParametersQuery<Person>
    {
        public PersonSearchQueryWithTermsAggregation(NameValueCollection parameters) : base(parameters)
        {
        }

        public PersonSearchQueryWithTermsAggregation(ICommonParameters parameters) : base(parameters)
        {
        }

        protected override AggregationDescriptor<Person> ApplyAggregations(AggregationDescriptor<Person> descriptor, ICommonParameters parameters)
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
    }

    public class Person
    {
        public static IEnumerable<Person> LoadFromResource()
        {
            using (
                var stream =
                    Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream("Nest.Searchify.Tests.Integration.Person.json"))
            {
                if (stream != null)
                {
                    using (var rdr = new StreamReader(stream))
                    {
                        var s = new JsonSerializer();
                        return s.Deserialize<IEnumerable<Person>>(new JsonTextReader(rdr));
                    }
                }
                return Enumerable.Empty<Person>();
            }
        }

        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public FilterField City { get; set; }
        public FilterField Country { get; set; }
        [ElasticProperty(Type = FieldType.GeoPoint)]
        public GeoPoint Location { get; set; }

        [ElasticProperty(Analyzer = "keyword")]
        public string Postcode { get; set; }

        [ElasticProperty(Analyzer = "english")]
        public string Profile { get; set; }

        public int Age { get; set; }
    }
}
