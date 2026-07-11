using System;
using System.Reflection;
using System.Reflection.Emit;
using Firaxis.Utility;

namespace Firaxis.TypeGeneration;

public class AssemblyGenerator
{
	public const string DefaultAssemblyName = "DynamicAssembly";

	public static AssemblyGenerator DefaultGenerator => Context.EnsureCreated<AssemblyGenerator>();

	public ModuleBuilder ModuleBuilder { get; private set; }

	public AssemblyBuilder AssemblyBuilder { get; private set; }

	public AssemblyGenerator()
	{
		CreateBuilders("DynamicAssembly", AssemblyBuilderAccess.Run);
	}

	public AssemblyGenerator(string sAssemblyName)
	{
		CreateBuilders(sAssemblyName, AssemblyBuilderAccess.Run);
	}

	public AssemblyGenerator(string sAssemblyName, AssemblyBuilderAccess access)
	{
		CreateBuilders(sAssemblyName, access);
	}

	public TypeGenerator DefineType(string sTypeName)
	{
		return new TypeGenerator(this, sTypeName, TypeAttributes.Public);
	}

	public TypeGenerator DefineType(string sTypeName, TypeAttributes attr)
	{
		return new TypeGenerator(this, sTypeName, attr);
	}

	public TypeGenerator DefineType(string sTypeName, TypeAttributes attr, Type parent)
	{
		return new TypeGenerator(this, sTypeName, attr, parent, null);
	}

	public TypeGenerator DefineType(string sTypeName, TypeAttributes attr, Type parent, Type[] interfaces)
	{
		return new TypeGenerator(this, sTypeName, attr, parent, interfaces);
	}

	public EnumGenerator DefineEnum(string sTypeName)
	{
		return new EnumGenerator(this, sTypeName);
	}

	public EnumGenerator DefineEnum(string sTypeName, TypeGeneration.Visibility visibility)
	{
		return new EnumGenerator(this, sTypeName, visibility);
	}

	private void CreateBuilders(string sAssemblyName, AssemblyBuilderAccess access)
	{
		AssemblyName name = new AssemblyName(sAssemblyName);
		AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, access);
		if (access == AssemblyBuilderAccess.RunAndSave || access == AssemblyBuilderAccess.Save)
		{
			ModuleBuilder = AssemblyBuilder.DefineDynamicModule(sAssemblyName, sAssemblyName + ".dll");
		}
		else
		{
			ModuleBuilder = AssemblyBuilder.DefineDynamicModule(sAssemblyName);
		}
	}
}
