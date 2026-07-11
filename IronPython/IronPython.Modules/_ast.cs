using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using IronPython.Compiler;
using IronPython.Compiler.Ast;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

public static class _ast
{
	private class ThrowingErrorSink : ErrorSink
	{
		public new static readonly ThrowingErrorSink Default = new ThrowingErrorSink();

		private ThrowingErrorSink()
		{
		}

		public override void Add(SourceUnit sourceUnit, string message, SourceSpan span, int errorCode, Severity severity)
		{
			if (severity == Severity.Warning)
			{
				PythonOps.SyntaxWarning(message, sourceUnit, span, errorCode);
				return;
			}
			throw PythonOps.SyntaxError(message, sourceUnit, span, errorCode);
		}
	}

	[PythonType]
	public abstract class AST
	{
		private PythonTuple __fields = new PythonTuple();

		private PythonTuple __attributes = new PythonTuple();

		protected int? _lineno;

		protected int? _col_offset;

		public PythonTuple _fields
		{
			get
			{
				return __fields;
			}
			protected set
			{
				__fields = value;
			}
		}

		public PythonTuple _attributes
		{
			get
			{
				return __attributes;
			}
			protected set
			{
				__attributes = value;
			}
		}

		public int lineno
		{
			get
			{
				if (_lineno.HasValue)
				{
					return _lineno.Value;
				}
				throw PythonOps.AttributeErrorForMissingAttribute(PythonTypeOps.GetName(this), "lineno");
			}
			set
			{
				_lineno = value;
			}
		}

		public int col_offset
		{
			get
			{
				if (_col_offset.HasValue)
				{
					return _col_offset.Value;
				}
				throw PythonOps.AttributeErrorForMissingAttribute(PythonTypeOps.GetName(this), "col_offset");
			}
			set
			{
				_col_offset = value;
			}
		}

		public void __setstate__(PythonDictionary state)
		{
			restoreProperties(__attributes, state);
			restoreProperties(__fields, state);
		}

		internal void restoreProperties(IEnumerable<object> names, IDictionary source)
		{
			foreach (object name in names)
			{
				if (name is string)
				{
					try
					{
						string text = (string)name;
						GetType().GetProperty(text).SetValue(this, source[text], null);
					}
					catch (KeyNotFoundException)
					{
					}
				}
			}
		}

		internal void storeProperties(IEnumerable<object> names, IDictionary target)
		{
			foreach (object name in names)
			{
				if (name is string)
				{
					string text = (string)name;
					try
					{
						object value = GetType().GetProperty(text).GetValue(this, null);
						target.Add(text, value);
					}
					catch (TargetInvocationException)
					{
					}
				}
			}
		}

		internal PythonDictionary getstate()
		{
			PythonDictionary pythonDictionary = new PythonDictionary(10);
			storeProperties(__fields, pythonDictionary);
			storeProperties(__attributes, pythonDictionary);
			return pythonDictionary;
		}

		public virtual object __reduce__()
		{
			return PythonTuple.MakeTuple(DynamicHelpers.GetPythonType(this), new PythonTuple(), getstate());
		}

		public virtual object __reduce_ex__(int protocol)
		{
			return __reduce__();
		}

		protected void GetSourceLocation(Node node)
		{
			_lineno = node.Start.Line;
			_col_offset = node.Start.Column - 1;
		}

		internal static IronPython.Runtime.List ConvertStatements(Statement stmt)
		{
			return ConvertStatements(stmt, allowNull: false);
		}

		internal static IronPython.Runtime.List ConvertStatements(Statement stmt, bool allowNull)
		{
			if (stmt == null)
			{
				if (allowNull)
				{
					return PythonOps.MakeEmptyList(0);
				}
				throw new ArgumentNullException("stmt");
			}
			if (stmt is SuiteStatement)
			{
				SuiteStatement suiteStatement = (SuiteStatement)stmt;
				IronPython.Runtime.List list = PythonOps.MakeEmptyList(suiteStatement.Statements.Count);
				{
					foreach (Statement statement in suiteStatement.Statements)
					{
						list.Add(Convert(statement));
					}
					return list;
				}
			}
			return PythonOps.MakeListNoCopy(Convert(stmt));
		}

		internal static stmt Convert(Statement stmt)
		{
			stmt stmt2;
			if (stmt is FunctionDefinition)
			{
				stmt2 = new FunctionDef((FunctionDefinition)stmt);
			}
			else if (stmt is ReturnStatement)
			{
				stmt2 = new Return((ReturnStatement)stmt);
			}
			else if (stmt is AssignmentStatement)
			{
				stmt2 = new Assign((AssignmentStatement)stmt);
			}
			else if (stmt is AugmentedAssignStatement)
			{
				stmt2 = new AugAssign((AugmentedAssignStatement)stmt);
			}
			else if (stmt is DelStatement)
			{
				stmt2 = new Delete((DelStatement)stmt);
			}
			else if (stmt is PrintStatement)
			{
				stmt2 = new Print((PrintStatement)stmt);
			}
			else if (stmt is ExpressionStatement)
			{
				stmt2 = new Expr((ExpressionStatement)stmt);
			}
			else if (stmt is ForStatement)
			{
				stmt2 = new For((ForStatement)stmt);
			}
			else if (stmt is WhileStatement)
			{
				stmt2 = new While((WhileStatement)stmt);
			}
			else if (stmt is IfStatement)
			{
				stmt2 = new If((IfStatement)stmt);
			}
			else if (stmt is WithStatement)
			{
				stmt2 = new With((WithStatement)stmt);
			}
			else if (stmt is RaiseStatement)
			{
				stmt2 = new Raise((RaiseStatement)stmt);
			}
			else if (stmt is TryStatement)
			{
				stmt2 = Convert((TryStatement)stmt);
			}
			else if (stmt is AssertStatement)
			{
				stmt2 = new Assert((AssertStatement)stmt);
			}
			else if (stmt is ImportStatement)
			{
				stmt2 = new Import((ImportStatement)stmt);
			}
			else if (stmt is FromImportStatement)
			{
				stmt2 = new ImportFrom((FromImportStatement)stmt);
			}
			else if (stmt is ExecStatement)
			{
				stmt2 = new Exec((ExecStatement)stmt);
			}
			else if (stmt is GlobalStatement)
			{
				stmt2 = new Global((GlobalStatement)stmt);
			}
			else if (stmt is ClassDefinition)
			{
				stmt2 = new ClassDef((ClassDefinition)stmt);
			}
			else if (stmt is BreakStatement)
			{
				stmt2 = new Break();
			}
			else if (stmt is ContinueStatement)
			{
				stmt2 = new Continue();
			}
			else
			{
				if (!(stmt is EmptyStatement))
				{
					throw new ArgumentTypeException("Unexpected statement type: " + stmt.GetType());
				}
				stmt2 = new Pass();
			}
			stmt2.GetSourceLocation(stmt);
			return stmt2;
		}

		internal static stmt Convert(TryStatement stmt)
		{
			if (stmt.Finally != null)
			{
				IronPython.Runtime.List body = ((stmt.Handlers == null || stmt.Handlers.Count == 0) ? ConvertStatements(stmt.Body) : PythonOps.MakeListNoCopy(new TryExcept(stmt)));
				return new TryFinally(body, ConvertStatements(stmt.Finally));
			}
			return new TryExcept(stmt);
		}

		internal static IronPython.Runtime.List ConvertAliases(IList<DottedName> names, IList<string> asnames)
		{
			IronPython.Runtime.List list = PythonOps.MakeEmptyList(names.Count);
			if (names == FromImportStatement.Star)
			{
				list.Add(new alias("*"));
			}
			else
			{
				for (int i = 0; i < names.Count; i++)
				{
					list.Add(new alias(names[i].MakeString(), asnames[i]));
				}
			}
			return list;
		}

		internal static IronPython.Runtime.List ConvertAliases(IList<string> names, IList<string> asnames)
		{
			IronPython.Runtime.List list = PythonOps.MakeEmptyList(names.Count);
			if (names == FromImportStatement.Star)
			{
				list.Add(new alias("*"));
			}
			else
			{
				for (int i = 0; i < names.Count; i++)
				{
					list.Add(new alias(names[i], asnames[i]));
				}
			}
			return list;
		}

		internal static slice TrySliceConvert(IronPython.Compiler.Ast.Expression expr)
		{
			if (expr is SliceExpression)
			{
				return new Slice((SliceExpression)expr);
			}
			if (expr is ConstantExpression && ((ConstantExpression)expr).Value == PythonOps.Ellipsis)
			{
				return Ellipsis.Instance;
			}
			if (expr is TupleExpression && ((TupleExpression)expr).IsExpandable)
			{
				return new ExtSlice(((Tuple)Convert(expr)).elts);
			}
			return null;
		}

		internal static expr Convert(IronPython.Compiler.Ast.Expression expr)
		{
			return Convert(expr, Load.Instance);
		}

		internal static expr Convert(IronPython.Compiler.Ast.Expression expr, expr_context ctx)
		{
			expr expr2;
			if (expr is ConstantExpression)
			{
				expr2 = Convert((ConstantExpression)expr);
			}
			else if (expr is NameExpression)
			{
				expr2 = new Name((NameExpression)expr, ctx);
			}
			else if (expr is UnaryExpression)
			{
				expr2 = new UnaryOp((UnaryExpression)expr);
			}
			else if (expr is BinaryExpression)
			{
				expr2 = Convert((BinaryExpression)expr);
			}
			else if (expr is AndExpression)
			{
				expr2 = new BoolOp((AndExpression)expr);
			}
			else if (expr is OrExpression)
			{
				expr2 = new BoolOp((OrExpression)expr);
			}
			else if (expr is CallExpression)
			{
				expr2 = new Call((CallExpression)expr);
			}
			else
			{
				if (expr is ParenthesisExpression)
				{
					return Convert(((ParenthesisExpression)expr).Expression);
				}
				if (expr is LambdaExpression)
				{
					expr2 = new Lambda((LambdaExpression)expr);
				}
				else if (expr is ListExpression)
				{
					expr2 = new List((ListExpression)expr, ctx);
				}
				else if (expr is TupleExpression)
				{
					expr2 = new Tuple((TupleExpression)expr, ctx);
				}
				else if (expr is DictionaryExpression)
				{
					expr2 = new Dict((DictionaryExpression)expr);
				}
				else if (expr is ListComprehension)
				{
					expr2 = new ListComp((ListComprehension)expr);
				}
				else if (expr is GeneratorExpression)
				{
					expr2 = new GeneratorExp((GeneratorExpression)expr);
				}
				else if (expr is MemberExpression)
				{
					expr2 = new Attribute((MemberExpression)expr, ctx);
				}
				else if (expr is YieldExpression)
				{
					expr2 = new Yield((YieldExpression)expr);
				}
				else if (expr is ConditionalExpression)
				{
					expr2 = new IfExp((ConditionalExpression)expr);
				}
				else if (expr is IndexExpression)
				{
					expr2 = new Subscript((IndexExpression)expr, ctx);
				}
				else if (expr is BackQuoteExpression)
				{
					expr2 = new Repr((BackQuoteExpression)expr);
				}
				else if (expr is SetExpression)
				{
					expr2 = new Set((SetExpression)expr);
				}
				else if (expr is DictionaryComprehension)
				{
					expr2 = new DictComp((DictionaryComprehension)expr);
				}
				else
				{
					if (!(expr is SetComprehension))
					{
						throw new ArgumentTypeException("Unexpected expression type: " + expr.GetType());
					}
					expr2 = new SetComp((SetComprehension)expr);
				}
			}
			expr2.GetSourceLocation(expr);
			return expr2;
		}

		internal static expr Convert(ConstantExpression expr)
		{
			if (expr.Value == null)
			{
				return new Name("None", Load.Instance);
			}
			if (expr.Value is int || expr.Value is double || expr.Value is long || expr.Value is BigInteger || expr.Value is Complex)
			{
				return new Num(expr.Value);
			}
			if (expr.Value is string)
			{
				return new Str((string)expr.Value);
			}
			if (expr.Value is Bytes)
			{
				return new Str(Converter.ConvertToString(expr.Value));
			}
			throw new ArgumentTypeException("Unexpected constant type: " + expr.Value.GetType());
		}

		internal static expr Convert(BinaryExpression expr)
		{
			AST aST = Convert(expr.Operator);
			if (BinaryExpression.IsComparison(expr))
			{
				return new Compare(expr);
			}
			if (aST is @operator)
			{
				return new BinOp(expr, (@operator)aST);
			}
			throw new ArgumentTypeException("Unexpected operator type: " + aST.GetType());
		}

		internal static AST Convert(Node node)
		{
			if (node is TryStatementHandler)
			{
				AST aST = new ExceptHandler((TryStatementHandler)node);
				aST.GetSourceLocation(node);
				return aST;
			}
			throw new ArgumentTypeException("Unexpected node type: " + node.GetType());
		}

		internal static IronPython.Runtime.List Convert(IList<ComprehensionIterator> iters)
		{
			ComprehensionIterator[] array = new ComprehensionIterator[iters.Count];
			iters.CopyTo(array, 0);
			return Convert(array);
		}

		internal static IronPython.Runtime.List Convert(ComprehensionIterator[] iters)
		{
			IronPython.Runtime.List list = new IronPython.Runtime.List();
			int num = 1;
			for (int i = 0; i < iters.Length; i++)
			{
				if (i == 0 || iters[i] is ComprehensionIf)
				{
					if (i != iters.Length - 1)
					{
						continue;
					}
					i++;
				}
				ComprehensionIf[] array = new ComprehensionIf[i - num];
				Array.Copy(iters, num, array, 0, array.Length);
				list.Add(new comprehension((ComprehensionFor)iters[num - 1], array));
				num = i + 1;
			}
			return list;
		}

		internal static AST Convert(PythonOperator op)
		{
			return op switch
			{
				PythonOperator.Add => Add.Instance, 
				PythonOperator.BitwiseAnd => BitAnd.Instance, 
				PythonOperator.BitwiseOr => BitOr.Instance, 
				PythonOperator.Divide => Div.Instance, 
				PythonOperator.Equal => Eq.Instance, 
				PythonOperator.FloorDivide => FloorDiv.Instance, 
				PythonOperator.GreaterThan => Gt.Instance, 
				PythonOperator.GreaterThanOrEqual => GtE.Instance, 
				PythonOperator.In => In.Instance, 
				PythonOperator.Invert => Invert.Instance, 
				PythonOperator.Is => Is.Instance, 
				PythonOperator.IsNot => IsNot.Instance, 
				PythonOperator.LeftShift => LShift.Instance, 
				PythonOperator.LessThan => Lt.Instance, 
				PythonOperator.LessThanOrEqual => LtE.Instance, 
				PythonOperator.Mod => Mod.Instance, 
				PythonOperator.Multiply => Mult.Instance, 
				PythonOperator.Negate => USub.Instance, 
				PythonOperator.Not => Not.Instance, 
				PythonOperator.NotEqual => NotEq.Instance, 
				PythonOperator.NotIn => NotIn.Instance, 
				PythonOperator.Pos => UAdd.Instance, 
				PythonOperator.Power => Pow.Instance, 
				PythonOperator.RightShift => RShift.Instance, 
				PythonOperator.Subtract => Sub.Instance, 
				PythonOperator.Xor => BitXor.Instance, 
				_ => throw new ArgumentException("Unexpected PyOperator: " + op, "op"), 
			};
		}
	}

	[PythonType]
	public class alias : AST
	{
		private string _name;

		private string _asname;

		public string name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		public string asname
		{
			get
			{
				return _asname;
			}
			set
			{
				_asname = value;
			}
		}

		public alias()
		{
			base._fields = new PythonTuple(new string[2] { "name", "asname" });
		}

		internal alias(string name, [Optional] string asname)
			: this()
		{
			_name = name;
			_asname = asname;
		}
	}

	[PythonType]
	public class arguments : AST
	{
		private IronPython.Runtime.List _args;

		private string _vararg;

		private string _kwarg;

		private IronPython.Runtime.List _defaults;

		public IronPython.Runtime.List args
		{
			get
			{
				return _args;
			}
			set
			{
				_args = value;
			}
		}

		public string vararg
		{
			get
			{
				return _vararg;
			}
			set
			{
				_vararg = value;
			}
		}

		public string kwarg
		{
			get
			{
				return _kwarg;
			}
			set
			{
				_kwarg = value;
			}
		}

		public IronPython.Runtime.List defaults
		{
			get
			{
				return _defaults;
			}
			set
			{
				_defaults = value;
			}
		}

		public arguments()
		{
			base._fields = new PythonTuple(new string[4] { "args", "vararg", "kwarg", "defaults" });
		}

		public arguments(IronPython.Runtime.List args, [Optional] string vararg, [Optional] string kwarg, IronPython.Runtime.List defaults)
			: this()
		{
			_args = args;
			_vararg = vararg;
			_kwarg = kwarg;
			_kwarg = kwarg;
			_defaults = defaults;
		}

		internal arguments(IList<Parameter> parameters)
			: this()
		{
			_args = PythonOps.MakeEmptyList(parameters.Count);
			_defaults = PythonOps.MakeEmptyList(parameters.Count);
			foreach (Parameter parameter in parameters)
			{
				if (parameter.IsList)
				{
					_vararg = parameter.Name;
					continue;
				}
				if (parameter.IsDictionary)
				{
					_kwarg = parameter.Name;
					continue;
				}
				args.Add(new Name(parameter.Name, Param.Instance));
				if (parameter.DefaultValue != null)
				{
					defaults.Add(AST.Convert(parameter.DefaultValue));
				}
			}
		}

		internal arguments(Parameter[] parameters)
			: this((IList<Parameter>)parameters)
		{
		}
	}

	[PythonType]
	public abstract class boolop : AST
	{
	}

	[PythonType]
	public abstract class cmpop : AST
	{
	}

	[PythonType]
	public class comprehension : AST
	{
		private expr _target;

		private expr _iter;

		private IronPython.Runtime.List _ifs;

		public expr target
		{
			get
			{
				return _target;
			}
			set
			{
				_target = value;
			}
		}

		public expr iter
		{
			get
			{
				return _iter;
			}
			set
			{
				_iter = value;
			}
		}

		public IronPython.Runtime.List ifs
		{
			get
			{
				return _ifs;
			}
			set
			{
				_ifs = value;
			}
		}

		public comprehension()
		{
			base._fields = new PythonTuple(new string[3] { "target", "iter", "ifs" });
		}

		public comprehension(expr target, expr iter, IronPython.Runtime.List ifs)
			: this()
		{
			_target = target;
			_iter = iter;
			_ifs = ifs;
		}

		internal comprehension(ComprehensionFor listFor, ComprehensionIf[] listIfs)
			: this()
		{
			_target = AST.Convert(listFor.Left, Store.Instance);
			_iter = AST.Convert(listFor.List);
			_ifs = PythonOps.MakeEmptyList(listIfs.Length);
			foreach (ComprehensionIf comprehensionIf in listIfs)
			{
				_ifs.Add(AST.Convert(comprehensionIf.Test));
			}
		}
	}

	[PythonType]
	public class excepthandler : AST
	{
		public excepthandler()
		{
			base._attributes = new PythonTuple(new string[2] { "lineno", "col_offset" });
		}
	}

	[PythonType]
	public abstract class expr : AST
	{
		protected expr()
		{
			base._attributes = new PythonTuple(new string[2] { "lineno", "col_offset" });
		}
	}

	[PythonType]
	public abstract class expr_context : AST
	{
	}

	[PythonType]
	public class keyword : AST
	{
		private string _arg;

		private expr _value;

		public string arg
		{
			get
			{
				return _arg;
			}
			set
			{
				_arg = value;
			}
		}

		public expr value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		public keyword()
		{
			base._fields = new PythonTuple(new string[2] { "arg", "value" });
		}

		public keyword(string arg, expr value)
			: this()
		{
			_arg = arg;
			_value = value;
		}

		internal keyword(Arg arg)
			: this()
		{
			_arg = arg.Name;
			_value = AST.Convert(arg.Expression);
		}
	}

	[PythonType]
	public abstract class mod : AST
	{
		internal abstract IronPython.Runtime.List GetStatements();
	}

	[PythonType]
	public abstract class @operator : AST
	{
	}

	[PythonType]
	public abstract class slice : AST
	{
	}

	[PythonType]
	public abstract class stmt : AST
	{
		protected stmt()
		{
			base._attributes = new PythonTuple(new string[2] { "lineno", "col_offset" });
		}
	}

	[PythonType]
	public abstract class unaryop : AST
	{
	}

	[PythonType]
	public class Add : @operator
	{
		internal static Add Instance = new Add();
	}

	[PythonType]
	public class And : boolop
	{
		internal static And Instance = new And();
	}

	[PythonType]
	public class Assert : stmt
	{
		private expr _test;

		private expr _msg;

		public expr test
		{
			get
			{
				return _test;
			}
			set
			{
				_test = value;
			}
		}

		public expr msg
		{
			get
			{
				return _msg;
			}
			set
			{
				_msg = value;
			}
		}

		public Assert()
		{
			base._fields = new PythonTuple(new string[2] { "test", "msg" });
		}

		public Assert(expr test, expr msg, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_test = test;
			_msg = msg;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal Assert(AssertStatement stmt)
			: this()
		{
			_test = AST.Convert(stmt.Test);
			if (stmt.Message != null)
			{
				_msg = AST.Convert(stmt.Message);
			}
		}
	}

	[PythonType]
	public class Assign : stmt
	{
		private IronPython.Runtime.List _targets;

		private expr _value;

		public IronPython.Runtime.List targets
		{
			get
			{
				return _targets;
			}
			set
			{
				_targets = value;
			}
		}

		public expr value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		public Assign()
		{
			base._fields = new PythonTuple(new string[2] { "targets", "value" });
		}

		public Assign(IronPython.Runtime.List targets, expr value, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_targets = targets;
			_value = value;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal Assign(AssignmentStatement stmt)
			: this()
		{
			_targets = PythonOps.MakeEmptyList(stmt.Left.Count);
			foreach (IronPython.Compiler.Ast.Expression item in stmt.Left)
			{
				_targets.Add(AST.Convert(item, Store.Instance));
			}
			_value = AST.Convert(stmt.Right);
		}
	}

	[PythonType]
	public class Attribute : expr
	{
		private expr _value;

		private string _attr;

		private expr_context _ctx;

		public expr value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		public string attr
		{
			get
			{
				return _attr;
			}
			set
			{
				_attr = value;
			}
		}

		public expr_context ctx
		{
			get
			{
				return _ctx;
			}
			set
			{
				_ctx = value;
			}
		}

		public Attribute()
		{
			base._fields = new PythonTuple(new string[3] { "value", "attr", "ctx" });
		}

		public Attribute(expr value, string attr, expr_context ctx, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_value = value;
			_attr = attr;
			_ctx = ctx;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal Attribute(MemberExpression attr, expr_context ctx)
			: this()
		{
			_value = AST.Convert(attr.Target);
			_attr = attr.Name;
			_ctx = ctx;
		}
	}

	[PythonType]
	public class AugAssign : stmt
	{
		private expr _target;

		private @operator _op;

		private expr _value;

		public expr target
		{
			get
			{
				return _target;
			}
			set
			{
				_target = value;
			}
		}

		public @operator op
		{
			get
			{
				return _op;
			}
			set
			{
				_op = value;
			}
		}

		public expr value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		public AugAssign()
		{
			base._fields = new PythonTuple(new string[3] { "target", "op", "value" });
		}

		public AugAssign(expr target, @operator op, expr value, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_target = target;
			_op = op;
			_value = value;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal AugAssign(AugmentedAssignStatement stmt)
			: this()
		{
			_target = AST.Convert(stmt.Left, Store.Instance);
			_value = AST.Convert(stmt.Right);
			_op = (@operator)AST.Convert(stmt.Operator);
		}
	}

	[PythonType]
	public class AugLoad : expr_context
	{
	}

	[PythonType]
	public class AugStore : expr_context
	{
	}

	[PythonType]
	public class BinOp : expr
	{
		private expr _left;

		private expr _right;

		private @operator _op;

		public expr left
		{
			get
			{
				return _left;
			}
			set
			{
				_left = value;
			}
		}

		public expr right
		{
			get
			{
				return _right;
			}
			set
			{
				_right = value;
			}
		}

		public @operator op
		{
			get
			{
				return _op;
			}
			set
			{
				_op = value;
			}
		}

		public BinOp()
		{
			base._fields = new PythonTuple(new string[3] { "left", "op", "right" });
		}

		public BinOp(expr left, @operator op, expr right, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_left = left;
			_op = op;
			_right = right;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal BinOp(BinaryExpression expr, @operator op)
			: this()
		{
			_left = AST.Convert(expr.Left);
			_right = AST.Convert(expr.Right);
			_op = op;
		}
	}

	[PythonType]
	public class BitAnd : @operator
	{
		internal static BitAnd Instance = new BitAnd();
	}

	[PythonType]
	public class BitOr : @operator
	{
		internal static BitOr Instance = new BitOr();
	}

	[PythonType]
	public class BitXor : @operator
	{
		internal static BitXor Instance = new BitXor();
	}

	[PythonType]
	public class BoolOp : expr
	{
		private boolop _op;

		private IronPython.Runtime.List _values;

		public boolop op
		{
			get
			{
				return _op;
			}
			set
			{
				_op = value;
			}
		}

		public IronPython.Runtime.List values
		{
			get
			{
				return _values;
			}
			set
			{
				_values = value;
			}
		}

		public BoolOp()
		{
			base._fields = new PythonTuple(new string[2] { "op", "values" });
		}

		public BoolOp(boolop op, IronPython.Runtime.List values, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_op = op;
			_values = values;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal BoolOp(AndExpression and)
			: this()
		{
			_values = PythonOps.MakeListNoCopy(AST.Convert(and.Left), AST.Convert(and.Right));
			_op = And.Instance;
		}

		internal BoolOp(OrExpression or)
			: this()
		{
			_values = PythonOps.MakeListNoCopy(AST.Convert(or.Left), AST.Convert(or.Right));
			_op = Or.Instance;
		}
	}

	[PythonType]
	public class Break : stmt
	{
		internal static Break Instance = new Break();

		internal Break()
			: this(null, null)
		{
		}

		public Break([Optional] int? lineno, [Optional] int? col_offset)
		{
			_lineno = lineno;
			_col_offset = col_offset;
		}
	}

	[PythonType]
	public class Call : expr
	{
		private expr _func;

		private IronPython.Runtime.List _args;

		private IronPython.Runtime.List _keywords;

		private expr _starargs;

		private expr _kwargs;

		public expr func
		{
			get
			{
				return _func;
			}
			set
			{
				_func = value;
			}
		}

		public IronPython.Runtime.List args
		{
			get
			{
				return _args;
			}
			set
			{
				_args = value;
			}
		}

		public IronPython.Runtime.List keywords
		{
			get
			{
				return _keywords;
			}
			set
			{
				_keywords = value;
			}
		}

		public expr starargs
		{
			get
			{
				return _starargs;
			}
			set
			{
				_starargs = value;
			}
		}

		public expr kwargs
		{
			get
			{
				return _kwargs;
			}
			set
			{
				_kwargs = value;
			}
		}

		public Call()
		{
			base._fields = new PythonTuple(new string[5] { "func", "args", "keywords", "starargs", "kwargs" });
		}

		public Call(expr func, IronPython.Runtime.List args, IronPython.Runtime.List keywords, [Optional] expr starargs, [Optional] expr kwargs, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_func = func;
			_args = args;
			_keywords = keywords;
			_starargs = starargs;
			_kwargs = kwargs;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal Call(CallExpression call)
			: this()
		{
			_args = PythonOps.MakeEmptyList(call.Args.Count);
			_keywords = new IronPython.Runtime.List();
			_func = AST.Convert(call.Target);
			foreach (Arg arg in call.Args)
			{
				if (arg.Name == null)
				{
					_args.Add(AST.Convert(arg.Expression));
				}
				else if (arg.Name == "*")
				{
					_starargs = AST.Convert(arg.Expression);
				}
				else if (arg.Name == "**")
				{
					_kwargs = AST.Convert(arg.Expression);
				}
				else
				{
					_keywords.Add(new keyword(arg));
				}
			}
		}
	}

	[PythonType]
	public class ClassDef : stmt
	{
		private string _name;

		private IronPython.Runtime.List _bases;

		private IronPython.Runtime.List _body;

		private IronPython.Runtime.List _decorator_list;

		public string name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		public IronPython.Runtime.List bases
		{
			get
			{
				return _bases;
			}
			set
			{
				_bases = value;
			}
		}

		public IronPython.Runtime.List body
		{
			get
			{
				return _body;
			}
			set
			{
				_body = value;
			}
		}

		public IronPython.Runtime.List decorator_list
		{
			get
			{
				return _decorator_list;
			}
			set
			{
				_decorator_list = value;
			}
		}

		public ClassDef()
		{
			base._fields = new PythonTuple(new string[4] { "name", "bases", "body", "decorator_list" });
		}

		public ClassDef(string name, IronPython.Runtime.List bases, IronPython.Runtime.List body, IronPython.Runtime.List decorator_list, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_name = name;
			_bases = bases;
			_body = body;
			_decorator_list = decorator_list;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal ClassDef(ClassDefinition def)
			: this()
		{
			_name = def.Name;
			_bases = PythonOps.MakeEmptyList(def.Bases.Count);
			foreach (IronPython.Compiler.Ast.Expression basis in def.Bases)
			{
				_bases.Add(AST.Convert(basis));
			}
			_body = AST.ConvertStatements(def.Body);
			_decorator_list = new IronPython.Runtime.List();
		}
	}

	[PythonType]
	public class Compare : expr
	{
		private expr _left;

		private IronPython.Runtime.List _ops;

		private IronPython.Runtime.List _comparators;

		public expr left
		{
			get
			{
				return _left;
			}
			set
			{
				_left = value;
			}
		}

		public IronPython.Runtime.List ops
		{
			get
			{
				return _ops;
			}
			set
			{
				_ops = value;
			}
		}

		public IronPython.Runtime.List comparators
		{
			get
			{
				return _comparators;
			}
			set
			{
				_comparators = value;
			}
		}

		public Compare()
		{
			base._fields = new PythonTuple(new string[3] { "left", "ops", "comparators" });
		}

		public Compare(expr left, IronPython.Runtime.List ops, IronPython.Runtime.List comparators, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_left = left;
			_ops = ops;
			_comparators = comparators;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal Compare(BinaryExpression expr)
			: this()
		{
			_left = AST.Convert(expr.Left);
			_ops = PythonOps.MakeList();
			_comparators = PythonOps.MakeList();
			while (BinaryExpression.IsComparison(expr.Right))
			{
				BinaryExpression binaryExpression = (BinaryExpression)expr.Right;
				_ops.Add(AST.Convert(expr.Operator));
				_comparators.Add(AST.Convert(binaryExpression.Left));
				expr = binaryExpression;
			}
			_ops.Add(AST.Convert(expr.Operator));
			_comparators.Add(AST.Convert(expr.Right));
		}
	}

	[PythonType]
	public class Continue : stmt
	{
		internal static Continue Instance = new Continue();

		internal Continue()
			: this(null, null)
		{
		}

		public Continue([Optional] int? lineno, [Optional] int? col_offset)
		{
			_lineno = lineno;
			_col_offset = col_offset;
		}
	}

	[PythonType]
	public class Del : expr_context
	{
		internal static Del Instance = new Del();
	}

	[PythonType]
	public class Delete : stmt
	{
		private IronPython.Runtime.List _targets;

		public IronPython.Runtime.List targets
		{
			get
			{
				return _targets;
			}
			set
			{
				_targets = value;
			}
		}

		public Delete()
		{
			base._fields = new PythonTuple(new string[1] { "targets" });
		}

		public Delete(IronPython.Runtime.List targets, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_targets = targets;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal Delete(DelStatement stmt)
			: this()
		{
			_targets = PythonOps.MakeEmptyList(stmt.Expressions.Count);
			foreach (IronPython.Compiler.Ast.Expression expression in stmt.Expressions)
			{
				_targets.Add(AST.Convert(expression, Del.Instance));
			}
		}
	}

	[PythonType]
	public class Dict : expr
	{
		private IronPython.Runtime.List _keys;

		private IronPython.Runtime.List _values;

		public IronPython.Runtime.List keys
		{
			get
			{
				return _keys;
			}
			set
			{
				_keys = value;
			}
		}

		public IronPython.Runtime.List values
		{
			get
			{
				return _values;
			}
			set
			{
				_values = value;
			}
		}

		public Dict()
		{
			base._fields = new PythonTuple(new string[2] { "keys", "values" });
		}

		public Dict(IronPython.Runtime.List keys, IronPython.Runtime.List values, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_keys = keys;
			_values = values;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal Dict(DictionaryExpression expr)
			: this()
		{
			_keys = PythonOps.MakeEmptyList(expr.Items.Count);
			_values = PythonOps.MakeEmptyList(expr.Items.Count);
			foreach (SliceExpression item in expr.Items)
			{
				_keys.Add(AST.Convert(item.SliceStart));
				_values.Add(AST.Convert(item.SliceStop));
			}
		}
	}

	[PythonType]
	public class DictComp : expr
	{
		private expr _key;

		private expr _value;

		private IronPython.Runtime.List _generators;

		public expr key
		{
			get
			{
				return _key;
			}
			set
			{
				_key = value;
			}
		}

		public expr value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		public IronPython.Runtime.List generators
		{
			get
			{
				return _generators;
			}
			set
			{
				_generators = value;
			}
		}

		public DictComp()
		{
			base._fields = new PythonTuple(new string[3] { "key", "value", "generators" });
		}

		public DictComp(expr key, expr value, IronPython.Runtime.List generators, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_key = key;
			_value = value;
			_generators = generators;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal DictComp(DictionaryComprehension comp)
			: this()
		{
			_key = AST.Convert(comp.Key);
			_value = AST.Convert(comp.Value);
			_generators = AST.Convert(comp.Iterators);
		}
	}

	[PythonType]
	public class Div : @operator
	{
		internal static Div Instance = new Div();
	}

	[PythonType]
	public class Ellipsis : slice
	{
		internal static Ellipsis Instance = new Ellipsis();
	}

	[PythonType]
	public class Eq : cmpop
	{
		internal static Eq Instance = new Eq();
	}

	[PythonType]
	public class ExceptHandler : excepthandler
	{
		private expr _type;

		private expr _name;

		private IronPython.Runtime.List _body;

		public expr type
		{
			get
			{
				return _type;
			}
			set
			{
				_type = value;
			}
		}

		public expr name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		public IronPython.Runtime.List body
		{
			get
			{
				return _body;
			}
			set
			{
				_body = value;
			}
		}

		public ExceptHandler()
		{
			base._fields = new PythonTuple(new string[3] { "type", "name", "body" });
		}

		public ExceptHandler([Optional] expr type, [Optional] expr name, IronPython.Runtime.List body, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_type = type;
			_name = name;
			_body = body;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal ExceptHandler(TryStatementHandler stmt)
			: this()
		{
			if (stmt.Test != null)
			{
				_type = AST.Convert(stmt.Test);
			}
			if (stmt.Target != null)
			{
				_name = AST.Convert(stmt.Target, Store.Instance);
			}
			_body = AST.ConvertStatements(stmt.Body);
		}
	}

	[PythonType]
	public class Exec : stmt
	{
		private expr _body;

		private expr _globals;

		private expr _locals;

		public expr body
		{
			get
			{
				return _body;
			}
			set
			{
				_body = value;
			}
		}

		public expr globals
		{
			get
			{
				return _globals;
			}
			set
			{
				_globals = value;
			}
		}

		public expr locals
		{
			get
			{
				return _locals;
			}
			set
			{
				_locals = value;
			}
		}

		public Exec()
		{
			base._fields = new PythonTuple(new string[3] { "body", "globals", "locals" });
		}

		public Exec(expr body, [Optional] expr globals, [Optional] expr locals, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_body = body;
			_globals = globals;
			_locals = locals;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		public Exec(ExecStatement stmt)
			: this()
		{
			_body = AST.Convert(stmt.Code);
			if (stmt.Globals != null)
			{
				_globals = AST.Convert(stmt.Globals);
			}
			if (stmt.Locals != null)
			{
				_locals = AST.Convert(stmt.Locals);
			}
		}
	}

	[PythonType]
	public class Expr : stmt
	{
		private expr _value;

		public expr value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		public Expr()
		{
			base._fields = new PythonTuple(new string[1] { "value" });
		}

		public Expr(expr value, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_value = value;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal Expr(ExpressionStatement stmt)
			: this()
		{
			_value = AST.Convert(stmt.Expression);
		}
	}

	[PythonType]
	public class Expression : mod
	{
		private expr _body;

		public expr body
		{
			get
			{
				return _body;
			}
			set
			{
				_body = value;
			}
		}

		public Expression()
		{
			base._fields = new PythonTuple(new string[1] { "body" });
		}

		public Expression(expr body)
			: this()
		{
			_body = body;
		}

		internal Expression(SuiteStatement suite)
			: this()
		{
			_body = AST.Convert(((ExpressionStatement)suite.Statements[0]).Expression);
		}

		internal override IronPython.Runtime.List GetStatements()
		{
			return PythonOps.MakeListNoCopy(_body);
		}
	}

	[PythonType]
	public class ExtSlice : slice
	{
		private IronPython.Runtime.List _dims;

		public IronPython.Runtime.List dims
		{
			get
			{
				return _dims;
			}
			set
			{
				_dims = value;
			}
		}

		public ExtSlice()
		{
			base._fields = new PythonTuple(new string[1] { "dims" });
		}

		public ExtSlice(IronPython.Runtime.List dims)
			: this()
		{
			_dims = dims;
		}
	}

	[PythonType]
	public class FloorDiv : @operator
	{
		internal static FloorDiv Instance = new FloorDiv();
	}

	[PythonType]
	public class For : stmt
	{
		private expr _target;

		private expr _iter;

		private IronPython.Runtime.List _body;

		private IronPython.Runtime.List _orelse;

		public expr target
		{
			get
			{
				return _target;
			}
			set
			{
				_target = value;
			}
		}

		public expr iter
		{
			get
			{
				return _iter;
			}
			set
			{
				_iter = value;
			}
		}

		public IronPython.Runtime.List body
		{
			get
			{
				return _body;
			}
			set
			{
				_body = value;
			}
		}

		public IronPython.Runtime.List orelse
		{
			get
			{
				return _orelse;
			}
			set
			{
				_orelse = value;
			}
		}

		public For()
		{
			base._fields = new PythonTuple(new string[4] { "target", "iter", "body", "orelse" });
		}

		public For(expr target, expr iter, IronPython.Runtime.List body, [Optional] IronPython.Runtime.List orelse, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_target = target;
			_iter = iter;
			_body = body;
			if (orelse == null)
			{
				_orelse = new IronPython.Runtime.List();
			}
			else
			{
				_orelse = orelse;
			}
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal For(ForStatement stmt)
			: this()
		{
			_target = AST.Convert(stmt.Left, Store.Instance);
			_iter = AST.Convert(stmt.List);
			_body = AST.ConvertStatements(stmt.Body);
			_orelse = AST.ConvertStatements(stmt.Else, allowNull: true);
		}
	}

	[PythonType]
	public class FunctionDef : stmt
	{
		private string _name;

		private arguments _args;

		private IronPython.Runtime.List _body;

		private IronPython.Runtime.List _decorators;

		public string name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		public arguments args
		{
			get
			{
				return _args;
			}
			set
			{
				_args = value;
			}
		}

		public IronPython.Runtime.List body
		{
			get
			{
				return _body;
			}
			set
			{
				_body = value;
			}
		}

		public IronPython.Runtime.List decorators
		{
			get
			{
				return _decorators;
			}
			set
			{
				_decorators = value;
			}
		}

		public FunctionDef()
		{
			base._fields = new PythonTuple(new string[4] { "name", "args", "body", "decorators" });
		}

		public FunctionDef(string name, arguments args, IronPython.Runtime.List body, IronPython.Runtime.List decorators, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_name = name;
			_args = args;
			_body = body;
			_decorators = decorators;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal FunctionDef(FunctionDefinition def)
			: this()
		{
			_name = def.Name;
			_args = new arguments(def.Parameters);
			_body = AST.ConvertStatements(def.Body);
			if (def.Decorators != null)
			{
				_decorators = PythonOps.MakeEmptyList(def.Decorators.Count);
				{
					foreach (IronPython.Compiler.Ast.Expression decorator in def.Decorators)
					{
						_decorators.Add(AST.Convert(decorator));
					}
					return;
				}
			}
			_decorators = PythonOps.MakeEmptyList(0);
		}
	}

	[PythonType]
	public class GeneratorExp : expr
	{
		internal class ExtractListComprehensionIterators : PythonWalker
		{
			private readonly List<ComprehensionIterator> _iterators = new List<ComprehensionIterator>();

			public YieldExpression Yield;

			public ComprehensionIterator[] Iterators => _iterators.ToArray();

			public override bool Walk(ForStatement node)
			{
				_iterators.Add(new ComprehensionFor(node.Left, node.List));
				node.Body.Walk(this);
				return false;
			}

			public override bool Walk(IfStatement node)
			{
				_iterators.Add(new ComprehensionIf(node.Tests[0].Test));
				node.Tests[0].Body.Walk(this);
				return false;
			}

			public override bool Walk(YieldExpression node)
			{
				Yield = node;
				return false;
			}
		}

		private expr _elt;

		private IronPython.Runtime.List _generators;

		public expr elt
		{
			get
			{
				return _elt;
			}
			set
			{
				_elt = value;
			}
		}

		public IronPython.Runtime.List generators
		{
			get
			{
				return _generators;
			}
			set
			{
				_generators = value;
			}
		}

		public GeneratorExp()
		{
			base._fields = new PythonTuple(new string[2] { "elt", "generators" });
		}

		public GeneratorExp(expr elt, IronPython.Runtime.List generators, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_elt = elt;
			_generators = generators;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal GeneratorExp(GeneratorExpression expr)
			: this()
		{
			ExtractListComprehensionIterators extractListComprehensionIterators = new ExtractListComprehensionIterators();
			expr.Function.Body.Walk(extractListComprehensionIterators);
			ComprehensionIterator[] iterators = extractListComprehensionIterators.Iterators;
			iterators[0] = new ComprehensionFor(((ComprehensionFor)iterators[0]).Left, expr.Iterable);
			_elt = AST.Convert(extractListComprehensionIterators.Yield.Expression);
			_generators = AST.Convert(iterators);
		}
	}

	[PythonType]
	public class Global : stmt
	{
		private IronPython.Runtime.List _names;

		public IronPython.Runtime.List names
		{
			get
			{
				return _names;
			}
			set
			{
				_names = value;
			}
		}

		public Global()
		{
			base._fields = new PythonTuple(new string[1] { "names" });
		}

		public Global(IronPython.Runtime.List names, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_names = names;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal Global(GlobalStatement stmt)
			: this()
		{
			_names = new IronPython.Runtime.List(stmt.Names);
		}
	}

	[PythonType]
	public class Gt : cmpop
	{
		internal static Gt Instance = new Gt();
	}

	[PythonType]
	public class GtE : cmpop
	{
		internal static GtE Instance = new GtE();
	}

	[PythonType]
	public class If : stmt
	{
		private expr _test;

		private IronPython.Runtime.List _body;

		private IronPython.Runtime.List _orelse;

		public expr test
		{
			get
			{
				return _test;
			}
			set
			{
				_test = value;
			}
		}

		public IronPython.Runtime.List body
		{
			get
			{
				return _body;
			}
			set
			{
				_body = value;
			}
		}

		public IronPython.Runtime.List orelse
		{
			get
			{
				return _orelse;
			}
			set
			{
				_orelse = value;
			}
		}

		public If()
		{
			base._fields = new PythonTuple(new string[3] { "test", "body", "orelse" });
		}

		public If(expr test, IronPython.Runtime.List body, [Optional] IronPython.Runtime.List orelse, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_test = test;
			_body = body;
			if (orelse == null)
			{
				_orelse = new IronPython.Runtime.List();
			}
			else
			{
				_orelse = orelse;
			}
		}

		internal If(IfStatement stmt)
			: this()
		{
			If obj = this;
			If obj2 = null;
			foreach (IfStatementTest test in stmt.Tests)
			{
				if (obj2 != null)
				{
					obj = new If();
					obj2._orelse = PythonOps.MakeListNoCopy(obj);
				}
				obj.Initialize(test);
				obj2 = obj;
			}
			obj._orelse = AST.ConvertStatements(stmt.ElseStatement, allowNull: true);
		}

		internal void Initialize(IfStatementTest ifTest)
		{
			_test = AST.Convert(ifTest.Test);
			_body = AST.ConvertStatements(ifTest.Body);
		}
	}

	[PythonType]
	public class IfExp : expr
	{
		private expr _test;

		private expr _body;

		private expr _orelse;

		public expr test
		{
			get
			{
				return _test;
			}
			set
			{
				_test = value;
			}
		}

		public expr body
		{
			get
			{
				return _body;
			}
			set
			{
				_body = value;
			}
		}

		public expr orelse
		{
			get
			{
				return _orelse;
			}
			set
			{
				_orelse = value;
			}
		}

		public IfExp()
		{
			base._fields = new PythonTuple(new string[3] { "test", "body", "orelse" });
		}

		public IfExp(expr test, expr body, expr orelse, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_test = test;
			_body = body;
			_orelse = orelse;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal IfExp(ConditionalExpression cond)
			: this()
		{
			_test = AST.Convert(cond.Test);
			_body = AST.Convert(cond.TrueExpression);
			_orelse = AST.Convert(cond.FalseExpression);
		}
	}

	[PythonType]
	public class Import : stmt
	{
		private IronPython.Runtime.List _names;

		public IronPython.Runtime.List names
		{
			get
			{
				return _names;
			}
			set
			{
				_names = value;
			}
		}

		public Import()
		{
			base._fields = new PythonTuple(new string[1] { "names" });
		}

		public Import(IronPython.Runtime.List names, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_names = names;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal Import(ImportStatement stmt)
			: this()
		{
			_names = AST.ConvertAliases(stmt.Names, stmt.AsNames);
		}
	}

	[PythonType]
	public class ImportFrom : stmt
	{
		private string _module;

		private IronPython.Runtime.List _names;

		private int _level;

		public string module
		{
			get
			{
				return _module;
			}
			set
			{
				_module = value;
			}
		}

		public IronPython.Runtime.List names
		{
			get
			{
				return _names;
			}
			set
			{
				_names = value;
			}
		}

		public int level
		{
			get
			{
				return _level;
			}
			set
			{
				_level = value;
			}
		}

		public ImportFrom()
		{
			base._fields = new PythonTuple(new string[3] { "module", "names", "level" });
		}

		public ImportFrom([Optional] string module, IronPython.Runtime.List names, [Optional] int level, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_module = module;
			_names = names;
			_level = level;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		public ImportFrom(FromImportStatement stmt)
			: this()
		{
			_module = stmt.Root.MakeString();
			_module = (string.IsNullOrEmpty(_module) ? null : _module);
			_names = AST.ConvertAliases(stmt.Names, stmt.AsNames);
			if (stmt.Root is RelativeModuleName)
			{
				_level = ((RelativeModuleName)stmt.Root).DotCount;
			}
		}
	}

	[PythonType]
	public class In : cmpop
	{
		internal static In Instance = new In();
	}

	[PythonType]
	public class Index : slice
	{
		private expr _value;

		public expr value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		public Index()
		{
			base._fields = new PythonTuple(new string[1] { "value" });
		}

		public Index(expr value)
			: this()
		{
			_value = value;
		}
	}

	[PythonType]
	public class Interactive : mod
	{
		private IronPython.Runtime.List _body;

		public IronPython.Runtime.List body
		{
			get
			{
				return _body;
			}
			set
			{
				_body = value;
			}
		}

		public Interactive()
		{
			base._fields = new PythonTuple(new string[1] { "body" });
		}

		public Interactive(IronPython.Runtime.List body)
			: this()
		{
			_body = body;
		}

		internal Interactive(SuiteStatement suite)
			: this()
		{
			_body = AST.ConvertStatements(suite);
		}

		internal override IronPython.Runtime.List GetStatements()
		{
			return _body;
		}
	}

	[PythonType]
	public class Invert : unaryop
	{
		internal static Invert Instance = new Invert();
	}

	[PythonType]
	public class Is : cmpop
	{
		internal static Is Instance = new Is();
	}

	[PythonType]
	public class IsNot : cmpop
	{
		internal static IsNot Instance = new IsNot();
	}

	[PythonType]
	public class Lambda : expr
	{
		private arguments _args;

		private expr _body;

		public arguments args
		{
			get
			{
				return _args;
			}
			set
			{
				_args = value;
			}
		}

		public expr body
		{
			get
			{
				return _body;
			}
			set
			{
				_body = value;
			}
		}

		public Lambda()
		{
			base._fields = new PythonTuple(new string[2] { "args", "body" });
		}

		public Lambda(arguments args, expr body, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_args = args;
			_body = body;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal Lambda(LambdaExpression lambda)
			: this()
		{
			FunctionDef functionDef = (FunctionDef)AST.Convert(lambda.Function);
			_args = functionDef.args;
			_body = ((Return)functionDef.body[0]).value;
		}
	}

	[PythonType]
	public class List : expr
	{
		private IronPython.Runtime.List _elts;

		private expr_context _ctx;

		public IronPython.Runtime.List elts
		{
			get
			{
				return _elts;
			}
			set
			{
				_elts = value;
			}
		}

		public expr_context ctx
		{
			get
			{
				return _ctx;
			}
			set
			{
				_ctx = value;
			}
		}

		public List()
		{
			base._fields = new PythonTuple(new string[2] { "elts", "ctx" });
		}

		public List(IronPython.Runtime.List elts, expr_context ctx, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_elts = elts;
			_ctx = ctx;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal List(ListExpression list, expr_context ctx)
			: this()
		{
			_elts = PythonOps.MakeEmptyList(list.Items.Count);
			foreach (IronPython.Compiler.Ast.Expression item in list.Items)
			{
				_elts.Add(AST.Convert(item, ctx));
			}
			_ctx = ctx;
		}
	}

	[PythonType]
	public class ListComp : expr
	{
		private expr _elt;

		private IronPython.Runtime.List _generators;

		public expr elt
		{
			get
			{
				return _elt;
			}
			set
			{
				_elt = value;
			}
		}

		public IronPython.Runtime.List generators
		{
			get
			{
				return _generators;
			}
			set
			{
				_generators = value;
			}
		}

		public ListComp()
		{
			base._fields = new PythonTuple(new string[2] { "elt", "generators" });
		}

		public ListComp(expr elt, IronPython.Runtime.List generators, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_elt = elt;
			_generators = generators;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal ListComp(ListComprehension comp)
			: this()
		{
			_elt = AST.Convert(comp.Item);
			_generators = AST.Convert(comp.Iterators);
		}
	}

	[PythonType]
	public class Load : expr_context
	{
		internal static Load Instance = new Load();
	}

	[PythonType]
	public class Lt : cmpop
	{
		internal static Lt Instance = new Lt();
	}

	[PythonType]
	public class LtE : cmpop
	{
		internal static LtE Instance = new LtE();
	}

	[PythonType]
	public class LShift : @operator
	{
		internal static LShift Instance = new LShift();
	}

	[PythonType]
	public class Mod : @operator
	{
		internal static Mod Instance = new Mod();
	}

	[PythonType]
	public class Module : mod
	{
		private IronPython.Runtime.List _body;

		public IronPython.Runtime.List body
		{
			get
			{
				return _body;
			}
			set
			{
				_body = value;
			}
		}

		public Module()
		{
			base._fields = new PythonTuple(new string[1] { "body" });
		}

		public Module(IronPython.Runtime.List body)
			: this()
		{
			_body = body;
		}

		internal Module(SuiteStatement suite)
			: this()
		{
			_body = AST.ConvertStatements(suite);
		}

		internal override IronPython.Runtime.List GetStatements()
		{
			return _body;
		}
	}

	[PythonType]
	public class Mult : @operator
	{
		internal static Mult Instance = new Mult();
	}

	[PythonType]
	public class Name : expr
	{
		private string _id;

		private expr_context _ctx;

		public expr_context ctx
		{
			get
			{
				return _ctx;
			}
			set
			{
				_ctx = value;
			}
		}

		public string id
		{
			get
			{
				return _id;
			}
			set
			{
				_id = value;
			}
		}

		public Name()
		{
			base._fields = new PythonTuple(new string[2] { "id", "ctx" });
		}

		public Name(string id, expr_context ctx, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_id = id;
			_ctx = ctx;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		public Name(string id, expr_context ctx)
			: this(id, ctx, null, null)
		{
		}

		internal Name(NameExpression expr, expr_context ctx)
			: this(expr.Name, ctx)
		{
		}
	}

	[PythonType]
	public class Not : unaryop
	{
		internal static Not Instance = new Not();
	}

	[PythonType]
	public class NotEq : cmpop
	{
		internal static NotEq Instance = new NotEq();
	}

	[PythonType]
	public class NotIn : cmpop
	{
		internal static NotIn Instance = new NotIn();
	}

	[PythonType]
	public class Num : expr
	{
		private object _n;

		public object n
		{
			get
			{
				return _n;
			}
			set
			{
				_n = value;
			}
		}

		public Num()
		{
			base._fields = new PythonTuple(new string[1] { "n" });
		}

		internal Num(object n)
			: this(n, null, null)
		{
		}

		public Num(object n, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_n = n;
			_lineno = lineno;
			_col_offset = col_offset;
		}
	}

	[PythonType]
	public class Or : boolop
	{
		internal static Or Instance = new Or();
	}

	[PythonType]
	public class Param : expr_context
	{
		internal static Param Instance = new Param();
	}

	[PythonType]
	public class Pass : stmt
	{
		internal static Pass Instance = new Pass();

		internal Pass()
			: this(null, null)
		{
		}

		public Pass([Optional] int? lineno, [Optional] int? col_offset)
		{
			_lineno = lineno;
			_col_offset = col_offset;
		}
	}

	[PythonType]
	public class Pow : @operator
	{
		internal static Pow Instance = new Pow();
	}

	[PythonType]
	public class Print : stmt
	{
		private expr _dest;

		private IronPython.Runtime.List _values;

		private bool _nl;

		public expr dest
		{
			get
			{
				return _dest;
			}
			set
			{
				_dest = value;
			}
		}

		public IronPython.Runtime.List values
		{
			get
			{
				return _values;
			}
			set
			{
				_values = value;
			}
		}

		public bool nl
		{
			get
			{
				return _nl;
			}
			set
			{
				_nl = value;
			}
		}

		public Print()
		{
			base._fields = new PythonTuple(new string[3] { "dest", "values", "nl" });
		}

		public Print([Optional] expr dest, IronPython.Runtime.List values, bool nl, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_dest = dest;
			_values = values;
			_nl = nl;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal Print(PrintStatement stmt)
			: this()
		{
			if (stmt.Destination != null)
			{
				_dest = AST.Convert(stmt.Destination);
			}
			_values = PythonOps.MakeEmptyList(stmt.Expressions.Count);
			foreach (IronPython.Compiler.Ast.Expression expression in stmt.Expressions)
			{
				_values.Add(AST.Convert(expression));
			}
			_nl = !stmt.TrailingComma;
		}
	}

	[PythonType]
	public class Raise : stmt
	{
		private expr _type;

		private expr _inst;

		private expr _tback;

		public expr type
		{
			get
			{
				return _type;
			}
			set
			{
				_type = value;
			}
		}

		public expr inst
		{
			get
			{
				return _inst;
			}
			set
			{
				_inst = value;
			}
		}

		public expr tback
		{
			get
			{
				return _tback;
			}
			set
			{
				_tback = value;
			}
		}

		public Raise()
		{
			base._fields = new PythonTuple(new string[3] { "type", "inst", "tback" });
		}

		public Raise([Optional] expr type, [Optional] expr inst, [Optional] expr tback, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_type = type;
			_inst = inst;
			_tback = tback;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal Raise(RaiseStatement stmt)
			: this()
		{
			if (stmt.ExceptType != null)
			{
				_type = AST.Convert(stmt.ExceptType);
			}
			if (stmt.Value != null)
			{
				_inst = AST.Convert(stmt.Value);
			}
			if (stmt.Traceback != null)
			{
				_tback = AST.Convert(stmt.Traceback);
			}
		}
	}

	[PythonType]
	public class Repr : expr
	{
		private expr _value;

		public expr value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		public Repr()
		{
			base._fields = new PythonTuple(new string[1] { "value" });
		}

		public Repr(expr value, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_value = value;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal Repr(BackQuoteExpression expr)
			: this()
		{
			_value = AST.Convert(expr.Expression);
		}
	}

	[PythonType]
	public class Return : stmt
	{
		private expr _value;

		public expr value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		public Return()
		{
			base._fields = new PythonTuple(new string[1] { "value" });
		}

		public Return([Optional] expr value, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_value = value;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		public Return(ReturnStatement statement)
			: this()
		{
			if (statement.Expression == null)
			{
				_value = null;
			}
			else
			{
				_value = AST.Convert(statement.Expression);
			}
		}
	}

	[PythonType]
	public class RShift : @operator
	{
		internal static RShift Instance = new RShift();
	}

	[PythonType]
	public class Set : expr
	{
		private IronPython.Runtime.List _elts;

		public IronPython.Runtime.List elts
		{
			get
			{
				return _elts;
			}
			set
			{
				_elts = value;
			}
		}

		public Set()
		{
			base._fields = new PythonTuple(new string[1] { "elts" });
		}

		public Set(IronPython.Runtime.List elts, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_elts = elts;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal Set(SetExpression setExpression)
			: this()
		{
			_elts = new IronPython.Runtime.List(setExpression.Items.Count);
			foreach (IronPython.Compiler.Ast.Expression item in setExpression.Items)
			{
				_elts.Add(AST.Convert(item));
			}
		}
	}

	[PythonType]
	public class SetComp : expr
	{
		private expr _elt;

		private IronPython.Runtime.List _generators;

		public expr elt
		{
			get
			{
				return _elt;
			}
			set
			{
				_elt = value;
			}
		}

		public IronPython.Runtime.List generators
		{
			get
			{
				return _generators;
			}
			set
			{
				_generators = value;
			}
		}

		public SetComp()
		{
			base._fields = new PythonTuple(new string[2] { "elt", "generators" });
		}

		public SetComp(expr elt, IronPython.Runtime.List generators, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_elt = elt;
			_generators = generators;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal SetComp(SetComprehension comp)
			: this()
		{
			_elt = AST.Convert(comp.Item);
			_generators = AST.Convert(comp.Iterators);
		}
	}

	[PythonType]
	public class Slice : slice
	{
		private expr _lower;

		private expr _upper;

		private expr _step;

		public expr lower
		{
			get
			{
				return _lower;
			}
			set
			{
				_lower = value;
			}
		}

		public expr upper
		{
			get
			{
				return _upper;
			}
			set
			{
				_upper = value;
			}
		}

		public expr step
		{
			get
			{
				return _step;
			}
			set
			{
				_step = value;
			}
		}

		public Slice()
		{
			base._fields = new PythonTuple(new string[3] { "lower", "upper", "step" });
		}

		public Slice([Optional] expr lower, [Optional] expr upper, [Optional] expr step)
			: this()
		{
			_lower = lower;
			_upper = upper;
			_step = step;
		}

		internal Slice(SliceExpression expr)
			: this()
		{
			if (expr.SliceStart != null)
			{
				_lower = AST.Convert(expr.SliceStart);
			}
			if (expr.SliceStop != null)
			{
				_upper = AST.Convert(expr.SliceStop);
			}
			if (expr.StepProvided)
			{
				if (expr.SliceStep != null)
				{
					_step = AST.Convert(expr.SliceStep);
				}
				else
				{
					_step = new Name("None", Load.Instance);
				}
			}
		}
	}

	[PythonType]
	public class Store : expr_context
	{
		internal static Store Instance = new Store();
	}

	[PythonType]
	public class Str : expr
	{
		private string _s;

		public string s
		{
			get
			{
				return _s;
			}
			set
			{
				_s = value;
			}
		}

		public Str()
		{
			base._fields = new PythonTuple(new string[1] { "s" });
		}

		internal Str(string s)
			: this(s, null, null)
		{
		}

		public Str(string s, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_s = s;
			_lineno = lineno;
			_col_offset = col_offset;
		}
	}

	[PythonType]
	public class Sub : @operator
	{
		internal static Sub Instance = new Sub();
	}

	[PythonType]
	public class Subscript : expr
	{
		private expr _value;

		private slice _slice;

		private expr_context _ctx;

		public expr value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		public slice slice
		{
			get
			{
				return _slice;
			}
			set
			{
				_slice = value;
			}
		}

		public expr_context ctx
		{
			get
			{
				return _ctx;
			}
			set
			{
				_ctx = value;
			}
		}

		public Subscript()
		{
			base._fields = new PythonTuple(new string[3] { "value", "slice", "ctx" });
		}

		public Subscript(expr value, slice slice, expr_context ctx, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_value = value;
			_slice = slice;
			_ctx = ctx;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal Subscript(IndexExpression expr, expr_context ctx)
			: this()
		{
			_value = AST.Convert(expr.Target);
			_ctx = ctx;
			_slice = AST.TrySliceConvert(expr.Index);
			if (_slice == null)
			{
				_slice = new Index(AST.Convert(expr.Index));
			}
		}
	}

	[PythonType]
	public class Suite : mod
	{
		private IronPython.Runtime.List _body;

		public IronPython.Runtime.List body
		{
			get
			{
				return _body;
			}
			set
			{
				_body = value;
			}
		}

		public Suite()
		{
			base._fields = new PythonTuple(new string[1] { "body" });
		}

		public Suite(IronPython.Runtime.List body)
			: this()
		{
			_body = body;
		}

		internal override IronPython.Runtime.List GetStatements()
		{
			return _body;
		}
	}

	[PythonType]
	public class TryExcept : stmt
	{
		private IronPython.Runtime.List _body;

		private IronPython.Runtime.List _handlers;

		private IronPython.Runtime.List _orelse;

		public IronPython.Runtime.List body
		{
			get
			{
				return _body;
			}
			set
			{
				_body = value;
			}
		}

		public IronPython.Runtime.List handlers
		{
			get
			{
				return _handlers;
			}
			set
			{
				_handlers = value;
			}
		}

		public IronPython.Runtime.List orelse
		{
			get
			{
				return _orelse;
			}
			set
			{
				_orelse = value;
			}
		}

		public TryExcept()
		{
			base._fields = new PythonTuple(new string[3] { "body", "handlers", "orelse" });
		}

		public TryExcept(IronPython.Runtime.List body, IronPython.Runtime.List handlers, [Optional] IronPython.Runtime.List orelse, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_body = body;
			_handlers = handlers;
			if (orelse == null)
			{
				_orelse = new IronPython.Runtime.List();
			}
			else
			{
				_orelse = orelse;
			}
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal TryExcept(TryStatement stmt)
			: this()
		{
			_body = AST.ConvertStatements(stmt.Body);
			_handlers = PythonOps.MakeEmptyList(stmt.Handlers.Count);
			foreach (TryStatementHandler handler in stmt.Handlers)
			{
				_handlers.Add(AST.Convert(handler));
			}
			_orelse = AST.ConvertStatements(stmt.Else, allowNull: true);
		}
	}

	[PythonType]
	public class TryFinally : stmt
	{
		private IronPython.Runtime.List _body;

		private IronPython.Runtime.List _finalbody;

		public IronPython.Runtime.List body
		{
			get
			{
				return _body;
			}
			set
			{
				_body = value;
			}
		}

		public IronPython.Runtime.List finalbody
		{
			get
			{
				return _finalbody;
			}
			set
			{
				_finalbody = value;
			}
		}

		public TryFinally()
		{
			base._fields = new PythonTuple(new string[2] { "body", "finalbody" });
		}

		public TryFinally(IronPython.Runtime.List body, IronPython.Runtime.List finalBody, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_body = body;
			_finalbody = finalbody;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal TryFinally(IronPython.Runtime.List body, IronPython.Runtime.List finalbody)
			: this()
		{
			_body = body;
			_finalbody = finalbody;
		}
	}

	[PythonType]
	public class Tuple : expr
	{
		private IronPython.Runtime.List _elts;

		private expr_context _ctx;

		public IronPython.Runtime.List elts
		{
			get
			{
				return _elts;
			}
			set
			{
				_elts = value;
			}
		}

		public expr_context ctx
		{
			get
			{
				return _ctx;
			}
			set
			{
				_ctx = value;
			}
		}

		public Tuple()
		{
			base._fields = new PythonTuple(new string[2] { "elts", "ctx" });
		}

		public Tuple(IronPython.Runtime.List elts, expr_context ctx, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_elts = elts;
			_ctx = ctx;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal Tuple(TupleExpression list, expr_context ctx)
			: this()
		{
			_elts = PythonOps.MakeEmptyList(list.Items.Count);
			foreach (IronPython.Compiler.Ast.Expression item in list.Items)
			{
				_elts.Add(AST.Convert(item, ctx));
			}
			_ctx = ctx;
		}
	}

	[PythonType]
	public class UnaryOp : expr
	{
		private unaryop _op;

		private expr _operand;

		public unaryop op
		{
			get
			{
				return _op;
			}
			set
			{
				_op = value;
			}
		}

		public expr operand
		{
			get
			{
				return _operand;
			}
			set
			{
				_operand = value;
			}
		}

		public UnaryOp()
		{
			base._fields = new PythonTuple(new string[2] { "op", "operand" });
		}

		internal UnaryOp(UnaryExpression expression)
			: this()
		{
			_op = (unaryop)AST.Convert(expression.Op);
			_operand = AST.Convert(expression.Expression);
		}

		public UnaryOp(unaryop op, expr operand, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_op = op;
			_operand = operand;
			_lineno = lineno;
			_col_offset = col_offset;
		}
	}

	[PythonType]
	public class UAdd : unaryop
	{
		internal static UAdd Instance = new UAdd();
	}

	[PythonType]
	public class USub : unaryop
	{
		internal static USub Instance = new USub();
	}

	[PythonType]
	public class While : stmt
	{
		private expr _test;

		private IronPython.Runtime.List _body;

		private IronPython.Runtime.List _orelse;

		public expr test
		{
			get
			{
				return _test;
			}
			set
			{
				_test = value;
			}
		}

		public IronPython.Runtime.List body
		{
			get
			{
				return _body;
			}
			set
			{
				_body = value;
			}
		}

		public IronPython.Runtime.List orelse
		{
			get
			{
				return _orelse;
			}
			set
			{
				_orelse = value;
			}
		}

		public While()
		{
			base._fields = new PythonTuple(new string[3] { "test", "body", "orelse" });
		}

		public While(expr test, IronPython.Runtime.List body, [Optional] IronPython.Runtime.List orelse, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_test = test;
			_body = body;
			if (orelse == null)
			{
				_orelse = new IronPython.Runtime.List();
			}
			else
			{
				_orelse = orelse;
			}
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal While(WhileStatement stmt)
			: this()
		{
			_test = AST.Convert(stmt.Test);
			_body = AST.ConvertStatements(stmt.Body);
			_orelse = AST.ConvertStatements(stmt.ElseStatement, allowNull: true);
		}
	}

	[PythonType]
	public class With : stmt
	{
		private expr _context_expr;

		private expr _optional_vars;

		private IronPython.Runtime.List _body;

		public expr context_expr
		{
			get
			{
				return _context_expr;
			}
			set
			{
				_context_expr = value;
			}
		}

		public expr optional_vars
		{
			get
			{
				return _optional_vars;
			}
			set
			{
				_optional_vars = value;
			}
		}

		public IronPython.Runtime.List body
		{
			get
			{
				return _body;
			}
			set
			{
				_body = value;
			}
		}

		public With()
		{
			base._fields = new PythonTuple(new string[3] { "context_expr", "optional_vars", "body" });
		}

		public With(expr context_expr, [Optional] expr optional_vars, IronPython.Runtime.List body, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_context_expr = context_expr;
			_optional_vars = optional_vars;
			_body = body;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal With(WithStatement with)
			: this()
		{
			_context_expr = AST.Convert(with.ContextManager);
			if (with.Variable != null)
			{
				_optional_vars = AST.Convert(with.Variable);
			}
			_body = AST.ConvertStatements(with.Body);
		}
	}

	[PythonType]
	public class Yield : expr
	{
		private expr _value;

		public expr value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		public Yield()
		{
			base._fields = new PythonTuple(new string[1] { "value" });
		}

		public Yield([Optional] expr value, [Optional] int? lineno, [Optional] int? col_offset)
			: this()
		{
			_value = value;
			_lineno = lineno;
			_col_offset = col_offset;
		}

		internal Yield(YieldExpression expr)
			: this()
		{
			_value = AST.Convert(expr.Expression);
		}
	}

	public const string __version__ = "62047";

	public const int PyCF_ONLY_AST = 1024;

	internal static AST BuildAst(CodeContext context, SourceUnit sourceUnit, PythonCompilerOptions opts, string mode)
	{
		Parser parser = Parser.CreateParser(new CompilerContext(sourceUnit, opts, ThrowingErrorSink.Default), (PythonOptions)context.LanguageContext.Options);
		PythonAst pythonAst = parser.ParseFile(makeModule: true);
		return ConvertToAST(pythonAst, mode);
	}

	private static mod ConvertToAST(PythonAst pythonAst, string kind)
	{
		ContractUtils.RequiresNotNull(pythonAst, "pythonAst");
		ContractUtils.RequiresNotNull(kind, "kind");
		return ConvertToAST((SuiteStatement)pythonAst.Body, kind);
	}

	private static mod ConvertToAST(SuiteStatement suite, string kind)
	{
		ContractUtils.RequiresNotNull(suite, "suite");
		ContractUtils.RequiresNotNull(kind, "kind");
		return kind switch
		{
			"exec" => new Module(suite), 
			"eval" => new Expression(suite), 
			"single" => new Interactive(suite), 
			_ => throw new ArgumentException("kind must be 'exec' or 'eval' or 'single'"), 
		};
	}

	private static stmt ConvertToAST(Statement stmt)
	{
		ContractUtils.RequiresNotNull(stmt, "stmt");
		return AST.Convert(stmt);
	}

	private static expr ConvertToAST(IronPython.Compiler.Ast.Expression expr)
	{
		ContractUtils.RequiresNotNull(expr, "expr");
		return AST.Convert(expr);
	}
}
