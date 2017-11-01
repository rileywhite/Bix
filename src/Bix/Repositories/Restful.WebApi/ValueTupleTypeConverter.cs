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

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Bix.Repositories.Restful.WebApi
{
    public class ValueTupleTypeConverter : TypeConverter
    {
        #region Construction

        public ValueTupleTypeConverter(Type targetType)
        {
            this.TargetType = targetType;
        }

        public Type TargetType { get; }

        #endregion

        #region Converting from source

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) { return true; }
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            switch(value)
            {
                case string source when this.OuterParenthesesRegex.IsMatch(source):
                    var sources = this.OuterParenthesesRegex.Match(source).Groups["content"].Captures[0].Value.Split(new[] { ", " }, StringSplitOptions.None);

                    foreach (var constructor in this.TargetType.GetConstructors())
                    {
                        if (this.TryInvokeConstructor(constructor, sources, out object item)) { return item; }
                    }

                    throw new ArgumentException($"No constructor was found matching the given source value: {value}", nameof(value));

                case string source:
                    throw new ArgumentException($"The given value did not match the expected format of a tuple: {value}", nameof(value));

                default:
                    return base.ConvertFrom(context, culture, value);
            }
        }

        private bool TryInvokeConstructor(ConstructorInfo constructor, string[] sources, out object item)
        {
            var parameters = constructor.GetParameters();
            if (parameters.Length != sources.Length)
            {
                item = null;
                return false;
            }

            var parameterConverters = parameters.Select(p => TypeDescriptor.GetConverter(p.ParameterType)).ToArray();
            if (parameterConverters.Any(pc => !pc.CanConvertFrom(typeof(string))))
            {
                item = null;
                return false;
            }

            var arguments = Enumerable
                .Range(0, parameters.Length)
                .Select(i => parameterConverters[i].ConvertFrom(sources[i])).ToArray();

            item = constructor.Invoke(arguments);
            return true;
        }

        #region Source string manipulation

        private Regex OuterParenthesesRegex = new Regex(@"^\((?<content>.+?)\)$");

        #endregion

        #endregion

        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            return base.IsValid(context, value);
        }

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            return base.CreateInstance(context, propertyValues);
        }

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            return base.GetCreateInstanceSupported(context);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return base.GetProperties(context, value, attributes);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return base.GetPropertiesSupported(context);
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return base.GetStandardValues(context);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return base.GetStandardValuesExclusive(context);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return base.GetStandardValuesSupported(context);
        }
    }
}
