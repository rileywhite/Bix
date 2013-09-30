using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mix.Validate
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class AutoRegisterValidateRulesAttribute : Attribute
    {
        public AutoRegisterValidateRulesAttribute(Type validationRuleProviderType)
        {
            Contract.Requires(validationRuleProviderType != null);
            Contract.Requires(typeof(IValidateRuleSource).IsAssignableFrom(validationRuleProviderType));
            this.ValidationRuleProviderType = validationRuleProviderType;
        }

        public Type ValidationRuleProviderType { get; private set; }
    }
}
