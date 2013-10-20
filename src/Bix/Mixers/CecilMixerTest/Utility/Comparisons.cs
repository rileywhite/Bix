/***************************************************************************/
// EncapsulateMixer.cs
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

using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bix.Mixers.CecilMixerTest.Utility
{
    internal static class Comparisons
    {
        public static int CompareTo<T>(this IEnumerable<T> left, IEnumerable<T> right, Comparison<T> comparison)
        {
            var sortedLeft = left.ToList();
            sortedLeft.Sort(comparison);
            var sortedRight = right.ToList();
            sortedRight.Sort(comparison);

            for (var i = 0; i < sortedLeft.Count && i < sortedRight.Count; i++)
            {
                var compareResult = comparison.Invoke(sortedLeft[i], sortedRight[i]);
                if (compareResult != 0) { return compareResult; }   // found a difference
            }

            return sortedLeft.Count.CompareTo(sortedRight.Count);   // items are same up to length of shorter list
        }

        private static Comparison<CustomAttribute> customAttributeComparison = new Comparison<CustomAttribute>(
            (left, right) =>
            {
                return left.AttributeType.FullName.CompareTo(right.AttributeType.FullName);
            });
        public static Comparison<CustomAttribute> CustomAttributeComparison
        {
            get { return customAttributeComparison; }
        }

        public static Comparison<CustomAttributeNamedArgument> customAttributeNamedArgumentComparison = new Comparison<CustomAttributeNamedArgument>(
            (left, right) =>
            {
                if (left.Name != right.Name)
                {
                    return left.Name.CompareTo(right.Name);
                }
                if (left.Argument.Type.FullName != right.Argument.Type.FullName)
                {
                    return left.Argument.Type.FullName.CompareTo(right.Argument.Type.FullName);
                }
                if (left.Argument.Value != null && right.Argument.Value != null)
                {
                    // unknown type...be as consistent as possible within appdomain even if arbitrary
                    return left.Argument.Value.GetHashCode().CompareTo(right.Argument.Value.GetHashCode());
                }
                if (left.Argument.Value == null && right.Argument.Value == null) { return 0; }
                if (left.Argument.Value == null) { return -1; }
                return 1;
            });
        public static Comparison<CustomAttributeNamedArgument> CustomAttributeNamedArgumentComparison
        {
            get { return customAttributeNamedArgumentComparison; }
        }

        private static Comparison<ModuleDefinition> moduleComparison = new Comparison<ModuleDefinition>(
            (left, right) =>
            {
                return left.Name.Replace("Reference", "").CompareTo(right.Name.Replace("Reference", ""));
            });
        public static Comparison<ModuleDefinition> ModuleComparison
        {
            get { return moduleComparison; }
        }

        private static Comparison<TypeDefinition> typeComparison = new Comparison<TypeDefinition>(
            (left, right) =>
            {
                return left.FullName.CompareTo(right.FullName);
            });
        public static Comparison<TypeDefinition> TypeComparison
        {
            get { return typeComparison; }
        }

        private static Comparison<MethodDefinition> methodComparison = new Comparison<MethodDefinition>(
            (left, right) =>
            {
                if (left.FullName != right.FullName) { return left.FullName.CompareTo(right.FullName); }
                if (left.Parameters.Count != right.Parameters.Count) { return left.Parameters.Count.CompareTo(right.Parameters.Count); }
                for (var i = 0; i < left.Parameters.Count; i++)
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
        public static Comparison<MethodDefinition> MethodComparison
        {
            get { return methodComparison; }
        }

        private static Comparison<PropertyDefinition> propertyComparison = new Comparison<PropertyDefinition>(
            (left, right) =>
            {
                if (left.DeclaringType.FullName != right.DeclaringType.FullName)
                {
                    return left.DeclaringType.FullName.CompareTo(right.DeclaringType.FullName);
                };

                if (left.PropertyType.FullName != right.PropertyType.FullName)
                {
                    return left.PropertyType.FullName.CompareTo(right.PropertyType.FullName);
                }

                if (left.Name != right.Name) { return left.Name.CompareTo(right.Name); }

                return left.CustomAttributes.CompareTo(right.CustomAttributes, Comparisons.CustomAttributeComparison);
            });
        public static Comparison<PropertyDefinition> PropertyComparison
        {
            get { return propertyComparison; }
        }

        private static Comparison<FieldDefinition> fieldComparison = new Comparison<FieldDefinition>(
            (left, right) =>
            {
                if (left.DeclaringType.FullName != right.DeclaringType.FullName)
                {
                    return left.DeclaringType.FullName.CompareTo(right.DeclaringType.FullName);
                };

                if (left.FieldType.FullName != right.FieldType.FullName)
                {
                    return left.FieldType.FullName.CompareTo(right.FieldType.FullName);
                }

                if (left.Name != right.Name) { return left.Name.CompareTo(right.Name); }

                return left.CustomAttributes.CompareTo(right.CustomAttributes, Comparisons.CustomAttributeComparison);
            });
        public static Comparison<FieldDefinition> FieldComparison
        {
            get { return fieldComparison; }
        }
    }
}
