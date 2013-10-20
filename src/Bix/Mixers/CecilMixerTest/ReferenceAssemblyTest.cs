using Bix.Mixers.CecilMixerTest.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CecilMixerTest
{
    [TestClass]
    public class ReferenceAssemblyTest
    {
        private static readonly string ReferenceAssemblyName = "Bix.Mixers.MixerTestTargetsReference.dll";
        private static readonly string MixedAssemblyName = "Bix.Mixers.MixerTestTargets.dll";

        [TestMethod]
        public void ReferenceTest()
        {
            var referenceAssembly = AssemblyDefinition.ReadAssembly(ReferenceAssemblyName);
            var mixedAssembly = AssemblyDefinition.ReadAssembly(MixedAssemblyName);

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

        private static Dictionary<string, List<string>> checkedMethodNamesByTypeName = new Dictionary<string, List<string>>();
        private static void AssertEquality(TypeDefinition expected, TypeDefinition actual)
        {
            Assert.AreEqual(expected.FullName, actual.FullName, string.Format("Looking for type {0} but found {1} in mixed assembly", expected.Name, actual.Name));

            if (!checkedMethodNamesByTypeName.ContainsKey(actual.FullName))
            {
                checkedMethodNamesByTypeName.Add(actual.FullName, new List<string>());

                // assume correctness of types that are not part of the assemblies being checked
                if (expected.Module.Assembly.Name.Name != ReferenceAssemblyName && actual.Module.Assembly.Name.Name != MixedAssemblyName) { return; }

                Assert.AreEqual(expected.IsAbstract, actual.IsAbstract, string.Format("Type {0} in mixed assembly does not match the reference value for IsAbstract", expected.Name));
                Assert.AreEqual(expected.IsAnsiClass, actual.IsAnsiClass, string.Format("Type {0} in mixed assembly does not match the reference value for IsAnsiClass", expected.Name));
                Assert.AreEqual(expected.IsArray, actual.IsArray, string.Format("Type {0} in mixed assembly does not match the reference value for IsArray", expected.Name));
                Assert.AreEqual(expected.IsAutoClass, actual.IsAutoClass, string.Format("Type {0} in mixed assembly does not match the reference value for IsAutoClass", expected.Name));
                Assert.AreEqual(expected.IsAutoLayout, actual.IsAutoLayout, string.Format("Type {0} in mixed assembly does not match the reference value for IsAutoLayout", expected.Name));
                Assert.AreEqual(expected.IsBeforeFieldInit, actual.IsBeforeFieldInit, string.Format("Type {0} in mixed assembly does not match the reference value for IsBeforeFieldInit", expected.Name));
                Assert.AreEqual(expected.IsByReference, actual.IsByReference, string.Format("Type {0} in mixed assembly does not match the reference value for IsByReference", expected.Name));
                Assert.AreEqual(expected.IsClass, actual.IsClass, string.Format("Type {0} in mixed assembly does not match the reference value for IsClass", expected.Name));
                Assert.AreEqual(expected.IsDefinition, actual.IsDefinition, string.Format("Type {0} in mixed assembly does not match the reference value for IsDefinition", expected.Name));
                Assert.AreEqual(expected.IsEnum, actual.IsEnum, string.Format("Type {0} in mixed assembly does not match the reference value for IsEnum", expected.Name));
                Assert.AreEqual(expected.IsExplicitLayout, actual.IsExplicitLayout, string.Format("Type {0} in mixed assembly does not match the reference value for IsExplicitLayout", expected.Name));
                Assert.AreEqual(expected.IsFunctionPointer, actual.IsFunctionPointer, string.Format("Type {0} in mixed assembly does not match the reference value for IsFunctionPointer", expected.Name));
                Assert.AreEqual(expected.IsGenericInstance, actual.IsGenericInstance, string.Format("Type {0} in mixed assembly does not match the reference value for IsGenericInstance", expected.Name));
                Assert.AreEqual(expected.IsGenericParameter, actual.IsGenericParameter, string.Format("Type {0} in mixed assembly does not match the reference value for IsGenericParameter", expected.Name));
                Assert.AreEqual(expected.IsImport, actual.IsImport, string.Format("Type {0} in mixed assembly does not match the reference value for IsImport", expected.Name));
                Assert.AreEqual(expected.IsInterface, actual.IsInterface, string.Format("Type {0} in mixed assembly does not match the reference value for IsInterface", expected.Name));
                Assert.AreEqual(expected.IsNested, actual.IsNested, string.Format("Type {0} in mixed assembly does not match the reference value for IsNested", expected.Name));
                Assert.AreEqual(expected.IsNestedAssembly, actual.IsNestedAssembly, string.Format("Type {0} in mixed assembly does not match the reference value for IsNestedAssembly", expected.Name));
                Assert.AreEqual(expected.IsNestedFamily, actual.IsNestedFamily, string.Format("Type {0} in mixed assembly does not match the reference value for IsNestedFamily", expected.Name));
                Assert.AreEqual(expected.IsNestedFamilyAndAssembly, actual.IsNestedFamilyAndAssembly, string.Format("Type {0} in mixed assembly does not match the reference value for IsNestedFamilyAndAssembly", expected.Name));
                Assert.AreEqual(expected.IsNestedFamilyOrAssembly, actual.IsNestedFamilyOrAssembly, string.Format("Type {0} in mixed assembly does not match the reference value for IsNestedFamilyOrAssembly", expected.Name));
                Assert.AreEqual(expected.IsNestedPrivate, actual.IsNestedPrivate, string.Format("Type {0} in mixed assembly does not match the reference value for IsNestedPrivate", expected.Name));
                Assert.AreEqual(expected.IsNestedPublic, actual.IsNestedPublic, string.Format("Type {0} in mixed assembly does not match the reference value for IsNestedPublic", expected.Name));
                Assert.AreEqual(expected.IsNotPublic, actual.IsNotPublic, string.Format("Type {0} in mixed assembly does not match the reference value for IsNotPublic", expected.Name));
                Assert.AreEqual(expected.IsOptionalModifier, actual.IsOptionalModifier, string.Format("Type {0} in mixed assembly does not match the reference value for IsOptionalModifier", expected.Name));
                Assert.AreEqual(expected.IsPinned, actual.IsPinned, string.Format("Type {0} in mixed assembly does not match the reference value for IsPinned", expected.Name));
                Assert.AreEqual(expected.IsPointer, actual.IsPointer, string.Format("Type {0} in mixed assembly does not match the reference value for IsPointer", expected.Name));
                Assert.AreEqual(expected.IsPrimitive, actual.IsPrimitive, string.Format("Type {0} in mixed assembly does not match the reference value for IsPrimitive", expected.Name));
                Assert.AreEqual(expected.IsPublic, actual.IsPublic, string.Format("Type {0} in mixed assembly does not match the reference value for IsPublic", expected.Name));
                Assert.AreEqual(expected.IsRequiredModifier, actual.IsRequiredModifier, string.Format("Type {0} in mixed assembly does not match the reference value for IsRequiredModifier", expected.Name));
                Assert.AreEqual(expected.IsRuntimeSpecialName, actual.IsRuntimeSpecialName, string.Format("Type {0} in mixed assembly does not match the reference value for IsRuntimeSpecialName", expected.Name));
                Assert.AreEqual(expected.IsSealed, actual.IsSealed, string.Format("Type {0} in mixed assembly does not match the reference value for IsSealed", expected.Name));
                Assert.AreEqual(expected.IsSentinel, actual.IsSentinel, string.Format("Type {0} in mixed assembly does not match the reference value for IsSentinel", expected.Name));
                Assert.AreEqual(expected.IsSequentialLayout, actual.IsSequentialLayout, string.Format("Type {0} in mixed assembly does not match the reference value for IsSequentialLayout", expected.Name));
                Assert.AreEqual(expected.IsSerializable, actual.IsSerializable, string.Format("Type {0} in mixed assembly does not match the reference value for IsSerializable", expected.Name));
                Assert.AreEqual(expected.IsSpecialName, actual.IsSpecialName, string.Format("Type {0} in mixed assembly does not match the reference value for IsSpecialName", expected.Name));
                Assert.AreEqual(expected.IsUnicodeClass, actual.IsUnicodeClass, string.Format("Type {0} in mixed assembly does not match the reference value for IsUnicodeClass", expected.Name));
                Assert.AreEqual(expected.IsValueType, actual.IsValueType, string.Format("Type {0} in mixed assembly does not match the reference value for IsValueType", expected.Name));
                Assert.AreEqual(expected.IsWindowsRuntime, actual.IsWindowsRuntime, string.Format("Type {0} in mixed assembly does not match the reference value for IsWindowsRuntime", expected.Name));

                if (expected.BaseType != null || actual.BaseType != null)
                {
                    if (expected.BaseType != null && actual.BaseType == null)
                    {
                        Assert.Fail(string.Format(
                            "Type {0} has no base type when base type {1} was expected in mixed assembly",
                            actual.Name,
                            expected.BaseType.Name));
                    }
                    else if (expected.BaseType != null && actual.BaseType == null)
                    {
                        Assert.Fail(string.Format(
                            "Type {0} has base type {1}, but no base type was expected in mixed assembly",
                            actual.Name,
                            actual.BaseType.Name));
                    }
                    else
                    {
                        AssertEquality(expected.BaseType.Resolve(), actual.BaseType.Resolve());
                    }
                }

                AssertEquality(
                    from @interface in expected.Interfaces select @interface.Resolve(),
                    from @interface in actual.Interfaces select @interface.Resolve());
                AssertEquality(expected.Methods, actual.Methods);
                AssertEquality(expected.Properties, actual.Properties);
                AssertEquality(expected.Fields, actual.Fields);
                AssertEquality(expected.Events, actual.Events);
                AssertEquality(expected.GenericParameters, actual.GenericParameters, actual);
                AssertEquality(expected.SecurityDeclarations, actual.SecurityDeclarations);
                AssertEquality(expected.CustomAttributes, actual.CustomAttributes, actual);
            }
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
            if (expected == null && actual == null) { return; }
            if (expected == null && actual != null)
            {
                Assert.Fail(string.Format("Expected no method but found {0} in type {1} of mixed assembly", actual.FullName, actual.DeclaringType));
            }
            if (expected != null && actual == null)
            {
                Assert.Fail(string.Format("Expected method {0} in type {1} but found none of mixed assembly", expected.FullName, expected.DeclaringType));
            }

            Assert.AreEqual(expected.FullName, actual.FullName);
            Assert.AreEqual(expected.DeclaringType.FullName, actual.DeclaringType.FullName);
            AssertEquality(expected.Parameters, actual.Parameters, actual);
            Assert.AreEqual(expected.ReturnType.FullName, actual.ReturnType.FullName);

            if (!checkedMethodNamesByTypeName.ContainsKey(actual.DeclaringType.FullName))
            {
                AssertEquality(expected.DeclaringType, actual.DeclaringType);
            }
            if (!checkedMethodNamesByTypeName[actual.DeclaringType.FullName].Contains(GetMethodSignatureString(actual)))
            {
                checkedMethodNamesByTypeName[actual.DeclaringType.FullName].Add(GetMethodSignatureString(actual));

                AssertEquality(
                    from methodReference in expected.Overrides select methodReference.Resolve(),
                    from methodReference in actual.Overrides select methodReference.Resolve());

                Assert.AreEqual(expected.Attributes, actual.Attributes, string.Format("Attributes for mixed method {0} on type {1} differ from reference", actual.FullName, actual.DeclaringType.FullName));
                Assert.AreEqual(expected.CallingConvention, actual.CallingConvention);
                Assert.AreEqual(expected.ExplicitThis, actual.ExplicitThis);
                Assert.AreEqual(expected.ImplAttributes, actual.ImplAttributes);
                Assert.AreEqual(expected.IsAbstract, actual.IsAbstract);
                Assert.AreEqual(expected.IsAddOn, actual.IsAddOn);
                Assert.AreEqual(expected.IsAssembly, actual.IsAssembly);
                Assert.AreEqual(expected.IsCheckAccessOnOverride, actual.IsCheckAccessOnOverride);
                Assert.AreEqual(expected.IsCompilerControlled, actual.IsCompilerControlled);
                Assert.AreEqual(expected.IsConstructor, actual.IsConstructor);
                Assert.AreEqual(expected.IsDefinition, actual.IsDefinition);
                Assert.AreEqual(expected.IsFamily, actual.IsFamily);
                Assert.AreEqual(expected.IsFamilyAndAssembly, actual.IsFamilyAndAssembly);
                Assert.AreEqual(expected.IsFamilyOrAssembly, actual.IsFamilyOrAssembly);
                Assert.AreEqual(expected.IsFinal, actual.IsFinal);
                Assert.AreEqual(expected.IsFire, actual.IsFire);
                Assert.AreEqual(expected.IsForwardRef, actual.IsForwardRef);
                Assert.AreEqual(expected.IsGenericInstance, actual.IsGenericInstance);
                Assert.AreEqual(expected.IsGetter, actual.IsGetter);
                Assert.AreEqual(expected.IsHideBySig, actual.IsHideBySig);
                Assert.AreEqual(expected.IsIL, actual.IsIL);
                Assert.AreEqual(expected.IsInternalCall, actual.IsInternalCall);
                Assert.AreEqual(expected.IsManaged, actual.IsManaged);
                Assert.AreEqual(expected.IsNative, actual.IsNative);
                Assert.AreEqual(expected.IsNewSlot, actual.IsNewSlot);
                Assert.AreEqual(expected.IsOther, actual.IsOther);
                Assert.AreEqual(expected.IsPInvokeImpl, actual.IsPInvokeImpl);
                Assert.AreEqual(expected.IsPreserveSig, actual.IsPreserveSig);
                Assert.AreEqual(expected.IsPrivate, actual.IsPrivate);
                Assert.AreEqual(expected.IsPublic, actual.IsPublic);
                Assert.AreEqual(expected.IsRemoveOn, actual.IsRemoveOn);
                Assert.AreEqual(expected.IsReuseSlot, actual.IsReuseSlot);
                Assert.AreEqual(expected.IsRuntime, actual.IsRuntime);
                Assert.AreEqual(expected.IsRuntimeSpecialName, actual.IsRuntimeSpecialName);
                Assert.AreEqual(expected.IsSetter, actual.IsSetter);
                Assert.AreEqual(expected.IsSpecialName, actual.IsSpecialName);
                Assert.AreEqual(expected.IsStatic, actual.IsStatic);
                Assert.AreEqual(expected.IsSynchronized, actual.IsSynchronized);
                Assert.AreEqual(expected.IsUnmanaged, actual.IsUnmanaged);
                Assert.AreEqual(expected.IsUnmanagedExport, actual.IsUnmanagedExport);
                Assert.AreEqual(expected.IsVirtual, actual.IsVirtual);
                Assert.AreEqual(expected.NoInlining, actual.NoInlining);
                Assert.AreEqual(expected.NoOptimization, actual.NoOptimization);
                Assert.AreEqual(expected.SemanticsAttributes, actual.SemanticsAttributes);

                AssertEquality(expected.SecurityDeclarations, actual.SecurityDeclarations);
                AssertEquality(expected.GenericParameters, actual.GenericParameters, actual);
                AssertEquality(expected.CustomAttributes, actual.CustomAttributes, actual);

                AssertEquality(expected.Body, actual.Body);
            }
        }

        private static void AssertEquality(MethodBody expected, MethodBody actual)
        {
            if (expected == null && actual == null) { return; }
            if (expected == null && actual != null)
            {
                Assert.Fail(string.Format("Expected no method body but found one on method {0} in type {1} of mixed assembly", actual.Method.FullName, actual.Method.DeclaringType.FullName));
            }
            if (expected != null && actual == null)
            {
                Assert.Fail(string.Format("Expected method body but found none on method {0} in type {1} of mixed assembly", expected.Method.FullName, expected.Method.DeclaringType.FullName));
            }

            int actualI = 0;
            int expectedI = 0;
            for (; expectedI < expected.Instructions.Count && actualI < actual.Instructions.Count; expectedI++)
            {
                if (expected.Instructions[expectedI].OpCode == OpCodes.Nop) { continue; }
                AssertEquality(expected.Instructions[expectedI], actual.Instructions[actualI]);
                ++actualI;
            }
        }

        private static void AssertEquality(Instruction expected, Instruction actual)
        {
            Assert.AreEqual(expected.Offset, actual.Offset);
            Assert.AreEqual(expected.OpCode, actual.OpCode);
            if (expected.Operand == null && actual.Operand == null) { return; }
            AssertEquality(expected.Operand as dynamic, actual.Operand as dynamic);
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
            Assert.AreEqual(expected.DeclaringType.FullName, actual.DeclaringType.FullName);
            Assert.AreEqual(expected.PropertyType.FullName, actual.PropertyType.FullName);
            Assert.AreEqual(expected.FullName, actual.FullName);
            AssertEquality(expected.GetMethod, actual.GetMethod);
            AssertEquality(expected.SetMethod, actual.SetMethod);
            AssertEquality(expected.OtherMethods, actual.OtherMethods);
            AssertEquality(expected.Parameters, actual.Parameters, actual);

            Assert.AreEqual(expected.Attributes, actual.Attributes);
            Assert.AreEqual(expected.IsDefinition, actual.IsDefinition);
            Assert.AreEqual(expected.IsRuntimeSpecialName, actual.IsRuntimeSpecialName);
            Assert.AreEqual(expected.IsSpecialName, actual.IsSpecialName);

            AssertEquality(expected.CustomAttributes, actual.CustomAttributes, actual);
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
            Assert.AreEqual(expected.DeclaringType.FullName, actual.DeclaringType.FullName);
            Assert.AreEqual(expected.FieldType.FullName, actual.FieldType.FullName);
            Assert.AreEqual(expected.FullName, actual.FullName);
            Assert.AreEqual(expected.InitialValue, actual.InitialValue); // may need to be a better comparison

            Assert.AreEqual(expected.Attributes, actual.Attributes);
            Assert.AreEqual(expected.IsAssembly, actual.IsAssembly);
            Assert.AreEqual(expected.IsCompilerControlled, actual.IsCompilerControlled);
            Assert.AreEqual(expected.IsDefinition, actual.IsDefinition);
            Assert.AreEqual(expected.IsFamily, actual.IsFamily);
            Assert.AreEqual(expected.IsFamilyAndAssembly, actual.IsFamilyAndAssembly);
            Assert.AreEqual(expected.IsFamilyOrAssembly, actual.IsFamilyOrAssembly);
            Assert.AreEqual(expected.IsInitOnly, actual.IsInitOnly);
            Assert.AreEqual(expected.IsLiteral, actual.IsLiteral);
            Assert.AreEqual(expected.IsNotSerialized, actual.IsNotSerialized);
            Assert.AreEqual(expected.IsPInvokeImpl, actual.IsPInvokeImpl);
            Assert.AreEqual(expected.IsPrivate, actual.IsPrivate);
            Assert.AreEqual(expected.IsPublic, actual.IsPublic);
            Assert.AreEqual(expected.IsRuntimeSpecialName, actual.IsRuntimeSpecialName);
            Assert.AreEqual(expected.IsSpecialName, actual.IsSpecialName);
            Assert.AreEqual(expected.IsStatic, actual.IsStatic);
            Assert.AreEqual(expected.Offset, actual.Offset);

            AssertEquality(expected.CustomAttributes, actual.CustomAttributes, actual);
        }

        private static void AssertEquality(IEnumerable<CustomAttribute> expected, IEnumerable<CustomAttribute> actual, MemberReference attributeTarget)
        {
            var sortedExpected = expected.ToList();
            sortedExpected.Sort(Comparisons.CustomAttributeComparison);
            var sortedActual = actual.ToList();
            sortedActual.Sort(Comparisons.CustomAttributeComparison);

            for (var i = 0; i < sortedExpected.Count && i < sortedActual.Count; i++)
            {
                AssertEquality(sortedExpected[i], sortedActual[i], attributeTarget);
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

        private static void AssertEquality(CustomAttribute expected, CustomAttribute actual, MemberReference attributeTarget)
        {
            Assert.AreEqual(
                expected.AttributeType.FullName,
                actual.AttributeType.FullName,
                string.Format("Expected attribute of type {0} but found type {1} for {2}", expected.AttributeType.FullName, actual.AttributeType.FullName, attributeTarget.FullName));
            AssertEquality(expected.Properties, actual.Properties, attributeTarget);
        }

        private static void AssertEquality(
            IEnumerable<CustomAttributeNamedArgument> expected,
            IEnumerable<CustomAttributeNamedArgument> actual,
            MemberReference attributeTarget)
        {
            var sortedExpected = expected.ToList();
            sortedExpected.Sort(Comparisons.CustomAttributeNamedArgumentComparison);
            var sortedActual = actual.ToList();
            sortedActual.Sort(Comparisons.CustomAttributeNamedArgumentComparison);

            for (var i = 0; i < sortedExpected.Count && i < sortedActual.Count; i++)
            {
                AssertEquality(sortedExpected[i], sortedActual[i], attributeTarget);
            }

            if (sortedExpected.Count > sortedActual.Count)
            {
                Assert.Fail("Did not find expected attribute named argument [{0}][{1}] for target {2} of mixed assembly", sortedExpected[sortedActual.Count].Name, sortedExpected[sortedActual.Count].Argument, attributeTarget.FullName);
            }

            if (sortedExpected.Count < sortedActual.Count)
            {
                Assert.Fail("Found extra attribute named argument [{0}][{1}] in type {2} of mixed assembly", sortedActual[sortedExpected.Count].Name, sortedActual[sortedExpected.Count].Argument, attributeTarget.FullName);
            }
        }

        private static void AssertEquality(
            CustomAttributeNamedArgument expected,
            CustomAttributeNamedArgument actual,
            MemberReference attributeTarget)
        {
            Assert.AreEqual(
                expected.Name,
                actual.Name,
                string.Format("Looking for custom attribute named argument with name {0} but found {1} for target {2} of mixed assembly", expected.Name, actual.Name, attributeTarget.FullName));

            AssertEquality(expected.Argument.Type.Resolve(), actual.Argument.Type.Resolve());
            if (expected.Argument.Value != null && actual.Argument.Value != null)
            {
                // this probably needs a better test, but for now this is it
                Assert.AreEqual(
                    expected.Argument.Value,
                    actual.Argument.Value,
                    string.Format(
                    "Looking for custom attribute named argument [{0}][{1}] but found [{2}][{3}] for target {4} of mixed assembly",
                    expected.Name,
                    expected.Argument.Value.ToString(),
                    actual.Name,
                    actual.Argument.Value.ToString(),
                    attributeTarget.FullName));
            }
            else
            {
                if (expected.Argument.Value == null && actual.Argument.Value != null)
                {
                    string.Format(
                        "Looking for custom attribute named argument [{0}] with null value but found [{1}][{2}] for target {3} of mixed assembly",
                        expected.Name,
                        actual.Name,
                        actual.Argument.Value.ToString());
                }
                if (expected.Argument.Value != null && actual.Argument.Value == null)
                {
                    string.Format(
                        "Looking for custom attribute named argument [{0}][{1}] value but found [{2}] with null value for target {3} of mixed assembly",
                        expected.Name,
                        expected.Argument.Value.ToString(),
                        actual.Name);
                }
            }
        }

        private static void AssertEquality(IEnumerable<ParameterDefinition> expected, IEnumerable<ParameterDefinition> actual, MemberReference itemWithParameters)
        {
            var expectedList = expected.ToList();
            var actualList = actual.ToList();

            for (var i = 0; i < expectedList.Count && i < actualList.Count; i++)
            {
                AssertEquality(expectedList[i], actualList[i], itemWithParameters);
            }

            if (expectedList.Count > actualList.Count)
            {
                Assert.Fail("Did not find expected parameter {0} {1} for item {2} of mixed assembly", expectedList[actualList.Count].ParameterType.Name, expectedList[actualList.Count].Name, itemWithParameters.FullName);
            }

            if (expectedList.Count < actualList.Count)
            {
                Assert.Fail("Found extra parameter {0} {1} for item {2} of mixed assembly", actualList[expectedList.Count].ParameterType.Name, actualList[expectedList.Count].Name, itemWithParameters.FullName);
            }
        }

        private static void AssertEquality(ParameterDefinition expected, ParameterDefinition actual, MemberReference itemWithParameters)
        {
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.ParameterType.FullName, actual.ParameterType.FullName);

            Assert.AreEqual(expected.Attributes, actual.Attributes);
            Assert.AreEqual(expected.Index, actual.Index);
            Assert.AreEqual(expected.IsIn, actual.IsIn);
            Assert.AreEqual(expected.IsLcid, actual.IsLcid);
            Assert.AreEqual(expected.IsOptional, actual.IsOptional);
            Assert.AreEqual(expected.IsOut, actual.IsOut);
            Assert.AreEqual(expected.IsReturnValue, actual.IsReturnValue);
            Assert.AreEqual(expected.Sequence, actual.Sequence);

            AssertEquality(expected.CustomAttributes, actual.CustomAttributes, itemWithParameters);
        }

        private static void AssertEquality(IEnumerable<GenericParameter> expected, IEnumerable<GenericParameter> actual, MemberReference itemWithParameters)
        {
            if (expected.Any() || actual.Any()) { throw new NotImplementedException("Implement this when it becomes necessary"); }
        }

        private static void AssertEquality(IEnumerable<EventDefinition> expected, IEnumerable<EventDefinition> actual)
        {
            if (expected.Any() || actual.Any()) { throw new NotImplementedException("Implement this when it becomes necessary"); }
        }

        private static void AssertEquality(IEnumerable<SecurityDeclaration> expected, IEnumerable<SecurityDeclaration> actual)
        {
            if (expected.Any() || actual.Any()) { throw new NotImplementedException("Implement this when it becomes necessary"); }
        }

        private static void AssertEquality(object expected, object actual)
        {
            // this will almost certainly need to become more clever if this test is going to be truly dependable
            if (expected == null && actual == null) { return; }
            Assert.AreEqual(expected == null, actual == null);
            Assert.AreEqual(expected.GetType().FullName, actual.GetType().FullName);
            Assert.AreEqual(expected.ToString(), actual.ToString());
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
