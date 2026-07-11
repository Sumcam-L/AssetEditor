namespace IronPython.Runtime.Types;

internal sealed class CachedGetIdIntExtensionMethod : CachedGetKey
{
	private readonly int _id;

	public CachedGetIdIntExtensionMethod(string name, int id)
		: base(name)
	{
		_id = id;
	}

	public override int GetHashCode()
	{
		return Name.GetHashCode() ^ _id;
	}

	public override bool Equals(CachedGetKey other)
	{
		if (!(other is CachedGetIdIntExtensionMethod cachedGetIdIntExtensionMethod))
		{
			return false;
		}
		if (cachedGetIdIntExtensionMethod._id == _id)
		{
			return cachedGetIdIntExtensionMethod.Name == Name;
		}
		return false;
	}
}
