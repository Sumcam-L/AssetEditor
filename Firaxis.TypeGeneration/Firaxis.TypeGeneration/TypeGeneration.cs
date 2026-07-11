using System.Reflection;

namespace Firaxis.TypeGeneration;

public static class TypeGeneration
{
	public enum Visibility
	{
		Public,
		Protected,
		Private,
		Internal,
		ProtectedInternal
	}

	public static TypeAttributes GetTypeVisibilityAttribute(Visibility accessModifier)
	{
		return accessModifier switch
		{
			Visibility.Public => TypeAttributes.Public, 
			Visibility.Protected => TypeAttributes.NestedFamily, 
			Visibility.Private => TypeAttributes.NestedPrivate, 
			Visibility.Internal => TypeAttributes.NestedAssembly, 
			Visibility.ProtectedInternal => TypeAttributes.NestedFamANDAssem, 
			_ => TypeAttributes.Public, 
		};
	}

	public static FieldAttributes GetFieldVisibilityAttribute(Visibility accessModifier)
	{
		return accessModifier switch
		{
			Visibility.Public => FieldAttributes.Public, 
			Visibility.Protected => FieldAttributes.Family, 
			Visibility.Private => FieldAttributes.Private, 
			Visibility.Internal => FieldAttributes.Assembly, 
			Visibility.ProtectedInternal => FieldAttributes.FamANDAssem, 
			_ => FieldAttributes.Public, 
		};
	}

	public static MethodAttributes GetMethodVisibilityAttribute(Visibility accessModifier)
	{
		return accessModifier switch
		{
			Visibility.Public => MethodAttributes.Public, 
			Visibility.Protected => MethodAttributes.Family, 
			Visibility.Private => MethodAttributes.Private, 
			Visibility.Internal => MethodAttributes.Assembly, 
			Visibility.ProtectedInternal => MethodAttributes.FamANDAssem, 
			_ => MethodAttributes.Public, 
		};
	}
}
