using FluentAssertions;
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
            [JsonConverter(typeof(GeoLocationJsonConverter))]
            [JsonProperty(PropertyName = "locpt")]
            public string Point { get; set; }
            [JsonConverter(typeof(GeoBoundingBoxJsonConverter))]
            public string BBox { get; set; }
        }

        public class TestGeoPointOutParameters : SearchParameters
        {
            [JsonProperty(PropertyName = "locpt")]
            public GeoLocation Point { get; set; }
            public GeoBoundingBox BBox { get; set; }
        }

        [Fact]
        public void ShouldDeserialiseGeoPointToGeoPoint()
        {
            var p = new TestGeoPointOutParameters()
            {
                Point = "2.1,3.4"
            };
            var json = JsonConvert.SerializeObject(p);
            var p2 = JsonConvert.DeserializeObject<TestGeoPointOutParameters>(json);
            p2.Point.Should().Be(p.Point);
        }

        [Fact]
        public void ShouldNotDeserialiseGeoPointToGeoPointWithInvalidPoints()
        {

            var json = "{ \"locpt\": \"3,4\" }";
            var p2 = JsonConvert.DeserializeObject<TestGeoPointOutParameters>(json);
            p2.Point.Latitude.Should().Be(3);
            p2.Point.Longitude.Should().Be(4);
        }

        [Fact]
        public void ShouldDeserialiseGeoBoundingBoxToGeoBoundingBox()
        {
            var p = new TestGeoPointOutParameters()
            {
                BBox = "[2.123,-3.213234][2.123,-3.213234]"
            };
            var json = JsonConvert.SerializeObject(p);
            var p2 = JsonConvert.DeserializeObject<TestGeoPointOutParameters>(json);
            p2.BBox.Should().NotBeNull();
            p2.BBox.TopLeft.Should().Be(p.BBox.TopLeft);
            p2.BBox.BottomRight.Should().Be(p.BBox.BottomRight);
        }

        [Fact]
        public void ShouldDeserialiseStringRepresentationToObject()
        {
            var p = new TestGeoPointParameters()
            {
                BBox = "[2.123,-3.213234][2.123,-3.213234]",
                Point = "2.1,3.4"
            };
            var json = JsonConvert.SerializeObject(p);
            var p2 = JsonConvert.DeserializeObject<TestGeoPointOutParameters>(json);
            p2.BBox.ToString().Should().BeEquivalentTo(p.BBox);
            p2.Point.ToString().Should().BeEquivalentTo(p.Point);
        }

        [Fact]
        public void ShouldDeserialiseObjectToStringRepresentation()
        {
            var p = new TestGeoPointOutParameters()
            {
                BBox = "[2.123,-3.213234][2.123,-3.213234]",
                Point = "2.1,3.4"
            };
            var json = JsonConvert.SerializeObject(p);
            var p2 = JsonConvert.DeserializeObject<TestGeoPointParameters>(json);
            p2.BBox.Should().BeEquivalentTo(p.BBox);
            p2.Point.Should().BeEquivalentTo(p.Point.ToString());
        }
    }
}