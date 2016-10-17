using Nest.Searchify.Converters;
using Nest.Searchify.Queries;
using Nest.Searchify.Queries.Models;
using Newtonsoft.Json;
using Xunit;

namespace Nest.Searchify.Tests.ParametersTests
{
    public class GeoParameterSerialisation
    {
        public class TestGeoPointParameters : SearchParameters
        {
            [JsonConverter(typeof(GeoPointToStringJsonConverter))]
            public string Point { get; set; }
            [JsonConverter(typeof(GeoBoundingBoxToStringJsonConverter))]
            public string BBox { get; set; }
        }

        public class TestGeoPointOutParameters : SearchParameters
        {
            public GeoPoint Point { get; set; }
            public GeoBoundingBox BBox { get; set; }
        }

        [Fact]
        public void Test()
        {
            var p = new TestGeoPointParameters()
            {
                BBox = "[2.123, -3.213234] [2.123, -3.213234]",
                Point = "2.1,3.4"
            };
            var json = JsonConvert.SerializeObject(p);
            var p2 = JsonConvert.DeserializeObject<TestGeoPointOutParameters>(json);
        }

        [Fact]
        public void Test2()
        {
            var p = new TestGeoPointOutParameters()
            {
                BBox = "[2.123, -3.213234] [2.123, -3.213234]",
                Point = "2.1,3.4"
            };
            var json = JsonConvert.SerializeObject(p);
            var p2 = JsonConvert.DeserializeObject<TestGeoPointParameters>(json);
        }
    }
}