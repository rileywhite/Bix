using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using System;
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

            Assert.AreEqual(
                referenceAssembly.Modules.Count,
                mixedAssembly.Modules.Count,
                string.Format("Reference assembly has {0} modules. Mixed assembly has {1} modules.", referenceAssembly.Modules.Count, mixedAssembly.Modules.Count));

            foreach (var referenceModule in referenceAssembly.Modules)
            {
                var mixedModule = mixedAssembly.Modules.SingleOrDefault(module => module.Name == referenceModule.Name.Replace(".MixerTestTargetsReference.", ".MixerTestTargets."));
                Assert.IsNotNull(mixedModule, "Could not find a unique mixed module matching reference module {0}", referenceModule.Name.Replace(".MixerTestTargetsReference.", ".MixerTestTargets."));

                foreach (var referenceType in referenceModule.Types)
                {
                    var mixedType = mixedModule.GetType(referenceType.FullName);
                    Assert.IsNotNull(mixedType);
                    Assert.AreEqual(referenceType.FullName, mixedType.FullName);

                    var methodComparison = new Comparison<MethodDefinition>(
                        (left, right) =>
                        {
                            if(left.FullName != right.FullName) { return left.FullName.CompareTo(right.FullName); }
                            if(left.Parameters.Count != right.Parameters.Count) { return left.Parameters.Count.CompareTo(right.Parameters.Count); }
                            for(var i = 0; i < left.Parameters.Count; i++)
                            {
                                if (left.Parameters[i].ParameterType.FullName != right.Parameters[i].ParameterType.FullName)
                                {
                                    return left.Parameters[i].ParameterType.FullName.CompareTo(right.Parameters[i].ParameterType.FullName);
                                }
                                if (left.Parameters[i].Name != right.Parameters[i].Name)
                                {
                                    return left.Parameters[i].Name.CompareTo(right.Parameters[i].Name);
                                }
                            }
                            return 0;
                        });
                    var sortedReferenceMethods = referenceType.Methods.ToList();
                    sortedReferenceMethods.Sort(methodComparison);
                    var sortedMixedMethods = mixedType.Methods.ToList();
                    sortedMixedMethods.Sort(methodComparison);

                    for (var i = 0; i < sortedReferenceMethods.Count; i++)
                    {
                        var referenceMethod = sortedReferenceMethods[i];
                        Assert.IsTrue(
                            i < sortedMixedMethods.Count,
                            string.Format("Could not find mixed method {0}.{1}", referenceType.FullName, GetMethodSignatureString(referenceMethod)));
                        var mixedMethod = sortedMixedMethods[i];
                        Assert.AreEqual(
                            GetMethodSignatureString(referenceMethod),
                            GetMethodSignatureString(mixedMethod),
                            "Did not find mixed method {0}.{1}", referenceType.FullName, GetMethodSignatureString(referenceMethod));
                    }

                    if (sortedReferenceMethods.Count < sortedMixedMethods.Count)
                    {
                        Assert.Fail(string.Format(
                            "Found extra mixed method {0}.{1}",
                            referenceType.FullName,
                            GetMethodSignatureString(sortedMixedMethods[sortedReferenceMethods.Count])));
                    }
                }

                Assert.AreEqual(
                    referenceModule.Types.Count,
                    mixedModule.Types.Count,
                    string.Format("Reference assembly has {0} types. Mixed assembly has {1} types.", referenceModule.Types.Count, mixedModule.Types.Count));
            }
        }

        private static string GetMethodSignatureString(MethodDefinition referenceMethod)
        {
            return string.Format("{0}({1})", referenceMethod.Name, GetMethodSignatureParametersString(referenceMethod));
        }

        private static string GetMethodSignatureParametersString(MethodDefinition referenceMethod)
        {
            return string.Join(", ", referenceMethod.Parameters.Select(parameter => string.Format("{0} {1}", parameter.ParameterType.Name, parameter.Name)).ToArray());
        }
    }
}
