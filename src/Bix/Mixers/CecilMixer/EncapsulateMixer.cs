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

using Bix.Mix.Encapsulate;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using System;

namespace Bix.Mixer.CecilMixer
{
    internal class EncapsulateMixer
    {
        public void AddEncapsulation(string assemblyPath, string classFullName)
        {
            var typeModule = ModuleDefinition.ReadModule(assemblyPath);
            var type = typeModule.GetType(classFullName);
            var bixModule = ModuleDefinition.ReadModule(typeof(IEncapsulates).Assembly.Location);

            // add interface to type
            type.Interfaces.Add(typeModule.Import(typeof(IEncapsulates)));

            // create inner DTO
            TypeDefinition dtoType = new TypeDefinition(string.Format("{0}/{1}", type.Namespace, type.Name), "Dto", TypeAttributes.NestedPublic | TypeAttributes.Class, typeModule.Import(typeof(object)));
            type.NestedTypes.Add(dtoType);

            typeModule.Write(assemblyPath, new WriterParameters { SymbolWriterProvider = new PdbWriterProvider() });
        }
    }
}
