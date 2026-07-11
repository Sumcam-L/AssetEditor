using System;
using System.Windows.Forms;

namespace Firaxis.Utility;

public class AccelKey
{
	public Keys Keys { get; set; }

	public EventHandler Handler { get; set; }

	public AccelKey(Keys keys, EventHandler handler)
	{
		Keys = keys;
		Handler = handler;
	}

	public override int GetHashCode()
	{
		return Keys.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is AccelKey)
		{
			return Equals((AccelKey)obj);
		}
		return false;
	}

	public bool Equals(AccelKey other)
	{
		return Keys == other.Keys;
	}
}
