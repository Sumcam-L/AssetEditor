using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace IronPython.Compiler.Ast;

public class ImportStatement : Statement
{
	private readonly ModuleName[] _names;

	private readonly string[] _asNames;

	private readonly bool _forceAbsolute;

	private PythonVariable[] _variables;

	internal PythonVariable[] Variables
	{
		get
		{
			return _variables;
		}
		set
		{
			_variables = value;
		}
	}

	public IList<DottedName> Names => _names;

	public IList<string> AsNames => _asNames;

	public ImportStatement(ModuleName[] names, string[] asNames, bool forceAbsolute)
	{
		_names = names;
		_asNames = asNames;
		_forceAbsolute = forceAbsolute;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression>();
		for (int i = 0; i < _names.Length; i++)
		{
			readOnlyCollectionBuilder.Add(base.GlobalParent.AddDebugInfoAndVoid(Node.AssignValue(base.Parent.GetVariableExpression(_variables[i]), LightExceptions.CheckAndThrow(System.Linq.Expressions.Expression.Call((_asNames[i] == null) ? AstMethods.ImportTop : AstMethods.ImportBottom, base.Parent.LocalContext, Utils.Constant(_names[i].MakeString()), Utils.Constant((!_forceAbsolute) ? (-1) : 0)))), _names[i].Span));
		}
		readOnlyCollectionBuilder.Add(Utils.Empty());
		return base.GlobalParent.AddDebugInfo(System.Linq.Expressions.Expression.Block(readOnlyCollectionBuilder.ToReadOnlyCollection()), base.Span);
	}

	public override void Walk(PythonWalker walker)
	{
		walker.Walk(this);
		walker.PostWalk(this);
	}
}
