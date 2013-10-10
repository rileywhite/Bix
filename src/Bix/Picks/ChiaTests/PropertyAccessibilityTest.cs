/***************************************************************************/
// PropertyAccessibilityTest.cs
// Copyright 2013 Riley White
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

using Bix.Mix.Encapsulate;
using Bix.Picks.MixTestTargets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Bix.Picks.ChiaTests
{
    [TestClass]
    public class PropertyAccessibilityTest
    {
        [TestMethod]
        public void IEncapsulatesIsImplemented()
        {
            Assert.AreEqual(
                typeof(EncapsulatesTestType).GetInterface(typeof(IEncapsulates).FullName),
                typeof(IEncapsulates),
                "Couldn't find IEncapsulates interface in list of test type's interfaces");
        }

        [TestMethod]
        public void InnerDtoTypeExists()
        {
            Assert.IsNotNull(typeof(EncapsulatesTestType).GetNestedType("Dto"));
        }

        [TestMethod]
        public void CorrectPropertiesAreRepresentedInDto()
        {
            Type dtoType = typeof(EncapsulatesTestType).GetNestedType("Dto");
            Assert.AreEqual(33, dtoType.GetProperties().Length);
            var propertyNames = new string[]
            {
                "PublicGetPublicSet",
                "PublicGetPrivateSet",
                "PublicGetInternalSet",
                "PublicGetProtectedSet",
                "PublicGetProtectedInternalSet",
                "PrivateGetPublicSet",
                "PrivateGetPrivateSet",
                "PrivateGetInternalSet",
                "PrivateGetProtectedSet",
                "PrivateGetProtectedInternalSet",
                "InternalGetPublicSet",
                "InternalGetPrivateSet",
                "InternalGetInternalSet",
                "InternalGetProtectedInternalSet",
                "ProtectedGetPublicSet",
                "ProtectedGetPrivateSet",
                "ProtectedGetProtectedSet",
                "ProtectedGetProtectedInternalSet",
                "ProtectedInternalGetPublicSet",
                "ProtectedInternalGetPrivateSet",
                "ProtectedInternalGetInternalSet",
                "ProtectedInternalGetProtectedSet",
                "ProtectedInternalGetProtectedInternalSet",
                "PublicGet",
                "PrivateGet",
                "InternalGet",
                "ProtectedGet",
                "ProtectedInternalGet",
                "PublicSet",
                "PrivateSet",
                "InternalSet",
                "ProtectedSet",
                "ProtectedInternalSet"
            };

            foreach (var propertyName in propertyNames)
            {
                var property = dtoType.GetProperty(propertyName);
                Assert.IsNotNull(property);
                Assert.IsTrue(property.CanRead);
                Assert.IsTrue(property.CanWrite);
            }
        }
    }
}
