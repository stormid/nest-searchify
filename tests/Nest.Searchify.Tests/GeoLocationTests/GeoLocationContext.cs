using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Elasticsearch.Net;
using FluentAssertions;
using Nest.Searchify.Utils;
using Newtonsoft.Json;
using Xunit;

namespace Nest.Searchify.Tests.GeoLocationTests
{
    public class GeoLocationContext
    {
        [Theory]
        [InlineData("0,0", "0,0", 0.00)]
        [InlineData("56.462018,-2.970721000000026", "55.9740046,-3.1883059", 34.75)]
        [InlineData("55.9740046,-3.1883059", "56.462018,-2.970721000000026", 34.75)]
        public void WhenCalculatingDistanceBetweenTwoPoints(string from, string to, double distance)
        {
            GeoLocation fromPoint = from;
            GeoLocation toPoint = to;

            var result = GeoMath.Distance(fromPoint.Latitude, fromPoint.Longitude, toPoint.Latitude, toPoint.Longitude,
                GeoMath.MeasureUnits.Miles);

            result.Should().Be(distance);
        }

        public class GeoLocationCreation
        {
            [Fact]
            public void WhenAttemptingToCreateAGeoLocationFromEmptyString()
            {
                Assert.Throws<ArgumentNullException>(() =>
                {
                    GeoLocation point = "";
                });
            }

            [Theory]
            [InlineData("hello")]
            [InlineData("1")]
            [InlineData("1,2,3")]
            public void WhenAttemptingToCreateAGeoLocationFromInvalidString(string input)
            {
                Action a = () =>
                {
                    GeoLocation point = input;
                };

                a.ShouldThrow<ArgumentException>();
            }
            
            [Theory]
            [InlineData(-91)]
            [InlineData(91)]
            [InlineData(90.1)]
            [InlineData(-90.1)]
            public void WhenAttemptingToCreateAGeoLocationWithInvalidLatitude(double latitude)
            {
                Action a = () =>
                {
                    var point = new GeoLocation(latitude, 0);
                };

                a.ShouldThrow<ArgumentOutOfRangeException>().And.Message.Should().Contain("latitude");
            }

            [Theory]
            [InlineData(-181)]
            [InlineData(181)]
            [InlineData(180.1)]
            [InlineData(-180.1)]
            public void WhenAttemptingToCreateAGeoLocationWithInvalidLongitude(double longitude)
            {
                Action a = () =>
                {
                    var point = new GeoLocation(0, longitude);
                };

                a.ShouldThrow<ArgumentOutOfRangeException>().And.Message.Should().Contain("longitude");
            }
        }

        public class GeoLocationSerialisationWithJsonNet
        {
            [Fact]
            public void WhenSerialisingAGeoLocationToString()
            {
                GeoLocation point = "1.1,1.22";

                var result = JsonConvert.SerializeObject(point, Formatting.None);
                var expected = "{\"lat\":1.1,\"lon\":1.22}";
                result.Should().Be(expected);
            }

            [Fact]
            public void WhenDeserialisingAGeoLocationFromString()
            {
                var input = "{\"lat\":1.1,\"lon\":1.22}";
                var result = JsonConvert.DeserializeObject<GeoLocation>(input);

                GeoLocation expected = "1.1,1.22";

                result.Should().Be(expected);
            }
        }

        public class GeoLocationSerialisationWithNestSerializer
        {
            [Fact]
            public void WhenSerialisingAGeoLocationToString()
            {
                GeoLocation point = "1.1,1.22";

                var stream = new MemoryStream();
                var connectionSettings = new ConnectionSettings();
                var client = new ElasticClient(connectionSettings);
                client.Serializer.Serialize(point, stream, SerializationFormatting.None);
                stream.Seek(0, SeekOrigin.Begin);
                using (var r = new StreamReader(stream))
                {
                    var result = r.ReadToEnd();
                    var expected = "{\"lat\":1.1,\"lon\":1.22}";
                    result.Should().Be(expected);
                }
            }

            [Fact]
            public void WhenDeserialisingAGeoLocationFromString()
            {
                var input = "{\"lat\":1.1,\"lon\":1.22}";
                var connectionSettings = new ConnectionSettings();
                var client = new ElasticClient(connectionSettings);
                var strm = new MemoryStream(Encoding.UTF8.GetBytes(input));
                var result = client.Serializer.Deserialize<GeoLocation>(strm);

                GeoLocation expected = "1.1,1.22";

                result.Should().Be(expected);
            }
        }
    }
}
