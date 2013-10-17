using Bix.Mixers.CecilMixerTest.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CecilMixerTest
{
    [TestClass]
    public class ReferenceAssemblyTest
    {
        [TestMethod]
        public void ReferenceTest()
        {
            var referenceAssembly = AssemblyDefinition.ReadAssembly("Bix.Mixers.MixerTestTargetsReference.dll");
            var mixedAssembly = AssemblyDefinition.ReadAssembly("Bix.Mixers.MixerTestTargets.dll");

            AssertEquality(referenceAssembly, mixedAssembly);
        }

        private static void AssertEquality(AssemblyDefinition expected, AssemblyDefinition actual)
        {
            AssertEquality(expected.Modules, actual.Modules);
        }

        private static void AssertEquality(IEnumerable<ModuleDefinition> expected, IEnumerable<ModuleDefinition> actual)
        {
            var sortedExpected = expected.ToList();
            sortedExpected.Sort(Comparisons.ModuleComparison);
            var sortedActual = actual.ToList();
            sortedActual.Sort(Comparisons.ModuleComparison);

            for(var i = 0; i < sortedExpected.Count && i < sortedActual.Count; i++)
            {
                AssertEquality(sortedExpected[i], sortedActual[i]);
            }

            if (sortedExpected.Count > sortedActual.Count)
            {
                Assert.Fail("Did not find expected module {0} in mixed assembly", sortedExpected[sortedActual.Count].FullyQualifiedName);
            }
        }

        private static void AssertEquality(ModuleDefinition expected, ModuleDefinition actual)
        {
            foreach (var expectedType in expected.Types)
            {
                var mixedType = actual.GetType(expectedType.FullName);
                Assert.IsNotNull(mixedType);
                Assert.AreEqual(expectedType.FullName, mixedType.FullName);

                var sortedReferenceAttributes = expectedType.CustomAttributes.ToList();
                sortedReferenceAttributes.Sort(Comparisons.CustomAttributeComparison);

                AssertEquality(expectedType, mixedType);
            }

            Assert.AreEqual(
                expected.Types.Count,
                actual.Types.Count,
                string.Format("Reference assembly has {0} types. Mixed assembly has {1} types.", expected.Types.Count, actual.Types.Count));
        }

        private static void AssertEquality(TypeDefinition expected, TypeDefinition actual)
        {
            var sortedExpected = expected.Methods.ToList();
            sortedExpected.Sort(Comparisons.MethodComparison);
            var sortedActual = actual.Methods.ToList();
            sortedActual.Sort(Comparisons.MethodComparison);

            for (var i = 0; i < sortedExpected.Count; i++)
            {
                var referenceMethod = sortedExpected[i];
                Assert.IsTrue(
                    i < sortedActual.Count,
                    string.Format("Could not find mixed method {0}.{1}", expected.FullName, GetMethodSignatureString(referenceMethod)));
                var mixedMethod = sortedActual[i];
                Assert.AreEqual(
                    GetMethodSignatureString(referenceMethod),
                    GetMethodSignatureString(mixedMethod),
                    "Did not find mixed method {0}.{1}", expected.FullName, GetMethodSignatureString(referenceMethod));
            }

            if (sortedExpected.Count < sortedActual.Count)
            {
                Assert.Fail(string.Format(
                    "Found extra mixed method {0}.{1}",
                    expected.FullName,
                    GetMethodSignatureString(sortedActual[sortedExpected.Count])));
            }
        }

        private static string GetMethodSignatureString(MethodDefinition method)
        {
            return string.Format("{0} {1}({2})", method.ReturnType, method.Name, GetMethodSignatureParametersString(method));
        }

        private static string GetMethodSignatureParametersString(MethodDefinition method)
        {
            return string.Join(", ", method.Parameters.Select(parameter => string.Format("{0} {1}", parameter.ParameterType.Name, parameter.Name)).ToArray());
        }
    }
}
