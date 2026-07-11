namespace SharpDX.Win32;

public class PropertyBagKey<T1, T2>
{
	public string Name { get; private set; }

	public PropertyBagKey(string name)
	{
		Name = name;
	}
}
