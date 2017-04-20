using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Elasticsearch.Net;
using FluentAssertions;
using Nest.Searchify.Converters;
using Nest.Searchify.Queries;
using Nest.Searchify.Utils;
using Newtonsoft.Json;
using Xunit;

namespace Nest.Searchify.Tests.GeoLocationTests
{
    public class GeoLocationParameterContext
    {
        [Theory]
        [InlineData("0,0", "0,0", 0.00)]
        [InlineData("56.462018,-2.970721000000026", "55.9740046,-3.1883059", 34.75)]
        [InlineData("55.9740046,-3.1883059", "56.462018,-2.970721000000026", 34.75)]
        public void WhenCalculatingDistanceBetweenTwoPoints(string from, string to, double distance)
        {
            GeoLocationParameter fromPoint = from;
            GeoLocationParameter toPoint = to;

            var result = GeoMath.Distance(fromPoint.Latitude, fromPoint.Longitude, toPoint.Latitude, toPoint.Longitude,
                GeoMath.MeasureUnits.Miles);

            result.Should().Be(distance);
        }

        public class GeoLocationParameterCreation
        {
            [Fact]
            public void WhenAttemptingToCreateAGeoLocationParameterFromEmptyString()
            {
                Assert.Throws<ArgumentNullException>(() =>
                {
                    GeoLocationParameter point = "";
                });
            }

            [Theory]
            [InlineData("hello")]
            [InlineData("1")]
            [InlineData("1,2,3")]
            public void WhenAttemptingToCreateAGeoLocationParameterFromInvalidString(string input)
            {
                Action a = () =>
                {
                    GeoLocationParameter point = input;
                };

                a.ShouldThrow<ArgumentException>();
            }
            
            [Theory]
            [InlineData(-91)]
            [InlineData(91)]
            [InlineData(90.1)]
            [InlineData(-90.1)]
            public void WhenAttemptingToCreateAGeoLocationParameterWithInvalidLatitude(double latitude)
            {
                Action a = () =>
                {
                    var point = new GeoLocationParameter(latitude, 0);
                };

                a.ShouldThrow<ArgumentOutOfRangeException>().And.Message.Should().Contain("latitude");
            }

            [Theory]
            [InlineData(-181)]
            [InlineData(181)]
            [InlineData(180.1)]
            [InlineData(-180.1)]
            public void WhenAttemptingToCreateAGeoLocationParameterWithInvalidLongitude(double longitude)
            {
                Action a = () =>
                {
                    var point = new GeoLocationParameter(0, longitude);
                };

                a.ShouldThrow<ArgumentOutOfRangeException>().And.Message.Should().Contain("longitude");
            }
        }

        public class GeoLocationParameterSerialisationWithJsonNet
        {
            [Fact]
            public void WhenSerialisingAGeoLocationParameterToString()
            {
                GeoLocationParameter point = "1.1,1.22";

                var result = JsonConvert.SerializeObject(point, Formatting.None);
                var expected = "{\"lat\":1.1,\"lon\":1.22}";
                result.Should().Be(expected);
            }

            [Fact]
            public void WhenDeserialisingAGeoLocationParameterFromString()
            {
                var input = "{\"lat\":1.1,\"lon\":1.22}";
                var result = JsonConvert.DeserializeObject<GeoLocationParameter>(input);

                GeoLocationParameter expected = "1.1,1.22";

                result.Should().Be(expected);
            }
        }

        public class GeoLocationParameterSerialisationWithNestSerializer
        {
            [Fact]
            public void WhenSerialisingAGeoLocationParameterToString()
            {
                GeoLocationParameter point = "1.1,1.22";

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
            public void WhenDeserialisingAGeoLocationParameterFromString()
            {
                var input = "{\"lat\":1.1,\"lon\":1.22}";
                var connectionSettings = new ConnectionSettings();
                var client = new ElasticClient(connectionSettings);
                var strm = new MemoryStream(Encoding.UTF8.GetBytes(input));
                var result = client.Serializer.Deserialize<GeoLocationParameter>(strm);

                GeoLocationParameter expected = "1.1,1.22";

                result.Should().Be(expected);
            }
        }
    }
}
