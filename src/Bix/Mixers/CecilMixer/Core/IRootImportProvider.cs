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

        TypeReference RootImport(TypeReference type);

        FieldReference RootImport(FieldReference field);

        PropertyReference RootImport(PropertyReference property);

        MethodReference RootImport(MethodReference method);

        EventReference RootImport(EventReference @event);
    }
}
