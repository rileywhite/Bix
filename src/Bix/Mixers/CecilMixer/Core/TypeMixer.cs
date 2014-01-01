using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class TypeMixer : MemberMixerBase<TypeDefinition, TypeWithRespectToModule>
    {
        public TypeMixer(TypeDefinition target, TypeWithRespectToModule source)
            : base(target, source) { }

        public override void Mix()
        {
            // TODO figure out how to redirect member references within the source to member references within the target
            var typeMixers = new List<TypeMixer>();
            foreach (var typeWithRespectToModule in from type in this.Source.Member.GetNestedTypes()
                                                     select new TypeWithRespectToModule(type, this.Target.Module))
            {
                //var fieldDefinition = this.Target.AddType(
                //    typeWithRespectToModule.Member.Name,
                //    this.Target,
                //    typeWithRespectToModule.CecilMemberDefinition.Attributes);
                //fieldMixers.Add(new FieldMixer(fieldDefinition, fieldWithRespectToModule));
            }

            var fieldMixers = new List<FieldMixer>();
            foreach (var fieldWithRespectToModule in from field in this.Source.Member.GetFields()
                                                     select new FieldWithRespectToModule(field, this.Target.Module))
            {
                var fieldDefinition = this.Target.AddField(
                    fieldWithRespectToModule.Member.Name,
                    this.Target,
                    fieldWithRespectToModule.CecilMemberDefinition.Attributes);
                fieldMixers.Add(new FieldMixer(fieldDefinition, fieldWithRespectToModule));
            }

            throw new NotImplementedException();
        }
    }
}
