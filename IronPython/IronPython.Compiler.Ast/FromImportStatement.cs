using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace IronPython.Compiler.Ast;

public class FromImportStatement : Statement
{
	private static readonly string[] _star = new string[1] { "*" };

	private readonly ModuleName _root;

	private readonly string[] _names;

	private readonly string[] _asNames;

	private readonly bool _fromFuture;

	private readonly bool _forceAbsolute;

	private PythonVariable[] _variables;

	public static IList<string> Star => _star;

	public DottedName Root => _root;

	public bool IsFromFuture => _fromFuture;

	public IList<string> Names => _names;

	public IList<string> AsNames => _asNames;

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

	public FromImportStatement(ModuleName root, string[] names, string[] asNames, bool fromFuture, bool forceAbsolute)
	{
		_root = root;
		_names = names;
		_asNames = asNames;
		_fromFuture = fromFuture;
		_forceAbsolute = forceAbsolute;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		if (_names == _star)
		{
			return base.GlobalParent.AddDebugInfo(System.Linq.Expressions.Expression.Call(AstMethods.ImportStar, base.Parent.LocalContext, Utils.Constant(_root.MakeString()), Utils.Constant(GetLevel())), base.Span);
		}
		ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression>();
		ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Variable(typeof(object), "module");
		System.Linq.Expressions.Expression[] array = new System.Linq.Expressions.Expression[_names.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Utils.Constant(_names[i]);
		}
		readOnlyCollectionBuilder.Add(base.GlobalParent.AddDebugInfoAndVoid(Node.AssignValue(parameterExpression, LightExceptions.CheckAndThrow(System.Linq.Expressions.Expression.Call(AstMethods.ImportWithNames, base.Parent.LocalContext, Utils.Constant(_root.MakeString()), System.Linq.Expressions.Expression.NewArrayInit(typeof(string), array), Utils.Constant(GetLevel())))), _root.Span));
		for (int j = 0; j < array.Length; j++)
		{
			readOnlyCollectionBuilder.Add(base.GlobalParent.AddDebugInfoAndVoid(Node.AssignValue(base.Parent.GetVariableExpression(_variables[j]), System.Linq.Expressions.Expression.Call(AstMethods.ImportFrom, base.Parent.LocalContext, parameterExpression, array[j])), base.Span));
		}
		readOnlyCollectionBuilder.Add(Utils.Empty());
		return base.GlobalParent.AddDebugInfo(System.Linq.Expressions.Expression.Block(new ParameterExpression[1] { parameterExpression }, readOnlyCollectionBuilder.ToArray()), base.Span);
	}

	private object GetLevel()
	{
		if (_root is RelativeModuleName relativeModuleName)
		{
			return relativeModuleName.DotCount;
		}
		if (_forceAbsolute)
		{
			return 0;
		}
		return -1;
	}

	public override void Walk(PythonWalker walker)
	{
		walker.Walk(this);
		walker.PostWalk(this);
	}
}
