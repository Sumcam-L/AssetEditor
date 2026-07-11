using System;
using System.Reflection.Emit;

namespace Firaxis.TypeGeneration;

public class EnumGenerator
{
	public EnumBuilder Builder { get; private set; }

	public Type CreateType()
	{
		return Builder.CreateType();
	}

	public void AddLiteral(string sName, object value)
	{
		Builder.DefineLiteral(sName, value);
	}

	public EnumGenerator(string sTypeName)
	{
		CreateBuilder(AssemblyGenerator.DefaultGenerator.ModuleBuilder, sTypeName, typeof(int), TypeGeneration.Visibility.Public);
	}

	public EnumGenerator(string sTypeName, TypeGeneration.Visibility visibility)
	{
		CreateBuilder(AssemblyGenerator.DefaultGenerator.ModuleBuilder, sTypeName, typeof(int), visibility);
	}

	public EnumGenerator(AssemblyGenerator assembly, string sTypeName)
	{
		CreateBuilder(assembly.ModuleBuilder, sTypeName, typeof(int), TypeGeneration.Visibility.Public);
	}

	public EnumGenerator(AssemblyGenerator assembly, string sTypeName, TypeGeneration.Visibility visibility)
	{
		CreateBuilder(assembly.ModuleBuilder, sTypeName, typeof(int), visibility);
	}

	private void CreateBuilder(ModuleBuilder module, string sTypeName, Type underlyingType, TypeGeneration.Visibility visibility)
	{
		Builder = module.DefineEnum(sTypeName, TypeGeneration.GetTypeVisibilityAttribute(visibility), underlyingType);
	}
}
