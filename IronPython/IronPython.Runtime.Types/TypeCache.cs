using System;
using System.Numerics;
using IronPython.Modules;
using IronPython.Runtime.Exceptions;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Types;

public static class TypeCache
{
	private static PythonType array;

	private static PythonType builtinfunction;

	private static PythonType pythondictionary;

	private static PythonType frozensetcollection;

	private static PythonType pythonfunction;

	private static PythonType builtin;

	private static PythonType obj;

	private static PythonType setcollection;

	private static PythonType pythontype;

	private static PythonType str;

	private static PythonType pythontuple;

	private static PythonType weakreference;

	private static PythonType list;

	private static PythonType pythonfile;

	private static PythonType pythonmodule;

	private static PythonType method;

	private static PythonType enumerate;

	private static PythonType intType;

	private static PythonType singleType;

	private static PythonType doubleType;

	private static PythonType biginteger;

	private static PythonType complex;

	private static PythonType super;

	private static PythonType oldclass;

	private static PythonType oldinstance;

	private static PythonType nullType;

	private static PythonType boolType;

	private static PythonType baseException;

	public static PythonType Array
	{
		get
		{
			if (array == null)
			{
				array = DynamicHelpers.GetPythonTypeFromType(typeof(Array));
			}
			return array;
		}
	}

	public static PythonType BuiltinFunction
	{
		get
		{
			if (builtinfunction == null)
			{
				builtinfunction = DynamicHelpers.GetPythonTypeFromType(typeof(BuiltinFunction));
			}
			return builtinfunction;
		}
	}

	public static PythonType Dict
	{
		get
		{
			if (pythondictionary == null)
			{
				pythondictionary = DynamicHelpers.GetPythonTypeFromType(typeof(PythonDictionary));
			}
			return pythondictionary;
		}
	}

	public static PythonType FrozenSet
	{
		get
		{
			if (frozensetcollection == null)
			{
				frozensetcollection = DynamicHelpers.GetPythonTypeFromType(typeof(FrozenSetCollection));
			}
			return frozensetcollection;
		}
	}

	public static PythonType Function
	{
		get
		{
			if (pythonfunction == null)
			{
				pythonfunction = DynamicHelpers.GetPythonTypeFromType(typeof(PythonFunction));
			}
			return pythonfunction;
		}
	}

	public static PythonType Builtin
	{
		get
		{
			if (builtin == null)
			{
				builtin = DynamicHelpers.GetPythonTypeFromType(typeof(Builtin));
			}
			return builtin;
		}
	}

	public static PythonType Object
	{
		get
		{
			if (obj == null)
			{
				obj = DynamicHelpers.GetPythonTypeFromType(typeof(object));
			}
			return obj;
		}
	}

	public static PythonType Set
	{
		get
		{
			if (setcollection == null)
			{
				setcollection = DynamicHelpers.GetPythonTypeFromType(typeof(SetCollection));
			}
			return setcollection;
		}
	}

	public static PythonType PythonType
	{
		get
		{
			if (pythontype == null)
			{
				pythontype = DynamicHelpers.GetPythonTypeFromType(typeof(PythonType));
			}
			return pythontype;
		}
	}

	public static PythonType String
	{
		get
		{
			if (str == null)
			{
				str = DynamicHelpers.GetPythonTypeFromType(typeof(string));
			}
			return str;
		}
	}

	public static PythonType PythonTuple
	{
		get
		{
			if (pythontuple == null)
			{
				pythontuple = DynamicHelpers.GetPythonTypeFromType(typeof(PythonTuple));
			}
			return pythontuple;
		}
	}

	public static PythonType WeakReference
	{
		get
		{
			if (weakreference == null)
			{
				weakreference = DynamicHelpers.GetPythonTypeFromType(typeof(WeakReference));
			}
			return weakreference;
		}
	}

	public static PythonType List
	{
		get
		{
			if (list == null)
			{
				list = DynamicHelpers.GetPythonTypeFromType(typeof(List));
			}
			return list;
		}
	}

	public static PythonType PythonFile
	{
		get
		{
			if (pythonfile == null)
			{
				pythonfile = DynamicHelpers.GetPythonTypeFromType(typeof(PythonFile));
			}
			return pythonfile;
		}
	}

	public static PythonType Module
	{
		get
		{
			if (pythonmodule == null)
			{
				pythonmodule = DynamicHelpers.GetPythonTypeFromType(typeof(PythonModule));
			}
			return pythonmodule;
		}
	}

	public static PythonType Method
	{
		get
		{
			if (method == null)
			{
				method = DynamicHelpers.GetPythonTypeFromType(typeof(Method));
			}
			return method;
		}
	}

	public static PythonType Enumerate
	{
		get
		{
			if (enumerate == null)
			{
				enumerate = DynamicHelpers.GetPythonTypeFromType(typeof(Enumerate));
			}
			return enumerate;
		}
	}

	public static PythonType Int32
	{
		get
		{
			if (intType == null)
			{
				intType = DynamicHelpers.GetPythonTypeFromType(typeof(int));
			}
			return intType;
		}
	}

	public static PythonType Single
	{
		get
		{
			if (singleType == null)
			{
				singleType = DynamicHelpers.GetPythonTypeFromType(typeof(float));
			}
			return singleType;
		}
	}

	public static PythonType Double
	{
		get
		{
			if (doubleType == null)
			{
				doubleType = DynamicHelpers.GetPythonTypeFromType(typeof(double));
			}
			return doubleType;
		}
	}

	public static PythonType BigInteger
	{
		get
		{
			if (biginteger == null)
			{
				biginteger = DynamicHelpers.GetPythonTypeFromType(typeof(BigInteger));
			}
			return biginteger;
		}
	}

	public static PythonType Complex
	{
		get
		{
			if (complex == null)
			{
				complex = DynamicHelpers.GetPythonTypeFromType(typeof(Complex));
			}
			return complex;
		}
	}

	public static PythonType Super
	{
		get
		{
			if (super == null)
			{
				super = DynamicHelpers.GetPythonTypeFromType(typeof(Super));
			}
			return super;
		}
	}

	public static PythonType OldClass
	{
		get
		{
			if (oldclass == null)
			{
				oldclass = DynamicHelpers.GetPythonTypeFromType(typeof(OldClass));
			}
			return oldclass;
		}
	}

	public static PythonType OldInstance
	{
		get
		{
			if (oldinstance == null)
			{
				oldinstance = DynamicHelpers.GetPythonTypeFromType(typeof(OldInstance));
			}
			return oldinstance;
		}
	}

	public static PythonType Null
	{
		get
		{
			if (nullType == null)
			{
				nullType = DynamicHelpers.GetPythonTypeFromType(typeof(DynamicNull));
			}
			return nullType;
		}
	}

	public static PythonType Boolean
	{
		get
		{
			if (boolType == null)
			{
				boolType = DynamicHelpers.GetPythonTypeFromType(typeof(bool));
			}
			return boolType;
		}
	}

	public static PythonType BaseException
	{
		get
		{
			if (baseException == null)
			{
				baseException = DynamicHelpers.GetPythonTypeFromType(typeof(PythonExceptions.BaseException));
			}
			return baseException;
		}
	}

	[Obsolete("use Complex instead")]
	public static PythonType Complex64 => Complex;
}
