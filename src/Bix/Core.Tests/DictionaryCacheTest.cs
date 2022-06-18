/***************************************************************************/
// Copyright 2013-2022 Riley White
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

using Autofac;
using Autofac.Extras.Moq;
using AutoFixture;
using System;
using System.Collections.Generic;
using Xunit;

namespace Bix.Core
{
    public class DictionaryCacheTest
    {
        [Fact]
        public void ConstructorUsesPassedDictionaryForStorage()
        {
            // arrange
            var fixture = new Fixture();
            var key = fixture.Create<string>();
            var value = fixture.Create<object>();
            var store = new Dictionary<string, object>
            {
                { key, value }
            };
            var cache = new DictionaryCache(store);

            // act
            var retrievedValue = cache.Get(key);

            //assert
            Assert.Same(value, retrievedValue);
        }

        [Fact]
        public void GetNullAndSetNullThrowExceptions()
        {
            using (var mock = AutoMock.GetStrict())
            {
                // arrange
                mock.Mock<IDictionary<string, object>>();
                var cache = mock.Create<DictionaryCache>();
                var fixture = new Fixture();

                // act & assert
                Assert.Throws<ArgumentNullException>("key", () => cache.Get(null));
                Assert.Throws<ArgumentNullException>("key", () => cache.Set(null, fixture.Create<object>()));
            }
        }

        [Fact]
        public void GetPullsFromTheInnerDictionary()
        {
            var store = new Dictionary<string, object>();
            using (var mock = AutoMock.GetStrict(builder => builder.RegisterInstance<IDictionary<string, object>>(store)))
            {
                // arrange
                var cache = mock.Create<DictionaryCache>();
                var fixture = new Fixture();
                var key = fixture.Create<string>();
                var value = fixture.Create<object>();
                store[key] = value;

                // act
                var retreivedValue = cache.Get(key);

                // assert
                Assert.Same(value, retreivedValue);
            }
        }

        [Fact]
        public void GetNotFoundThrowsException()
        {
            using (var mock = AutoMock.GetStrict())
            {
                // arrange
                mock.Mock<IDictionary<string, object>>();
                var cache = mock.Create<DictionaryCache>();
                var fixture = new Fixture();
                var key = fixture.Create<string>();

                // act & assert
                Assert.ThrowsAny<Exception>(() => cache.Get(key));
            }
        }
    }
}
