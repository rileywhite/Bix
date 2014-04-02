using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.CecilMixer.Core
{
    [ContractClass(typeof(IRootImportProviderContract))]
    internal interface IRootImportProvider
    {
        TItem DynamicRootImport<TItem>(TItem item);

        TypeDefinition RootImport(TypeReference type);

        FieldDefinition RootImport(FieldReference field);

        PropertyDefinition RootImport(PropertyReference property);

        MethodDefinition RootImport(MethodReference method);

        EventDefinition RootImport(EventReference @event);
    }
}
