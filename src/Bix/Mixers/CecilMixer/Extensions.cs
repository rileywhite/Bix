using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using MethodInfo = System.Reflection.MethodInfo;
using ParameterInfo = System.Reflection.ParameterInfo;
using PropertyInfo = System.Reflection.PropertyInfo;
using System.Runtime.CompilerServices;

namespace Bix.Mixers.CecilMixer
{
    internal static class Extensions
    {
        public static FieldDefinition AddField(
            this TypeDefinition target,
            string name,
            TypeReference fieldType,
            FieldAttributes fieldAttributes = FieldAttributes.Private)
        {
            Contract.Assert(target != null);
            Contract.Assert(target.Module != null);
            Contract.Assert(!string.IsNullOrWhiteSpace(name));
            Contract.Assert(fieldType != null);
            Contract.Ensures(Contract.Result<FieldDefinition>() != null);

            var field = new FieldDefinition(name, fieldAttributes, fieldType);
            field.DeclaringType = target;
            target.Fields.Add(field);

            return field;
        }

        public static PropertyDefinition AddAutoProperty(
            this TypeDefinition target,
            string name,
            TypeReference propertyType)
        {
            Contract.Assert(target != null);
            Contract.Assert(target.Module != null);
            Contract.Assert(!string.IsNullOrWhiteSpace(name));
            Contract.Assert(propertyType != null);
            Contract.Ensures(Contract.Result<PropertyDefinition>() != null);

            var field = target.AddField(
                string.Format("<{0}>k__BackingField", name),
                propertyType);
            field.MarkAsCompilerGenerated();

            return target.AddProperty(
                name,
                propertyType,
                getterMethodBuilder: ilProcessor =>
                {
                    ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                    ilProcessor.Append(Instruction.Create(OpCodes.Ldfld, field));
                    ilProcessor.Append(Instruction.Create(OpCodes.Ret));
                },
                setterMethodBuilder: ilProcessor =>
                {
                    ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                    ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_1));
                    ilProcessor.Append(Instruction.Create(OpCodes.Stfld, field));
                    ilProcessor.Append(Instruction.Create(OpCodes.Ret));
                });
        }

        public static PropertyDefinition AddProperty(
            this TypeDefinition target,
            string name,
            TypeReference propertyType,
            Action<ILProcessor> getterMethodBuilder = null,
            MethodAttributes getterMethodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            Action<ILProcessor> setterMethodBuilder = null,
            MethodAttributes setterMethodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig)
        {
            Contract.Assert(target != null);
            Contract.Assert(!string.IsNullOrWhiteSpace(name));
            Contract.Assert(propertyType != null);
            Contract.Assert(getterMethodBuilder != null || setterMethodBuilder != null);
            Contract.Ensures(Contract.Result<PropertyDefinition>() != null);

            MethodDefinition getterMethod;
            if (getterMethodBuilder == null) { getterMethod = null; }
            else
            {
                getterMethod = target.AddMethod(
                    string.Format("get_{0}", name),
                    propertyType,
                    getterMethodBuilder,
                    methodAttributes: getterMethodAttributes);
            }

            MethodDefinition setterMethod;
            if (setterMethodBuilder == null) { setterMethod = null; }
            else
            {
                setterMethod = target.AddMethod(
                    string.Format("set_{0}", name),
                    null,
                    setterMethodBuilder,
                    methodAttributes: setterMethodAttributes);
                setterMethod.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, propertyType));
            }

            return target.AddProperty(name, propertyType, getterMethod, setterMethod);
        }

        public static PropertyDefinition AddProperty(
            this TypeDefinition target,
            string name,
            TypeReference propertyType,
            MethodDefinition getterMethod = null,
            MethodDefinition setterMethod = null,
            PropertyAttributes propertyAttributes = PropertyAttributes.None)
        {
            Contract.Assert(target != null);
            Contract.Assert(target.Module != null);
            Contract.Assert(!string.IsNullOrWhiteSpace(name));
            Contract.Assert(propertyType != null);
            Contract.Assert(getterMethod != null || setterMethod != null);
            Contract.Ensures(Contract.Result<PropertyDefinition>() != null);

            PropertyDefinition property = new PropertyDefinition(name, propertyAttributes, propertyType);
            property.GetMethod = getterMethod;
            property.SetMethod = setterMethod;

            property.DeclaringType = target;
            target.Properties.Add(property);

            return property;
        }

        public static PropertyDefinition ImplementAutoPropertyExplicitly(
            this TypeDefinition target,
            PropertyInfo interfacePropertyInfo)
        {
            Contract.Assert(target != null);
            Contract.Assert(target.Module != null);
            Contract.Assert(interfacePropertyInfo != null);
            Contract.Assert(interfacePropertyInfo.DeclaringType != null);
            Contract.Assert(interfacePropertyInfo.DeclaringType.IsInterface);
            Contract.Ensures(Contract.Result<PropertyDefinition>() != null);

            var propertyType = target.Module.Import(interfacePropertyInfo.PropertyType);

            var field = target.AddField(
                string.Format("<{0}.{1}.{2}>k__BackingField", interfacePropertyInfo.DeclaringType.Namespace, interfacePropertyInfo.DeclaringType.Name, interfacePropertyInfo.Name),
                propertyType);
            field.MarkAsCompilerGenerated();

            Action<ILProcessor> getterMethodBuilder;
            if (interfacePropertyInfo.GetMethod == null) { getterMethodBuilder = null; }
            else
            {
                getterMethodBuilder = ilProcessor =>
                {
                    ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                    ilProcessor.Append(Instruction.Create(OpCodes.Ldfld, field));
                    ilProcessor.Append(Instruction.Create(OpCodes.Stloc_0));
                    Instruction weird = Instruction.Create(OpCodes.Ldloc_0);
                    ilProcessor.Append(Instruction.Create(OpCodes.Br_S, weird));
                    ilProcessor.Append(weird);
                    ilProcessor.Append(Instruction.Create(OpCodes.Ret));
                };
            }

            Action<ILProcessor> setterMethodBuilder;
            if (interfacePropertyInfo.GetMethod == null) { setterMethodBuilder = null; }
            else
            {
                setterMethodBuilder = ilProcessor =>
                {
                    ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                    ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_1));
                    ilProcessor.Append(Instruction.Create(OpCodes.Stfld, field));
                    ilProcessor.Append(Instruction.Create(OpCodes.Ret));
                };
            }

            var property = target.ImplementPropertyExplicitly(
                interfacePropertyInfo,
                getterMethodBuilder,
                setterMethodBuilder);

            if (property.GetMethod != null)
            {
                property.GetMethod.Body.Variables.Add(new VariableDefinition(propertyType));
                property.GetMethod.Body.InitLocals = true;
                property.GetMethod.MarkAsCompilerGenerated();
            }

            if (property.SetMethod != null)
            {
                property.SetMethod.MarkAsCompilerGenerated();
            }

            return property;
        }

        public static PropertyDefinition ImplementPropertyExplicitly(
            this TypeDefinition target,
            PropertyInfo interfacePropertyInfo,
            Action<ILProcessor> getterMethodBuilder = null,
            Action<ILProcessor> setterMethodBuilder = null)
        {
            Contract.Assert(target != null);
            Contract.Assert(target.Module != null);
            Contract.Assert(interfacePropertyInfo != null);
            Contract.Assert(interfacePropertyInfo.DeclaringType != null);
            Contract.Assert(interfacePropertyInfo.DeclaringType.IsInterface);
            Contract.Assert((interfacePropertyInfo.GetMethod == null) == (getterMethodBuilder == null));
            Contract.Assert((interfacePropertyInfo.SetMethod == null) == (setterMethodBuilder == null));
            Contract.Assert(getterMethodBuilder != null || setterMethodBuilder != null);
            Contract.Ensures(Contract.Result<PropertyDefinition>() != null);

            var propertyType = target.Module.Import(interfacePropertyInfo.PropertyType);

            MethodDefinition getterMethod;
            if (getterMethodBuilder == null) { getterMethod = null; }
            else
            {
                getterMethod = target.AddMethod(
                    string.Format("{0}.{1}.{2}", interfacePropertyInfo.DeclaringType.Namespace, interfacePropertyInfo.DeclaringType.Name, interfacePropertyInfo.GetMethod.Name),
                    propertyType,
                    getterMethodBuilder,
                    methodAttributes: MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.SpecialName);
                getterMethod.Overrides.Add(target.Module.Import(interfacePropertyInfo.GetMethod));
            }

            MethodDefinition setterMethod;
            if (setterMethodBuilder == null) { setterMethod = null; }
            else
            {
                setterMethod = target.AddMethod(
                    string.Format("{0}.{1}.{2}", interfacePropertyInfo.DeclaringType.Namespace, interfacePropertyInfo.DeclaringType.Name, interfacePropertyInfo.SetMethod.Name),
                    target.Module.Import(typeof(void)),
                    setterMethodBuilder,
                    methodAttributes: MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.SpecialName);
                setterMethod.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, propertyType));
                setterMethod.Overrides.Add(target.Module.Import(interfacePropertyInfo.SetMethod));
            }

            return target.AddProperty(
                string.Format("{0}.{1}.{2}", interfacePropertyInfo.DeclaringType.Namespace, interfacePropertyInfo.DeclaringType.Name, interfacePropertyInfo.Name),
                propertyType,
                getterMethod,
                setterMethod);
        }

        public static MethodDefinition ImplementMethodExplicitly(
            this TypeDefinition target,
            MethodInfo interfaceMethodInfo,
            Action<ILProcessor> methodBuilder)
        {
            Contract.Assert(target != null);
            Contract.Assert(target.Module != null);
            Contract.Assert(interfaceMethodInfo != null);
            Contract.Assert(interfaceMethodInfo.DeclaringType != null);
            Contract.Assert(interfaceMethodInfo.DeclaringType.IsInterface);
            Contract.Assert(methodBuilder != null);
            Contract.Ensures(Contract.Result<MethodDefinition>() != null);

            var method = target.AddMethod(
                string.Format("{0}.{1}.{2}", interfaceMethodInfo.DeclaringType.Namespace, interfaceMethodInfo.DeclaringType.Name, interfaceMethodInfo.Name),
                target.Module.Import(interfaceMethodInfo.ReturnType),
                methodBuilder,
                interfaceMethodInfo.GetParameters(),
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final);

            method.Overrides.Add(target.Module.Import(interfaceMethodInfo));

            return method;
        }

        public static MethodDefinition AddMethod(
            this TypeDefinition target,
            string name,
            TypeReference returnType,
            Action<ILProcessor> bodyBuilder,
            ParameterInfo[] parameterInfos = null,
            MethodAttributes methodAttributes = MethodAttributes.Public)
        {
            return AddMethod(
                target,
                name,
                returnType,
                bodyBuilder,
                (parameterInfos ?? new ParameterInfo[0]).ToParameterDefinitionsForModule(target.Module),
                methodAttributes);
        }

        public static ParameterDefinition[] ToParameterDefinitionsForModule(this ParameterInfo[] parameters, ModuleDefinition module)
        {
            Contract.Assert(module != null);

            if (parameters == null) { return null; }
            ParameterDefinition[] parameterDefinitions = new ParameterDefinition[parameters.Length];
            for (int i = 0; i < parameters.Length; i++ )
            {
                if (parameters[i] == null) { throw new ArgumentException("Cannot convert null parameter info to a parameter definition", "parameters"); }

                // TODO more work will have to be done here (e.g. for parameters...maybe other stuff?)
                var parameterDefinition = new ParameterDefinition(parameters[i].Name, ParameterAttributes.None, module.Import(parameters[i].ParameterType));

                parameterDefinitions[i] = parameterDefinition;
            }
            return parameterDefinitions;
        }

        public static MethodDefinition AddMethod(
            this TypeDefinition target,
            string name,
            TypeReference returnType,
            Action<ILProcessor> bodyBuilder,
            ParameterDefinition[] parameters,
            MethodAttributes methodAttributes = MethodAttributes.Public)
        {
            Contract.Assert(target != null);
            Contract.Assert(target.Module != null);
            Contract.Assert(!string.IsNullOrWhiteSpace(name));
            Contract.Assert(bodyBuilder != null);
            Contract.Ensures(Contract.Result<MethodDefinition>() != null);

            returnType = returnType ?? target.Module.Import(typeof(void));

            var method = new MethodDefinition(
                name,
                methodAttributes,
                returnType);

            method.DeclaringType = target;

            if (parameters != null) foreach (var parameter in parameters)
                {
                    method.Parameters.Add(parameter);
                }

            bodyBuilder(method.Body.GetILProcessor());

            target.Methods.Add(method);

            return method;
        }

        public static MethodReference ImportConstructor(this ModuleDefinition module, Type target, params Type[] argumentTypes)
        {
            Contract.Assert(module != null);
            Contract.Assert(target != null);
            Contract.Ensures(Contract.Result<MethodReference>() != null);

            argumentTypes = argumentTypes ?? new Type[0];
            Contract.Assert(!argumentTypes.Any(argumentType => argumentType == null));

            var constructor = target.GetConstructor(argumentTypes);
            if(constructor == null)
            {
                var exception = new ArgumentException("No constructor with the provided arguments could be found", "argumentTypes");
                exception.Data.Add("target", target.FullName);
            }

            return module.Import(constructor);
        }

        public static void MarkAsCompilerGenerated<TModuleMember>(this TModuleMember item)
            where TModuleMember : MemberReference, ICustomAttributeProvider
        {
            Contract.Assert(item != null);
            Contract.Assert(item.Module != null);
            Contract.Assert(item.CustomAttributes != null);

            item.CustomAttributes.Add(new CustomAttribute(item.Module.ImportConstructor(typeof(CompilerGeneratedAttribute))));
        }
    }
}
