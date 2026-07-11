using System.Collections.Generic;
using System.Text;

namespace IronPython.Compiler.Ast;

public class DottedName : Node
{
	private readonly string[] _names;

	public IList<string> Names => _names;

	public DottedName(string[] names)
	{
		_names = names;
	}

	public string MakeString()
	{
		if (_names.Length == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder(_names[0]);
		for (int i = 1; i < _names.Length; i++)
		{
			stringBuilder.Append('.');
			stringBuilder.Append(_names[i]);
		}
		return stringBuilder.ToString();
	}

	public override void Walk(PythonWalker walker)
	{
		walker.Walk(this);
		walker.PostWalk(this);
	}
}
