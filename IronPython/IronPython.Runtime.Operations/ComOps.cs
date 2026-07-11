namespace IronPython.Runtime.Operations;

public static class ComOps
{
	public static string __str__(object self)
	{
		return self.ToString();
	}

	public static string __repr__(object self)
	{
		return $"<{self.ToString()} object at {PythonOps.HexId(self)}>";
	}
}
