using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Elasticsearch.Net.Serialization;
using FluentAssertions;
using Nest.Searchify.Queries.Models;
using Nest.Searchify.Utils;
using Newtonsoft.Json;
using Xunit;

namespace Nest.Searchify.Tests.GeoPointTests
{
    public class GeoBoundingBoxTests
    {
        public class GeoBoundingBoxCreation
        {
            [Fact]
            public void WhenAttemptingToCreateAGeoBoundingBoxFromEmptyString()
            {
                Assert.Throws<ArgumentNullException>(() =>
                {
                    GeoBoundingBox bbox = "";
                });
            }

            [Fact]
            public void WhenAttemptingToCreateAGeoBoundingBoxFromInvalidString()
            {
                Assert.Throws<FormatException>(() =>
                {
                    GeoBoundingBox bbox = "hello";
                });
            }

            [Theory]
            [InlineData("-91,0")]
            [InlineData("91,0")]
            [InlineData("90.1,0")]
            [InlineData("-90.1,0")]
            public void WhenAttemptingToCreateAGeoBoundingBoxWithInvalidLatitude(string geopoint)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    new GeoBoundingBox(geopoint, "0,0");
                })
                .Message.Should().Contain("latitude");
            }

            [Theory]
            [InlineData("[][]")]
            [InlineData("abc")]
            [InlineData("[a,b][c,d]")]
            [InlineData("[1,2,3][1,2,3]")]
            [InlineData("[1,2],[1,2]")]
            [InlineData(" [1,2],[1,2] ")]
            public void WhenAttemptingToCreateAGeoBoundingBoxWithInValidArgsFromString(string bboxString)
            {
                Assert.Throws<FormatException>(() =>
                {
                    GeoBoundingBox bbox = bboxString;
                });
            }

            [Theory]
            [InlineData("-23,1", "53,4")]
            public void WhenAttemptingToCreateAGeoBoundingBoxWithValidArgs(string topLeft, string bottomRight)
            {
                var bbox = new GeoBoundingBox(topLeft, bottomRight);
                bbox.Should().NotBeNull();
            }

            [Theory]
            [InlineData("[23,1][53,4]", 23, 1, 53, 4)]
            [InlineData("[2.123, -3.213234] [2.123, -3.213234]", 2.123, -3.213234, 2.123, -3.213234)]
            public void WhenAttemptingToCreateAGeoBoundingBoxWithValidArgsFromString(string bboxString, double topLeftLat, double topLeftLon, double bottomRightLat, double bottomRightLon)
            {
                GeoBoundingBox bbox = bboxString;
                bbox.Should().NotBeNull();
                bbox.TopLeft.Latitude.Should().Be(topLeftLat);
                bbox.TopLeft.Longitude.Should().Be(topLeftLon);
                bbox.BottomRight.Latitude.Should().Be(bottomRightLat);
                bbox.BottomRight.Longitude.Should().Be(bottomRightLon);
            }
        }
    }

    public class GeoPointContext
    {
        [Theory]
        [InlineData("0,0", "0,0", 0.00)]
        [InlineData("56.462018,-2.970721000000026", "55.9740046,-3.1883059", 34.75)]
        [InlineData("55.9740046,-3.1883059", "56.462018,-2.970721000000026", 34.75)]
        public void WhenCalculatingDistanceBetweenTwoPoints(string from, string to, double distance)
        {
            GeoPoint fromPoint = from;
            GeoPoint toPoint = to;

            var result = GeoMath.Distance(fromPoint.Latitude, fromPoint.Longitude, toPoint.Latitude, toPoint.Longitude,
                GeoMath.MeasureUnits.Miles);

            result.Should().Be(distance);
        }

        public class GeoPointCreation
        {
            [Fact]
            public void WhenAttemptingToCreateAGeoPointFromEmptyString()
            {
                Assert.Throws<ArgumentNullException>(() =>
                {
                    GeoPoint point = "";
                });
            }

            [Fact]
            public void WhenAttemptingToCreateAGeoPointFromInvalidString()
            {
                Assert.Throws<FormatException>(() =>
                {
                    GeoPoint point = "hello";
                });
            }

            [Fact]
            public void WhenAttemptingToCreateAGeoPointFromStringWithTooFewParts()
            {
                Assert.Throws<InvalidCastException>(() =>
                {
                    GeoPoint point = "1";
                });
            }

            [Fact]
            public void WhenAttemptingToCreateAGeoPointFromStringWithTooManyParts()
            {
                Assert.Throws<InvalidCastException>(() =>
                {
                    GeoPoint point = "1,2,3";
                });
            }

            [Theory]
            [InlineData(-91)]
            [InlineData(91)]
            [InlineData(90.1)]
            [InlineData(-90.1)]
            public void WhenAttemptingToCreateAGeoPointWithInvalidLatitude(double latitude)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var point = new GeoPoint(latitude, 0);
                }).Message.Should().Contain("latitude");
            }

            [Theory]
            [InlineData(-181)]
            [InlineData(181)]
            [InlineData(180.1)]
            [InlineData(-180.1)]
            public void WhenAttemptingToCreateAGeoPointWithInvalidLongitude(double longitude)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var point = new GeoPoint(0, longitude);
                }).Message.Should().Contain("longitude");
            }
        }

        public class GeoPointSerialisationWithJsonNet
        {
            [Fact]
            public void WhenSerialisingAGeoPointToString()
            {
                GeoPoint point = "1.1,1.22";

                var result = JsonConvert.SerializeObject(point, Formatting.None);
                var expected = "{\"lat\":1.1,\"lon\":1.22}";
                result.Should().Be(expected);
            }

            [Fact]
            public void WhenDeserialisingAGeoPointFromString()
            {
                var input = "{\"lat\":1.1,\"lon\":1.22}";
                var result = JsonConvert.DeserializeObject<GeoPoint>(input);

                GeoPoint expected = "1.1,1.22";

                result.Should().Be(expected);
            }
        }

        public class GeoPointSerialisationWithNestSerializer
        {
            [Fact]
            public void WhenSerialisingAGeoPointToString()
            {
                GeoPoint point = "1.1,1.22";

                var connectionSettings = new ConnectionSettings();
                var client = new ElasticClient(connectionSettings);
                var byteResult = client.Serializer.Serialize(point, SerializationFormatting.None);
                var result = Encoding.UTF8.GetString(byteResult);
                var expected = "{\"lat\":1.1,\"lon\":1.22}";
                result.Should().Be(expected);
            }

            [Fact]
            public void WhenDeserialisingAGeoPointFromString()
            {
                var input = "{\"lat\":1.1,\"lon\":1.22}";
                var connectionSettings = new ConnectionSettings();
                var client = new ElasticClient(connectionSettings);
                var strm = new MemoryStream(Encoding.UTF8.GetBytes(input));
                var result = client.Serializer.Deserialize<GeoPoint>(strm);

                GeoPoint expected = "1.1,1.22";

                result.Should().Be(expected);
            }
        }
    }
}
