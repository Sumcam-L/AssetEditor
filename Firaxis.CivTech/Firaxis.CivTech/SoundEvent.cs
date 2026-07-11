using System;
using System.Collections.Generic;

namespace Firaxis.CivTech;

public class SoundEvent : IComparable<SoundEvent>, IEquatable<SoundEvent>
{
	public readonly string Name;

	public readonly AudioScriptType Type;

	public readonly IList<string> Files;

	public SoundEvent(string name, AudioScriptType type)
	{
		Name = name;
		Type = type;
		Files = new List<string>();
	}

	public int CompareTo(SoundEvent other)
	{
		int num = Type.CompareTo(other.Type);
		if (num == 0)
		{
			num = Name.CompareTo(other.Name);
		}
		return num;
	}

	public bool Equals(SoundEvent other)
	{
		return Type.Equals(other.Type) && Name.Equals(other.Name);
	}

	public override bool Equals(object other)
	{
		SoundEvent soundEvent = other as SoundEvent;
		return Type.Equals(soundEvent?.Type) && Name.Equals(soundEvent?.Name);
	}

	public override int GetHashCode()
	{
		return Type.GetHashCode() ^ Name.GetHashCode();
	}
}
