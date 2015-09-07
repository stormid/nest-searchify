using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nest.Searchify.Tests.Integration
{
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
