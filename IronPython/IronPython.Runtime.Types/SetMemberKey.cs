using System;

namespace IronPython.Runtime.Types;

internal class SetMemberKey : IEquatable<SetMemberKey>
{
	public readonly Type Type;

	public readonly string Name;

	public SetMemberKey(Type type, string name)
	{
		Type = type;
		Name = name;
	}

	public bool Equals(SetMemberKey other)
	{
		if (Type == other.Type)
		{
			return Name == other.Name;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SetMemberKey other))
		{
			return false;
		}
		return Equals(other);
	}

	public override int GetHashCode()
	{
		return Type.GetHashCode() ^ Name.GetHashCode();
	}
}
