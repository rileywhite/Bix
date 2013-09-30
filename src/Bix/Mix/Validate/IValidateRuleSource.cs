using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mix.Validate
{
    public interface IValidateRuleSource
    {
        IValidateRule[] GetValidateRules();
    }
}
