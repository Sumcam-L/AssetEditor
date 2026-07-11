namespace Firaxis.CivTech;

public class ToolHostEventArgs
{
	public readonly string DLLPath;

	public ToolHostEventArgs(string s)
	{
		DLLPath = s;
	}
}
