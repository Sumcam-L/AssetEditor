using System;

namespace IronPython;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = false)]
public sealed class DictionaryTypeInfoAttribute : Attribute
{
	private readonly Type _keyType;

	private readonly Type _valueType;

	public Type KeyType => _keyType;

	public Type ValueType => _valueType;

	public DictionaryTypeInfoAttribute(Type keyType, Type valueType)
	{
		_keyType = keyType;
		_valueType = valueType;
	}
}
