using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.CecilMixer.Core
{
    [ContractClassFor(typeof(IRootImportProvider))]
    internal abstract class IRootImportProviderContract : IRootImportProvider
    {
        public TItem DynamicRootImport<TItem>(TItem item)
        {
            Contract.Ensures(item == null || Contract.Result<TItem>() != null);
            throw new NotSupportedException();
        }

        public TypeDefinition RootImport(TypeReference type)
        {
            Contract.Ensures(type == null || Contract.Result<TypeDefinition>() != null);
            throw new NotSupportedException();
        }

        public FieldDefinition RootImport(FieldReference field)
        {
            Contract.Ensures(field == null || Contract.Result<FieldDefinition>() != null);
            throw new NotSupportedException();
        }

        public PropertyDefinition RootImport(PropertyReference property)
        {
            Contract.Ensures(property == null || Contract.Result<PropertyDefinition>() != null);
            throw new NotSupportedException();
        }

        public MethodDefinition RootImport(MethodReference method)
        {
            Contract.Ensures(method == null || Contract.Result<MethodDefinition>() != null);
            throw new NotSupportedException();
        }

        public EventDefinition RootImport(EventReference @event)
        {
            Contract.Ensures(@event == null || Contract.Result<EventDefinition>() != null);
            throw new NotSupportedException();
        }
    }
}
