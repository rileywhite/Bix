using Bix.Mix;
using Bix.Mix.Encapsulate;
using Bix.Mixers.MixerTestTargets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.Serialization;

namespace CecilMixerTest.EncapsulateTests
{
    [TestClass]
    public class IEncapsulatesImplementationTest
    {
        [TestMethod]
        public void IEncapsulatesIsImplemented()
        {
            Assert.AreEqual(
                typeof(EncapsulatesTestType).GetInterface(typeof(IEncapsulates).FullName),
                typeof(IEncapsulates),
                "Couldn't find IEncapsulates interface in list of test type's interfaces");

            Assert.AreEqual(
                typeof(EncapsulatesTestType).GetInterface(typeof(IMixes).FullName),
                typeof(IMixes),
                "Couldn't find IMixes interface in list of test type's interfaces");

            Assert.AreEqual(
                typeof(EncapsulatesTestType).GetInterface(typeof(ISerializable).FullName),
                typeof(ISerializable),
                "Couldn't find IMixes interface in list of test type's interfaces");
        }
    }
}
