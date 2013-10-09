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
using System;

namespace Bix.Mixer.CecilMixer
{
    internal class EncapsulateMixer
    {
        public void AddEncapsulation(string assemblyPath, string classFullName)
        {
            var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
            var type = assembly.MainModule.GetType(classFullName);
            var mixAssembly = AssemblyDefinition.ReadAssembly(typeof(IEncapsulates).Assembly.Location);
            type.Interfaces.Add(assembly.MainModule.Import(mixAssembly.MainModule.GetType(typeof(IEncapsulates).FullName)));
            assembly.Write(assemblyPath);
        }
    }
}
