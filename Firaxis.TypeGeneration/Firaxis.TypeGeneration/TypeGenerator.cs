using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Firaxis.TypeGeneration;

public class TypeGenerator
{
	public TypeBuilder Builder { get; private set; }

	public Type CreateType()
	{
		return Builder.CreateType();
	}

	public void AddProperty(string sName, Type t, TypeGeneration.Visibility visibility)
	{
		AddProperty(sName, t, visibility, visibility);
	}

	public void AddProperty(string sName, Type t, TypeGeneration.Visibility getVisibility, TypeGeneration.Visibility setVisibility)
	{
		FieldBuilder field = Builder.DefineField("_" + sName, t, FieldAttributes.PrivateScope);
		PropertyBuilder propertyBuilder = Builder.DefineProperty(sName, PropertyAttributes.None, t, null);
		MethodAttributes methodAttributes = MethodAttributes.HideBySig | MethodAttributes.SpecialName;
		MethodAttributes methodVisibilityAttribute = TypeGeneration.GetMethodVisibilityAttribute(getVisibility);
		MethodAttributes attributes = methodAttributes | methodVisibilityAttribute;
		MethodBuilder methodBuilder = Builder.DefineMethod("get_" + sName, attributes, t, Type.EmptyTypes);
		ILGenerator iLGenerator = methodBuilder.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldfld, field);
		iLGenerator.Emit(OpCodes.Ret);
		propertyBuilder.SetGetMethod(methodBuilder);
		MethodAttributes methodVisibilityAttribute2 = TypeGeneration.GetMethodVisibilityAttribute(setVisibility);
		MethodAttributes attributes2 = methodAttributes | methodVisibilityAttribute2;
		Type[] parameterTypes = new Type[1] { t };
		MethodBuilder methodBuilder2 = Builder.DefineMethod(sName, attributes2, null, parameterTypes);
		ILGenerator iLGenerator2 = methodBuilder2.GetILGenerator();
		iLGenerator2.Emit(OpCodes.Ldarg_0);
		iLGenerator2.Emit(OpCodes.Ldarg_1);
		iLGenerator2.Emit(OpCodes.Stfld, field);
		iLGenerator2.Emit(OpCodes.Ret);
		propertyBuilder.SetSetMethod(methodBuilder2);
	}

	public TypeGenerator(string sTypeName)
	{
		CreateBuilder(AssemblyGenerator.DefaultGenerator.ModuleBuilder, sTypeName, TypeAttributes.Public);
	}

	public TypeGenerator(string sTypeName, TypeAttributes attr)
	{
		CreateBuilder(AssemblyGenerator.DefaultGenerator.ModuleBuilder, sTypeName, attr);
	}

	public TypeGenerator(string sTypeName, TypeAttributes attr, Type parent)
	{
		CreateBuilder(AssemblyGenerator.DefaultGenerator.ModuleBuilder, sTypeName, attr, parent, null);
	}

	public TypeGenerator(string sTypeName, TypeAttributes attr, Type parent, Type[] interfaces)
	{
		CreateBuilder(AssemblyGenerator.DefaultGenerator.ModuleBuilder, sTypeName, attr, parent, interfaces);
	}

	public TypeGenerator(AssemblyGenerator assembly, string sTypeName, TypeAttributes attr)
	{
		CreateBuilder(assembly.ModuleBuilder, sTypeName, attr);
	}

	public TypeGenerator(AssemblyGenerator assembly, string sTypeName, TypeAttributes attr, Type parent)
	{
		CreateBuilder(assembly.ModuleBuilder, sTypeName, attr, parent, null);
	}

	public TypeGenerator(AssemblyGenerator assembly, string sTypeName, TypeAttributes attr, Type parent, Type[] interfaces)
	{
		CreateBuilder(assembly.ModuleBuilder, sTypeName, attr, parent, interfaces);
	}

	private void CreateBuilder(ModuleBuilder module, string sTypeName, TypeAttributes attr)
	{
		Builder = module.DefineType(sTypeName, attr, null, null);
	}

	private void CreateBuilder(ModuleBuilder module, string sTypeName, TypeAttributes attr, Type parent, Type[] interfaces)
	{
		Builder = module.DefineType(sTypeName, attr, parent, interfaces);
	}
}
