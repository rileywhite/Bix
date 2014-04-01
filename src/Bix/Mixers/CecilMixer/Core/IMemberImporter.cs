﻿using Mono.Cecil;
using System;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal interface IMemberImporter<TMemberInfo, TMemberDefinition>
        where TMemberInfo : MemberInfo
        where TMemberDefinition : MemberReference, IMemberDefinition, Mono.Cecil.ICustomAttributeProvider
    {
        TMemberDefinition GetMemberDefinition(TMemberInfo memberInfo);

        ModuleDefinition ReferencingModule { get; set; }
    }
}
