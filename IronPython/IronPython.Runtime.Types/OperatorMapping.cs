using System;
using System.Collections.Generic;
using IronPython.Runtime.Binding;

namespace IronPython.Runtime.Types;

internal class OperatorMapping
{
	private static readonly OperatorMapping[] _infos = MakeOperatorTable();

	private readonly PythonOperationKind _operator;

	private readonly string _name;

	private readonly string _altName;

	private readonly Type _altExpectedType;

	public PythonOperationKind Operator => _operator;

	public string Name => _name;

	public string AlternateName => _altName;

	public Type AlternateExpectedType => _altExpectedType;

	private OperatorMapping(PythonOperationKind op, string name, string altName)
	{
		_operator = op;
		_name = name;
		_altName = altName;
	}

	private OperatorMapping(PythonOperationKind op, string name, string altName, Type alternateExpectedType)
	{
		_operator = op;
		_name = name;
		_altName = altName;
		_altExpectedType = alternateExpectedType;
	}

	public static OperatorMapping GetOperatorMapping(PythonOperationKind op)
	{
		OperatorMapping[] infos = _infos;
		foreach (OperatorMapping operatorMapping in infos)
		{
			if (operatorMapping._operator == op)
			{
				return operatorMapping;
			}
		}
		return null;
	}

	public static OperatorMapping GetOperatorMapping(string name)
	{
		OperatorMapping[] infos = _infos;
		foreach (OperatorMapping operatorMapping in infos)
		{
			if (operatorMapping.Name == name || operatorMapping.AlternateName == name)
			{
				return operatorMapping;
			}
		}
		return null;
	}

	private static OperatorMapping[] MakeOperatorTable()
	{
		List<OperatorMapping> list = new List<OperatorMapping>();
		list.Add(new OperatorMapping(PythonOperationKind.Negate, "op_UnaryNegation", "Negate"));
		list.Add(new OperatorMapping(PythonOperationKind.Positive, "op_UnaryPlus", "Plus"));
		list.Add(new OperatorMapping(PythonOperationKind.Not, "op_LogicalNot", null));
		list.Add(new OperatorMapping(PythonOperationKind.OnesComplement, "op_OnesComplement", "OnesComplement"));
		list.Add(new OperatorMapping(PythonOperationKind.Add, "op_Addition", "Add"));
		list.Add(new OperatorMapping(PythonOperationKind.Subtract, "op_Subtraction", "Subtract"));
		list.Add(new OperatorMapping(PythonOperationKind.Multiply, "op_Multiply", "Multiply"));
		list.Add(new OperatorMapping(PythonOperationKind.Divide, "op_Division", "Divide"));
		list.Add(new OperatorMapping(PythonOperationKind.Mod, "op_Modulus", "Mod"));
		list.Add(new OperatorMapping(PythonOperationKind.ExclusiveOr, "op_ExclusiveOr", "ExclusiveOr"));
		list.Add(new OperatorMapping(PythonOperationKind.BitwiseAnd, "op_BitwiseAnd", "BitwiseAnd"));
		list.Add(new OperatorMapping(PythonOperationKind.BitwiseOr, "op_BitwiseOr", "BitwiseOr"));
		list.Add(new OperatorMapping(PythonOperationKind.LeftShift, "op_LeftShift", "LeftShift"));
		list.Add(new OperatorMapping(PythonOperationKind.RightShift, "op_RightShift", "RightShift"));
		list.Add(new OperatorMapping(PythonOperationKind.Equal, "op_Equality", "Equals"));
		list.Add(new OperatorMapping(PythonOperationKind.GreaterThan, "op_GreaterThan", "GreaterThan"));
		list.Add(new OperatorMapping(PythonOperationKind.LessThan, "op_LessThan", "LessThan"));
		list.Add(new OperatorMapping(PythonOperationKind.NotEqual, "op_Inequality", "NotEquals"));
		list.Add(new OperatorMapping(PythonOperationKind.GreaterThanOrEqual, "op_GreaterThanOrEqual", "GreaterThanOrEqual"));
		list.Add(new OperatorMapping(PythonOperationKind.LessThanOrEqual, "op_LessThanOrEqual", "LessThanOrEqual"));
		list.Add(new OperatorMapping(PythonOperationKind.InPlaceMultiply, "op_MultiplicationAssignment", "InPlaceMultiply"));
		list.Add(new OperatorMapping(PythonOperationKind.InPlaceSubtract, "op_SubtractionAssignment", "InPlaceSubtract"));
		list.Add(new OperatorMapping(PythonOperationKind.InPlaceExclusiveOr, "op_ExclusiveOrAssignment", "InPlaceExclusiveOr"));
		list.Add(new OperatorMapping(PythonOperationKind.InPlaceLeftShift, "op_LeftShiftAssignment", "InPlaceLeftShift"));
		list.Add(new OperatorMapping(PythonOperationKind.InPlaceRightShift, "op_RightShiftAssignment", "InPlaceRightShift"));
		list.Add(new OperatorMapping(PythonOperationKind.InPlaceMod, "op_ModulusAssignment", "InPlaceMod"));
		list.Add(new OperatorMapping(PythonOperationKind.InPlaceAdd, "op_AdditionAssignment", "InPlaceAdd"));
		list.Add(new OperatorMapping(PythonOperationKind.InPlaceBitwiseAnd, "op_BitwiseAndAssignment", "InPlaceBitwiseAnd"));
		list.Add(new OperatorMapping(PythonOperationKind.InPlaceBitwiseOr, "op_BitwiseOrAssignment", "InPlaceBitwiseOr"));
		list.Add(new OperatorMapping(PythonOperationKind.InPlaceDivide, "op_DivisionAssignment", "InPlaceDivide"));
		list.Add(new OperatorMapping(PythonOperationKind.GetItem, "get_Item", "GetItem"));
		list.Add(new OperatorMapping(PythonOperationKind.SetItem, "set_Item", "SetItem"));
		list.Add(new OperatorMapping(PythonOperationKind.DeleteItem, "del_Item", "DeleteItem"));
		list.Add(new OperatorMapping(PythonOperationKind.Compare, "op_Compare", "Compare", typeof(int)));
		list.Add(new OperatorMapping(PythonOperationKind.CallSignatures, "GetCallSignatures", null));
		list.Add(new OperatorMapping(PythonOperationKind.Documentation, "GetDocumentation", null));
		list.Add(new OperatorMapping(PythonOperationKind.IsCallable, "IsCallable", null));
		return list.ToArray();
	}
}
