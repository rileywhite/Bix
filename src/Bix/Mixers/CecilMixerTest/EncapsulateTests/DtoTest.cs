using Bix.Mix.Encapsulate;
using Bix.Mixers.MixerTestTargets;
using NUnit.Framework;
using System;

namespace Bix.Mixers.CecilMixerTest.EncapsulateTests
{
    [TestFixture]
    public class DtoTest
    {
        [Test]
        public void DtoTypeCreatedCorrectly()
        {
            Assert.IsNotNull(typeof(EncapsulatesTestType).GetNestedType("Dto"));
        }

        [Test]
        public void CorrectPropertiesAreRepresentedAndAreReadWriteInDto()
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
