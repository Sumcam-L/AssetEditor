using System;
using System.Collections.ObjectModel;

namespace IronPython.Runtime;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = false)]
public sealed class SequenceTypeInfoAttribute : Attribute
{
	private readonly ReadOnlyCollection<Type> _types;

	public ReadOnlyCollection<Type> Types => _types;

	public SequenceTypeInfoAttribute(params Type[] types)
	{
		_types = new ReadOnlyCollection<Type>(types);
	}
}
