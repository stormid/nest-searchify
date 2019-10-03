using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Nest.Searchify.Tests.ParametersTests
{
    public class GeoLocationJsonPropertyIgnoredTestCase
    {
        [Fact]
        public void ShouldSerialiseGeoLocation()
        {
            var p = new GeoLocation(1.23, 5.67);

            var json = JsonConvert.SerializeObject(p);
            var p2 = JsonConvert.DeserializeObject<GeoLocation>(json);

            p2.Latitude.ToString().Should().BeEquivalentTo(p.Latitude.ToString());
            p2.Longitude.ToString().Should().BeEquivalentTo(p.Longitude.ToString());
        }

        [Fact]
        public void GeoLocationDoesNotHonourJsonPropertyAttributes()
        {
            //From GeoLocation.cs
            //[JsonProperty("lat")]
            //public double Latitude
            //{
            //    get;
            //}

            //[JsonProperty("lon")]
            //public double Longitude
            //{
            //    get;
            //}

            var p = new Nest.GeoLocation(1.23, 5.67);
            var expectedJson = "{\"lat\":1.23,\"lon\":5.67}";
            var actualJson = "{\"Latitude\":1.23,\"Longitude\":5.67}";

            
            var json = JsonConvert.SerializeObject(p);
            
            json.Should().NotBe(expectedJson);
            
            json.Should().Be(actualJson);
        }
    }
}
