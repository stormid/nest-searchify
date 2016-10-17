using System;
using FluentAssertions;
using Nest.Searchify.Queries.Models;
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
}