using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.CecilMixer.Core
{
    internal class MethodMixer : MemberMixerBase<MethodInfo, MethodDefinition, MethodWithRespectToModule>
    {
        public MethodMixer(MethodDefinition target, MethodWithRespectToModule source)
            : base(target, source)
        {
            Contract.Requires(target != null);
            Contract.Requires(source != null);
        }


        public override void Mix()
        {
            Contract.Assert(this.Target.DeclaringType != null);
            Contract.Assert(this.Target.Name == this.Source.MemberDefinition.Name);

            this.Target.Attributes = this.Source.MemberDefinition.Attributes;
            this.Target.CallingConvention = this.Source.MemberDefinition.CallingConvention;
            this.Target.ExplicitThis = this.Source.MemberDefinition.ExplicitThis;
            this.Target.HasSecurity = this.Source.MemberDefinition.HasSecurity;
            this.Target.HasThis = this.Source.MemberDefinition.HasThis;
            this.Target.ImplAttributes = this.Source.MemberDefinition.ImplAttributes;
            this.Target.IsAbstract = this.Source.MemberDefinition.IsAbstract;
            this.Target.IsAddOn = this.Source.MemberDefinition.IsAddOn;
            this.Target.IsAssembly = this.Source.MemberDefinition.IsAssembly;
            this.Target.IsCheckAccessOnOverride = this.Source.MemberDefinition.IsCheckAccessOnOverride;
            this.Target.IsCompilerControlled = this.Source.MemberDefinition.IsCompilerControlled;
            this.Target.IsFamily = this.Source.MemberDefinition.IsFamily;
            this.Target.IsFamilyAndAssembly = this.Source.MemberDefinition.IsFamilyAndAssembly;
            this.Target.IsFamilyOrAssembly = this.Source.MemberDefinition.IsFamilyOrAssembly;
            this.Target.IsFinal = this.Source.MemberDefinition.IsFinal;
            this.Target.IsFire = this.Source.MemberDefinition.IsFire;
            this.Target.IsForwardRef = this.Source.MemberDefinition.IsForwardRef;
            this.Target.IsGetter = this.Source.MemberDefinition.IsGetter;
            this.Target.IsHideBySig = this.Source.MemberDefinition.IsHideBySig;
            this.Target.IsIL = this.Source.MemberDefinition.IsIL;
            this.Target.IsInternalCall = this.Source.MemberDefinition.IsInternalCall;
            this.Target.IsManaged = this.Source.MemberDefinition.IsManaged;
            this.Target.IsNative = this.Source.MemberDefinition.IsNative;
            this.Target.IsNewSlot = this.Source.MemberDefinition.IsNewSlot;
            this.Target.IsOther = this.Source.MemberDefinition.IsOther;
            this.Target.IsPInvokeImpl = this.Source.MemberDefinition.IsPInvokeImpl;
            this.Target.IsPreserveSig = this.Source.MemberDefinition.IsPreserveSig;
            this.Target.IsPrivate = this.Source.MemberDefinition.IsPrivate;
            this.Target.IsPublic = this.Source.MemberDefinition.IsPublic;
            this.Target.IsRemoveOn = this.Source.MemberDefinition.IsRemoveOn;
            this.Target.IsReuseSlot = this.Source.MemberDefinition.IsReuseSlot;
            this.Target.IsRuntime = this.Source.MemberDefinition.IsRuntime;
            this.Target.IsRuntimeSpecialName = this.Source.MemberDefinition.IsRuntimeSpecialName;
            this.Target.IsSetter = this.Source.MemberDefinition.IsSetter;
            this.Target.IsSpecialName = this.Source.MemberDefinition.IsSpecialName;
            this.Target.IsStatic = this.Source.MemberDefinition.IsStatic;
            this.Target.IsSynchronized = this.Source.MemberDefinition.IsSynchronized;
            this.Target.IsUnmanaged = this.Source.MemberDefinition.IsUnmanaged;
            this.Target.IsUnmanagedExport = this.Source.MemberDefinition.IsUnmanagedExport;
            this.Target.IsVirtual = this.Source.MemberDefinition.IsVirtual;
            this.Target.NoInlining = this.Source.MemberDefinition.NoInlining;
            this.Target.NoOptimization = this.Source.MemberDefinition.NoOptimization;
            this.Target.SemanticsAttributes = this.Source.MemberDefinition.SemanticsAttributes;

            // TODO look more closely
            this.Target.MetadataToken = this.Source.MemberDefinition.MetadataToken;
            if (this.Source.MemberDefinition.PInvokeInfo != null)
            {
                this.Target.PInvokeInfo = new PInvokeInfo(
                    this.Source.MemberDefinition.PInvokeInfo.Attributes,
                    this.Source.MemberDefinition.PInvokeInfo.EntryPoint,
                    this.Source.MemberDefinition.PInvokeInfo.Module);
            }
            //this.Target.MethodReturnType = new MethodReturnType(this.Target)
            //{
            //    Attributes = this.Source.MemberDefinition.MethodReturnType.Attributes,
            //    Constant = this.Source.MemberDefinition.MethodReturnType.Constant,
            //    // CustomAttributes
            //    HasConstant = this.Source.MemberDefinition.MethodReturnType.HasConstant,
            //    HasDefault = this.Source.MemberDefinition.MethodReturnType.HasDefault,
            //    HasFieldMarshal = this.Source.MemberDefinition.MethodReturnType.HasFieldMarshal,
            //    MarshalInfo = this.Source.MemberDefinition.MethodReturnType.MarshalInfo,
            //    MetadataToken = this.Source.MemberDefinition.MethodReturnType.MetadataToken,
            //    ReturnType = this.Source.ReferencingModule.Import(this.Source.MemberDefinition.MethodReturnType.ReturnType)
            //};
            this.Target.ReturnType = this.Source.ReferencingModule.Import(this.Source.MemberDefinition.ReturnType);

            foreach (var parameter in this.Source.MemberInfo.GetParameters().ToParameterDefinitionsForModule(this.Source.ReferencingModule))
            {
                // TODO process parameter type in case they are mixed types
                this.Target.Parameters.Add(parameter);
            }

            Contract.Assert(this.Target.Parameters.Count == this.Source.MemberDefinition.Parameters.Count);

            var parameterOperandReplacementMap = new Dictionary<ParameterDefinition, ParameterDefinition>(this.Source.MemberDefinition.Parameters.Count);
            for (int i = 0; i < this.Source.MemberDefinition.Parameters.Count; i++)
            {
                // verifying same name should be sufficient since name must be unique within a parameter list
                Contract.Assert(this.Target.Parameters[i].Name == this.Source.MemberDefinition.Parameters[i].Name);

                parameterOperandReplacementMap.Add(this.Source.MemberDefinition.Parameters[i], this.Target.Parameters[i]);
            }

            if (this.Source.MemberDefinition.HasBody)
            {
                this.CloneBody(this.Source.MemberDefinition.Body, this.Target.Body, parameterOperandReplacementMap);
            }

            //this.Target.CustomAttributes = this.Source.MemberDefinition;
            //this.Target.GenericParameters = this.Source.MemberDefinition;
            //this.Target.SecurityDeclarations = this.Source.MemberDefinition;

            this.IsMixed = true;
            Contract.Assert(this.Target.SignatureEquals(this.Source.MemberDefinition));
        }

        private void CloneBody(
            Mono.Cecil.Cil.MethodBody sourceBody,
            Mono.Cecil.Cil.MethodBody targetBody,
            Dictionary<ParameterDefinition, ParameterDefinition> parameterOperandReplacementMap)
        {
            Contract.Requires(sourceBody != null);
            Contract.Requires(targetBody != null);
            Contract.Requires(parameterOperandReplacementMap != null);

            targetBody.InitLocals = sourceBody.InitLocals;

            // TODO not sure about this
            targetBody.LocalVarToken = new MetadataToken(
                sourceBody.LocalVarToken.TokenType,
                sourceBody.LocalVarToken.RID);

            targetBody.MaxStackSize = sourceBody.MaxStackSize;

            // TODO this one may be tough to get right
            targetBody.Scope = sourceBody.Scope;

            var variableOperandReplacementMap = new Dictionary<VariableDefinition, VariableDefinition>(sourceBody.Variables.Count);
            foreach (var sourceVariable in sourceBody.Variables)
            {
                var targetVariable = new VariableDefinition(
                    sourceVariable.Name,
                    // TODO may need to replace with mixed type
                    this.Source.ReferencingModule.Import(sourceVariable.VariableType));

                variableOperandReplacementMap.Add(sourceVariable, targetVariable);

                targetBody.Variables.Add(targetVariable);
            }

            var instructionOperandReplacementMap = new Dictionary<Instruction, Instruction>(sourceBody.Instructions.Count);
            var ilProcessor = targetBody.GetILProcessor();
            foreach (var sourceInstruction in sourceBody.Instructions)
            {
                Instruction targetInstruction;
                if (sourceInstruction.Operand == null)
                {
                    targetInstruction = ilProcessor.Create(sourceInstruction.OpCode);
                }
                else
                {
                    targetInstruction = this.CreateInstructionWithOperand(ilProcessor, sourceInstruction.OpCode, (dynamic)sourceInstruction.Operand);
                }
                targetInstruction.Offset = sourceInstruction.Offset;

                ilProcessor.Append(targetInstruction);
                instructionOperandReplacementMap.Add(sourceInstruction, targetInstruction);
            }

            foreach (var targetInstruction in targetBody.Instructions)
            {
                if (TryReplaceParameterOperand(parameterOperandReplacementMap, targetInstruction)) { continue; }
                if (TryReplaceThisReferenceOperand(sourceBody.ThisParameter, targetBody.ThisParameter, targetInstruction)) { continue; }
                if (TryReplaceVariableOperand(variableOperandReplacementMap, targetInstruction)) { continue; }
                if (TryReplaceInstructionOperand(instructionOperandReplacementMap, targetInstruction)) { continue; }
                if (TryReplaceInstructionsOperand(instructionOperandReplacementMap, targetInstruction)) { continue; }
            }
        }

        private bool TryReplaceParameterOperand(Dictionary<ParameterDefinition, ParameterDefinition> parameterOperandReplacementMap, Instruction targetInstruction)
        {
            Contract.Requires(parameterOperandReplacementMap != null);
            Contract.Requires(targetInstruction != null);

            var parameterOperand = targetInstruction.Operand as ParameterDefinition;
            if (parameterOperand != null)
            {
                ParameterDefinition replacementParameterOperand;
                if (parameterOperandReplacementMap.TryGetValue(parameterOperand, out replacementParameterOperand))
                {
                    targetInstruction.Operand = replacementParameterOperand;
                }
                return true;
            }

            return false;
        }

        private bool TryReplaceThisReferenceOperand(ParameterDefinition sourceThis, ParameterDefinition targetThis, Instruction targetInstruction)
        {
            Contract.Requires(targetInstruction != null);
            if (targetInstruction.Operand == sourceThis)
            {
                targetInstruction.Operand = targetThis;
                return true;
            }
            else { return false; }
        }

        private bool TryReplaceVariableOperand(Dictionary<VariableDefinition, VariableDefinition> variableOperandReplacementMap, Instruction targetInstruction)
        {
            Contract.Requires(variableOperandReplacementMap != null);
            Contract.Requires(targetInstruction != null);

            var variableOperand = targetInstruction.Operand as VariableDefinition;
            if (variableOperand != null)
            {
                VariableDefinition replacementVariableOperand;
                if (variableOperandReplacementMap.TryGetValue(variableOperand, out replacementVariableOperand))
                {
                    targetInstruction.Operand = replacementVariableOperand;
                }
                return true;
            }

            return false;
        }

        private bool TryReplaceInstructionOperand(Dictionary<Instruction, Instruction> instructionOperandReplacementMap, Instruction targetInstruction)
        {
            Contract.Requires(instructionOperandReplacementMap != null);
            Contract.Requires(targetInstruction != null);

            var instructionOperand = targetInstruction.Operand as Instruction;
            if (instructionOperand != null)
            {
                Instruction replacementInstructionOperand;
                if (instructionOperandReplacementMap.TryGetValue(instructionOperand, out replacementInstructionOperand))
                {
                    targetInstruction.Operand = replacementInstructionOperand;
                }
                return true;
            }

            return false;
        }

        private bool TryReplaceInstructionsOperand(Dictionary<Instruction, Instruction> instructionOperandReplacementMap, Instruction targetInstruction)
        {
            Contract.Requires(instructionOperandReplacementMap != null);
            Contract.Requires(targetInstruction != null);

            var instructionsOperand = targetInstruction.Operand as Instruction[];
            if (instructionsOperand != null)
            {
                for (int i = 0; i < instructionsOperand.Length; i++)
                {
                    Instruction replacementInstructionOperand;
                    if (instructionOperandReplacementMap.TryGetValue(instructionsOperand[i], out replacementInstructionOperand))
                    {
                        instructionsOperand[i] = replacementInstructionOperand;
                    }
                }
                return true;
            }

            return false;
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, object unsupportedOperand)
        {
            if (unsupportedOperand == null) { return ilProcessor.Create(opCode); }

            throw new NotSupportedException(
                string.Format("Unsupported operand of type in instruction to be cloned: {0}", unsupportedOperand.GetType().FullName));
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, byte value)
        {
            return ilProcessor.Create(opCode, value);
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, CallSite site)
        {
            throw new NotImplementedException();
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, double value)
        {
            return ilProcessor.Create(opCode, value);
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, FieldReference field)
        {
            // TODO replace field reference with mixed version of field reference if needed
            return ilProcessor.Create(opCode, this.Source.ReferencingModule.Import(field));
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, float value)
        {
            return ilProcessor.Create(opCode, value);
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, Instruction target)
        {
            return ilProcessor.Create(opCode, target);
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, Instruction[] targets)
        {
            return ilProcessor.Create(opCode, targets);
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, int value)
        {
            return ilProcessor.Create(opCode, value);
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, long value)
        {
            return ilProcessor.Create(opCode, value);
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, MethodReference method)
        {
            // TODO replace method reference with mixed version of method reference if needed
            return ilProcessor.Create(opCode, this.Source.ReferencingModule.Import(method));
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, ParameterDefinition parameter)
        {
            return ilProcessor.Create(opCode, parameter);
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, sbyte value)
        {
            return ilProcessor.Create(opCode, value);
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, string value)
        {
            return ilProcessor.Create(opCode, value);
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, TypeReference type)
        {
            // TODO replace type reference with mixed version of type reference if needed
            return ilProcessor.Create(opCode, this.Source.ReferencingModule.Import(type));
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, VariableDefinition variable)
        {
            return ilProcessor.Create(opCode, variable);
        }
    }
}
