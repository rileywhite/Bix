using Bix.Mix;
using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class InterfaceMixCommand<TInterface, TTemplate>
        where TTemplate : TInterface
    {
        public InterfaceMixCommand(TypeDefinition target)
        {
            Contract.Requires(typeof(TInterface).IsInterface);
            Contract.Requires(target != null);
            Contract.Requires(target.Module != null);
            Contract.Requires(!target.IsValueType);
            Contract.Requires(!target.IsPrimitive);

            Contract.Ensures(this.Target != null);
            Contract.Ensures(!this.Target.IsValueType);
            Contract.Ensures(!this.Target.IsPrimitive);
            Contract.Ensures(this.TargetModule != null);
            Contract.Ensures(this.Source != null);

            this.Source = new TypeWithRespectToModule(typeof(TTemplate), target);
            this.TargetModule = target.Module;
            this.Target = target;
        }

        private TypeWithRespectToModule Source { get; set; }

        private ModuleDefinition TargetModule { get; set; }

        private TypeDefinition target;
        private TypeDefinition Target
        {
            get { return this.target; }
            set
            {
                Contract.Requires(value != null);
                Contract.Ensures(this.Target != null);

                if(value.Interfaces.Any(@interface => @interface.Resolve() == this.Source.MemberDefinition))
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

            this.Target.Interfaces.Add(this.TargetModule.Import(typeof(TInterface)));
            new TypeMixer(this.Target, this.Source).Mix();

            this.IsMixed = true;
        }
    }
}
