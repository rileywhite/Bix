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

            if (sortedExpected.Count < sortedActual.Count)
            {
                Assert.Fail("Found extra module {0} in mixed assembly", sortedActual[sortedExpected.Count].FullyQualifiedName);
            }
        }

        private static void AssertEquality(ModuleDefinition expected, ModuleDefinition actual)
        {
            AssertEquality(expected.Types, actual.Types);
        }

        private static void AssertEquality(IEnumerable<TypeDefinition> expected, IEnumerable<TypeDefinition> actual)
        {
            var sortedExpected = expected.ToList();
            sortedExpected.Sort(Comparisons.TypeComparison);
            var sortedActual = actual.ToList();
            sortedActual.Sort(Comparisons.TypeComparison);

            for (var i = 0; i < sortedExpected.Count && i < sortedActual.Count; i++)
            {
                AssertEquality(sortedExpected[i], sortedActual[i]);
            }

            if (sortedExpected.Count > sortedActual.Count)
            {
                Assert.Fail("Did not find expected type {0} in mixed assembly", sortedExpected[sortedActual.Count].FullName);
            }

            if (sortedExpected.Count < sortedActual.Count)
            {
                Assert.Fail("Found extra type {0} in mixed assembly", sortedActual[sortedExpected.Count].FullName);
            }
        }

        private static void AssertEquality(TypeDefinition expected, TypeDefinition actual)
        {
            AssertEquality(expected.Methods, actual.Methods);
            AssertEquality(expected.Fields, actual.Fields);
        }

        private static void AssertEquality(IEnumerable<MethodDefinition> expected, IEnumerable<MethodDefinition> actual)
        {
            var sortedExpected = expected.ToList();
            sortedExpected.Sort(Comparisons.MethodComparison);
            var sortedActual = actual.ToList();
            sortedActual.Sort(Comparisons.MethodComparison);

            for (var i = 0; i < sortedExpected.Count && i < sortedActual.Count; i++)
            {
                AssertEquality(sortedExpected[i], sortedActual[i]);
            }

            if (sortedExpected.Count > sortedActual.Count)
            {
                Assert.Fail("Did not find expected method {0} in type {1} of mixed assembly", sortedExpected[sortedActual.Count].FullName, sortedExpected[sortedActual.Count].DeclaringType.FullName);
            }

            if (sortedExpected.Count < sortedActual.Count)
            {
                Assert.Fail("Found extra method {0} in type {1} of mixed assembly", sortedActual[sortedExpected.Count].FullName, sortedActual[sortedExpected.Count].DeclaringType.FullName);
            }
        }

        private static void AssertEquality(MethodDefinition expected, MethodDefinition actual)
        {
            // TODO
        }

        private static void AssertEquality(IEnumerable<PropertyDefinition> expected, IEnumerable<PropertyDefinition> actual)
        {
            var sortedExpected = expected.ToList();
            sortedExpected.Sort(Comparisons.PropertyComparison);
            var sortedActual = actual.ToList();
            sortedActual.Sort(Comparisons.PropertyComparison);

            for (var i = 0; i < sortedExpected.Count && i < sortedActual.Count; i++)
            {
                AssertEquality(sortedExpected[i], sortedActual[i]);
            }

            if (sortedExpected.Count > sortedActual.Count)
            {
                Assert.Fail("Did not find expected property {0} in type {1} of mixed assembly", sortedExpected[sortedActual.Count].FullName, sortedExpected[sortedActual.Count].DeclaringType.FullName);
            }

            if (sortedExpected.Count < sortedActual.Count)
            {
                Assert.Fail("Found extra propery {0} in type {1} of mixed assembly", sortedActual[sortedExpected.Count].FullName, sortedActual[sortedExpected.Count].DeclaringType.FullName);
            }
        }

        private static void AssertEquality(PropertyDefinition expected, PropertyDefinition actual)
        {
            // TODO
        }

        private static void AssertEquality(IEnumerable<FieldDefinition> expected, IEnumerable<FieldDefinition> actual)
        {
            var sortedExpected = expected.ToList();
            sortedExpected.Sort(Comparisons.FieldComparison);
            var sortedActual = actual.ToList();
            sortedActual.Sort(Comparisons.FieldComparison);

            for (var i = 0; i < sortedExpected.Count && i < sortedActual.Count; i++)
            {
                AssertEquality(sortedExpected[i], sortedActual[i]);
            }

            if (sortedExpected.Count > sortedActual.Count)
            {
                Assert.Fail("Did not find expected field {0} in type {1} of mixed assembly", sortedExpected[sortedActual.Count].FullName, sortedExpected[sortedActual.Count].DeclaringType.FullName);
            }

            if (sortedExpected.Count < sortedActual.Count)
            {
                Assert.Fail("Found extra field {0} in type {1} of mixed assembly", sortedActual[sortedExpected.Count].FullName, sortedActual[sortedExpected.Count].DeclaringType.FullName);
            }
        }

        private static void AssertEquality(FieldDefinition expected, FieldDefinition actual)
        {
            // TODO
        }

        private static void AssertEquality(IEnumerable<CustomAttribute> expected, IEnumerable<CustomAttribute> actual, MemberReference attributeTarget)
        {
            var sortedExpected = expected.ToList();
            sortedExpected.Sort(Comparisons.CustomAttributeComparison);
            var sortedActual = actual.ToList();
            sortedActual.Sort(Comparisons.CustomAttributeComparison);

            for (var i = 0; i < sortedExpected.Count && i < sortedActual.Count; i++)
            {
                AssertEquality(sortedExpected[i], sortedActual[i]);
            }

            if (sortedExpected.Count > sortedActual.Count)
            {
                Assert.Fail("Did not find expected attribute {0} for target {1} of mixed assembly", sortedExpected[sortedActual.Count].AttributeType, attributeTarget.FullName);
            }

            if (sortedExpected.Count < sortedActual.Count)
            {
                Assert.Fail("Found extra attribute {0} in type {1} of mixed assembly", sortedActual[sortedExpected.Count].AttributeType, attributeTarget.FullName);
            }
        }

        private static void AssertEquality(CustomAttribute expected, CustomAttribute actual)
        {
            // TODO
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
