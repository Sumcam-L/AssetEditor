using System.Collections.Generic;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;

namespace IronPython.Modules;

public static class xxsubtype
{
	[PythonType]
	public class spamlist : List
	{
		private int _state;

		public int state
		{
			get
			{
				return _state;
			}
			set
			{
				_state = value;
			}
		}

		public spamlist()
		{
		}

		public spamlist(object sequence)
			: base(sequence)
		{
		}

		public int getstate()
		{
			return state;
		}

		public void setstate(int value)
		{
			state = value;
		}

		public static object staticmeth([ParamDictionary] IDictionary<object, object> dict, params object[] args)
		{
			return PythonTuple.MakeTuple(null, PythonTuple.MakeTuple(args), dict);
		}

		[ClassMethod]
		public static object classmeth(PythonType cls, [ParamDictionary] IDictionary<object, object> dict, params object[] args)
		{
			return PythonTuple.MakeTuple(cls, PythonTuple.MakeTuple(args), dict);
		}
	}

	[PythonType]
	public class spamdict : PythonDictionary
	{
		private int _state;

		public int state
		{
			get
			{
				return _state;
			}
			set
			{
				_state = value;
			}
		}

		public int getstate()
		{
			return state;
		}

		public void setstate(int value)
		{
			state = value;
		}
	}

	public const string __doc__ = "Provides samples on how to subtype built-in types from .NET.";

	public static double bench(CodeContext context, object x, string name)
	{
		double num = PythonTime.clock();
		for (int i = 0; i < 1001; i++)
		{
			PythonOps.GetBoundAttr(context, x, name);
		}
		return PythonTime.clock() - num;
	}
}
