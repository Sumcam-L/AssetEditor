using System;

namespace Firaxis.CivTech.AssetObjects;

public class EntityID : IEquatable<EntityID>, IComparable<EntityID>
{
	public readonly string Name;

	public readonly InstanceType Type;

	private string _toString;

	private string CachedToString
	{
		get
		{
			if (_toString == null)
			{
				string nameFromType = EnumToStringConverter.GetNameFromType(Type);
				_toString = $"{nameFromType}: {Name}";
			}
			return _toString;
		}
	}

	public EntityID(string name, InstanceType type)
	{
		Name = name;
		Type = type;
	}

	public EntityID(IInstanceEntity entity)
		: this(entity?.Name ?? string.Empty, entity?.Type ?? InstanceType.IT_INVALID)
	{
	}

	public EntityID(IObjectValue objectValue)
		: this(objectValue?.GetBoundObjectName() ?? string.Empty, objectValue?.GetBoundObjectType() ?? InstanceType.IT_INVALID)
	{
	}

	public bool Matches(IInstanceEntity entity)
	{
		if (entity == null)
		{
			return false;
		}
		return entity.Name == Name && entity.Type == Type;
	}

	public override string ToString()
	{
		return CachedToString;
	}

	public override int GetHashCode()
	{
		return CachedToString.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		bool flag = obj is EntityID;
		if (flag)
		{
			EntityID other = (EntityID)obj;
			flag = Equals(other);
		}
		return flag;
	}

	public bool Equals(EntityID other)
	{
		return other != null && Name == other.Name && Type == other.Type;
	}

	public int CompareTo(EntityID other)
	{
		return CachedToString.CompareTo(other.CachedToString);
	}
}
