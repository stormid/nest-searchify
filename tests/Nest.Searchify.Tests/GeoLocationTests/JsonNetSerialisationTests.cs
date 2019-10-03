using FluentAssertions;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Xunit;

namespace Nest.Searchify.Tests.GeoLocationTests
{
    public class JsonNetSerialisationTests
    {
        public class SampleObjectWithJsonProperty
        {
            [JsonProperty("lat")]
            public double Latitude { get; set; }
        }

        [DataContract]
        public class SampleObjectWithDataMember
        {
            [DataMember(Name = "lat")]
            public double Latitude { get; set; }
        }

        [Fact]
        public void ShouldSerialiseFromJsonPropertyAttributes()
        {
            var json = @"{ ""lat"" : ""1.234"" }";

            var result = JsonConvert.DeserializeObject<SampleObjectWithJsonProperty>(json);

            result.Latitude.Should().Be(1.234);
        }

        [Fact]
        public void ShouldSerialiseFromDataMemberAttributes()
        {
            var json = @"{ ""lat"" : ""1.234"" }";


            var result = JsonConvert.DeserializeObject<SampleObjectWithDataMember>(json);

            result.Latitude.Should().Be(1.234);
        }

        [Fact(Skip = "This is broken!!")]
        public void ShouldSerialiseNESTGeoLocation()
        {
            var json = @"{ ""lat"" : ""1.234"" }";

            var result = JsonConvert.DeserializeObject<Nest.GeoLocation>(json);

            result.Latitude.Should().Be(1.234);
        }
    }
}