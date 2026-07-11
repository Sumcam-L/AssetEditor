using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Interpreter;

namespace IronPython.Compiler.Ast;

public class DictionaryExpression : Expression, IInstructionProvider
{
	private class EmptyDictInstruction : Instruction
	{
		public static EmptyDictInstruction Instance = new EmptyDictInstruction();

		public override int ProducedStack => 1;

		public override int Run(InterpretedFrame frame)
		{
			frame.Push(PythonOps.MakeEmptyDict());
			return 1;
		}
	}

	private readonly SliceExpression[] _items;

	private static System.Linq.Expressions.Expression EmptyDictExpression = System.Linq.Expressions.Expression.Call(AstMethods.MakeEmptyDict);

	public IList<SliceExpression> Items => _items;

	public DictionaryExpression(params SliceExpression[] items)
	{
		_items = items;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		if (_items.Length != 0)
		{
			return ReduceConstant() ?? ReduceDictionaryWithItems();
		}
		return EmptyDictExpression;
	}

	private System.Linq.Expressions.Expression ReduceDictionaryWithItems()
	{
		System.Linq.Expressions.Expression[] array = new System.Linq.Expressions.Expression[_items.Length * 2];
		Type type = null;
		bool flag = false;
		for (int i = 0; i < _items.Length; i++)
		{
			SliceExpression sliceExpression = _items[i];
			array[i * 2] = Node.TransformOrConstantNull(sliceExpression.SliceStop, typeof(object));
			System.Linq.Expressions.Expression expression = (array[i * 2 + 1] = Node.TransformOrConstantNull(sliceExpression.SliceStart, typeof(object)));
			Type type2 = ((expression.NodeType != ExpressionType.Convert) ? expression.Type : ((System.Linq.Expressions.UnaryExpression)expression).Operand.Type);
			if (type == null)
			{
				type = type2;
			}
			else if (type2 == typeof(object))
			{
				flag = true;
			}
			else if (type2 != type)
			{
				flag = true;
			}
		}
		return System.Linq.Expressions.Expression.Call(flag ? AstMethods.MakeDictFromItems : AstMethods.MakeHomogeneousDictFromItems, System.Linq.Expressions.Expression.NewArrayInit(typeof(object), array));
	}

	private System.Linq.Expressions.Expression ReduceConstant()
	{
		for (int i = 0; i < _items.Length; i++)
		{
			SliceExpression sliceExpression = _items[i];
			if (!sliceExpression.SliceStop.IsConstant || !sliceExpression.SliceStart.IsConstant)
			{
				return null;
			}
		}
		CommonDictionaryStorage commonDictionaryStorage = new CommonDictionaryStorage();
		for (int j = 0; j < _items.Length; j++)
		{
			SliceExpression sliceExpression2 = _items[j];
			commonDictionaryStorage.AddNoLock(sliceExpression2.SliceStart.GetConstantValue(), sliceExpression2.SliceStop.GetConstantValue());
		}
		return System.Linq.Expressions.Expression.Call(AstMethods.MakeConstantDict, System.Linq.Expressions.Expression.Constant(new ConstantDictionaryStorage(commonDictionaryStorage), typeof(object)));
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this) && _items != null)
		{
			SliceExpression[] items = _items;
			foreach (SliceExpression sliceExpression in items)
			{
				sliceExpression.Walk(walker);
			}
		}
		walker.PostWalk(this);
	}

	void IInstructionProvider.AddInstructions(LightCompiler compiler)
	{
		if (_items.Length == 0)
		{
			compiler.Instructions.Emit(EmptyDictInstruction.Instance);
		}
		else
		{
			compiler.Compile(Reduce());
		}
	}
}
