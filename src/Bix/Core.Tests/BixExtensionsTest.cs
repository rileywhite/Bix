using Newtonsoft.Json;
using AutoFixture;
using System;
using Xunit;

namespace Bix.Core
{
    public class BixExtensionsTest
    {
        [Fact]
        public void ToJsonFailsOnNull()
        {
            // arrange
            object obj = null;

            // act & assert
            Assert.Throws<ArgumentNullException>("source", () => obj.ToJson());
        }

        [Fact]
        public void ToJsonForwardsCallToJsonSerializer()
        {
            // arrange
            var fixture = new Fixture();
            var str = fixture.Create<string>();

            var indentedSerializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented };
            var jsonWithDefaultSettings = JsonConvert.SerializeObject(str);
            var jsonWithIndentedSettings = JsonConvert.SerializeObject(str, indentedSerializerSettings);

            // act
            var json = str.ToJson();

            // assert
            Assert.Equal(jsonWithDefaultSettings, json);
        }

        [Fact]
        public void ToJsonForwardsCallToJsonSerializerWithGivenSettings()
        {
            // arrange
            var fixture = new Fixture();
            var str = fixture.Create<string>();

            var indentedSerializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented };
            var jsonWithIndentedSettings = JsonConvert.SerializeObject(str, indentedSerializerSettings);

            // act
            var json = str.ToJson(indentedSerializerSettings);

            // assert
            Assert.Equal(jsonWithIndentedSettings, json);
        }
    }
}
