/***************************************************************************/
// Copyright 2013-2017 Riley White
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
/***************************************************************************/

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

        [Fact]
        public void ConvertFromJsonFailsOnNull()
        {
            // arrange
            string json = null;

            // act & assert
            Assert.Throws<ArgumentNullException>("source", () => json.ConvertFromJson<object>());
        }

        [Fact]
        public void ConvertFromJsonReturnsNullForObjectTypeAndInvalidJson()
        {
            // arrange
            string json = "";

            // act
            var value = json.ConvertFromJson<object>();

            // assert
            Assert.Null(value);
        }

        [Fact]
        public void ConvertFromJsonFailsForValueTypeAndInvalidJson()
        {
            // arrange
            string json = "";

            // act & assert
            Assert.Throws<JsonSerializationException>(() => json.ConvertFromJson<decimal>());
        }

        [Fact]
        public void ConvertFromJsonWorksForObjectType()
        {
            // arrange
            string json = "hello";

            // act
            var str = json.ConvertFromJson<string>();

            // assert
            Assert.Equal("hello", str);
        }
    }
}
