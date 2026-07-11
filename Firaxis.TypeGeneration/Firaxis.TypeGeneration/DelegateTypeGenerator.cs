using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Firaxis.TypeGeneration;

public static class DelegateTypeGenerator
{
	public static Type CreateDelegateType(MethodInfo method)
	{
		return CreateDelegateType(AssemblyGenerator.DefaultGenerator, method);
	}

	public static Type CreateDelegateType(string sTypeName, Type returnType, Type[] parameterTypes)
	{
		return CreateDelegateType(AssemblyGenerator.DefaultGenerator, sTypeName, returnType, parameterTypes);
	}

	public static Type CreateDelegateType(AssemblyGenerator assembly, MethodInfo method)
	{
		ParameterInfo[] parameters = method.GetParameters();
		Type[] array = new Type[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			array[i] = parameters[i].ParameterType;
		}
		return CreateDelegateType(assembly, method.Name + "Delegate", method.ReturnType, array);
	}

	public static Type CreateDelegateType(AssemblyGenerator assembly, string sTypeName, Type returnType, Type[] parameterTypes)
	{
		TypeAttributes attr = TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AutoClass;
		TypeBuilder typeBuilder = assembly.ModuleBuilder.DefineType(sTypeName, attr, typeof(MulticastDelegate));
		MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName;
		ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(attributes, CallingConventions.Standard, new Type[2]
		{
			typeof(object),
			typeof(IntPtr)
		});
		constructorBuilder.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
		MethodAttributes attributes2 = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask;
		MethodBuilder methodBuilder = typeBuilder.DefineMethod("Invoke", attributes2, returnType, parameterTypes);
		methodBuilder.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
		return typeBuilder.CreateType();
	}
}
