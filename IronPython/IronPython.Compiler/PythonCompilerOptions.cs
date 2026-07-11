using System;
using IronPython.Runtime;
using Microsoft.Scripting;

namespace IronPython.Compiler;

[Serializable]
public sealed class PythonCompilerOptions : CompilerOptions
{
	private ModuleOptions _module;

	private bool _skipFirstLine;

	private bool _dontImplyIndent;

	private string _moduleName;

	private int[] _initialIndentation;

	private CompilationMode _compilationMode;

	public bool DontImplyDedent
	{
		get
		{
			return _dontImplyIndent;
		}
		set
		{
			_dontImplyIndent = value;
		}
	}

	public int[] InitialIndent
	{
		get
		{
			return _initialIndentation;
		}
		set
		{
			_initialIndentation = value;
		}
	}

	public bool TrueDivision
	{
		get
		{
			return (_module & ModuleOptions.TrueDivision) != 0;
		}
		set
		{
			if (value)
			{
				_module |= ModuleOptions.TrueDivision;
			}
			else
			{
				_module &= ~ModuleOptions.TrueDivision;
			}
		}
	}

	public bool AllowWithStatement
	{
		get
		{
			return (_module & ModuleOptions.WithStatement) != 0;
		}
		set
		{
			if (value)
			{
				_module |= ModuleOptions.WithStatement;
			}
			else
			{
				_module &= ~ModuleOptions.WithStatement;
			}
		}
	}

	public bool AbsoluteImports
	{
		get
		{
			return (_module & ModuleOptions.AbsoluteImports) != 0;
		}
		set
		{
			if (value)
			{
				_module |= ModuleOptions.AbsoluteImports;
			}
			else
			{
				_module &= ~ModuleOptions.AbsoluteImports;
			}
		}
	}

	public bool Verbatim
	{
		get
		{
			return (_module & ModuleOptions.Verbatim) != 0;
		}
		set
		{
			if (value)
			{
				_module |= ModuleOptions.Verbatim;
			}
			else
			{
				_module &= ~ModuleOptions.Verbatim;
			}
		}
	}

	public bool PrintFunction
	{
		get
		{
			return (_module & ModuleOptions.PrintFunction) != 0;
		}
		set
		{
			if (value)
			{
				_module |= ModuleOptions.PrintFunction;
			}
			else
			{
				_module &= ~ModuleOptions.PrintFunction;
			}
		}
	}

	public bool UnicodeLiterals
	{
		get
		{
			return (_module & ModuleOptions.UnicodeLiterals) != 0;
		}
		set
		{
			if (value)
			{
				_module |= ModuleOptions.UnicodeLiterals;
			}
			else
			{
				_module &= ~ModuleOptions.UnicodeLiterals;
			}
		}
	}

	public bool Interpreted
	{
		get
		{
			return (_module & ModuleOptions.Interpret) != 0;
		}
		set
		{
			if (value)
			{
				_module |= ModuleOptions.Interpret;
			}
			else
			{
				_module &= ~ModuleOptions.Interpret;
			}
		}
	}

	public bool Optimized
	{
		get
		{
			return (_module & ModuleOptions.Optimized) != 0;
		}
		set
		{
			if (value)
			{
				_module |= ModuleOptions.Optimized;
			}
			else
			{
				_module &= ~ModuleOptions.Optimized;
			}
		}
	}

	public ModuleOptions Module
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

	public string ModuleName
	{
		get
		{
			return _moduleName;
		}
		set
		{
			_moduleName = value;
		}
	}

	public bool SkipFirstLine
	{
		get
		{
			return _skipFirstLine;
		}
		set
		{
			_skipFirstLine = value;
		}
	}

	internal CompilationMode CompilationMode
	{
		get
		{
			return _compilationMode;
		}
		set
		{
			_compilationMode = value;
		}
	}

	public PythonCompilerOptions()
		: this(ModuleOptions.None)
	{
	}

	public PythonCompilerOptions(ModuleOptions features)
	{
		_module = features;
	}

	[Obsolete("Use the overload that takes ModuleOptions instead")]
	public PythonCompilerOptions(bool trueDivision)
	{
		TrueDivision = trueDivision;
	}
}
