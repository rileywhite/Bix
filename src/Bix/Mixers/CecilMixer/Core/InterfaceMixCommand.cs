using Bix.Mix;
using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class InterfaceMixCommand<TMixesInterface, TTemplate>
        where TMixesInterface : IMixes
        where TTemplate : TMixesInterface
    {
        public InterfaceMixCommand(TypeDefinition target)
        {
            Contract.Requires(typeof(TMixesInterface).IsInterface);
            Contract.Requires(target != null);
            Contract.Requires(target.Module != null);
            Contract.Requires(!target.IsValueType);
            Contract.Requires(!target.IsPrimitive);

            Contract.Ensures(this.Target != null);
            Contract.Ensures(!this.Target.IsValueType);
            Contract.Ensures(!this.Target.IsPrimitive);
            Contract.Ensures(this.TargetModule != null);
            Contract.Ensures(this.InterfaceType != null);
            Contract.Ensures(this.InterfaceType.Member.IsInterface);
            Contract.Ensures(this.TemplateType != null);

            this.InterfaceType = new TypeWithRespectToModule(typeof(TMixesInterface), target.Module);
            this.TemplateType = new TypeWithRespectToModule(typeof(TTemplate), target.Module);
            this.TargetModule = target.Module;
            this.Target = target;
        }

        private TypeWithRespectToModule InterfaceType { get; set; }

        private TypeWithRespectToModule TemplateType { get; set; }

        private ModuleDefinition TargetModule { get; set; }

        private TypeDefinition target;
        private TypeDefinition Target
        {
            get { return this.target; }
            set
            {
                Contract.Requires(value != null);
                Contract.Ensures(this.Target != null);

                if(value.Interfaces.Any(@interface => @interface.Resolve() == this.TemplateType.CecilMemberDefinition))
                {
                    throw new ArgumentException("Cannot set a target type that already implements the interface to be mixed", "value");
                }

                this.target = value;
            }
        }

        public bool IsMixed { get; set; }

        public void Mix()
        {
            Contract.Requires(!this.IsMixed);
            Contract.Ensures(this.IsMixed);

            this.IsMixed = true;

            this.Target.Interfaces.Add(this.InterfaceType.CecilMemberDefinition);
            new TypeMixer(this.Target, this.TemplateType).Mix();
        }
    }
}
