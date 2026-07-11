namespace IronPython.Compiler.Ast;

internal class PythonVariable
{
	private readonly string _name;

	private readonly ScopeStatement _scope;

	private VariableKind _kind;

	private bool _deleted;

	private bool _readBeforeInitialized;

	private bool _accessedInNestedScope;

	private int _index;

	public string Name => _name;

	public bool IsGlobal
	{
		get
		{
			if (Kind != VariableKind.Global)
			{
				return Scope.IsGlobal;
			}
			return true;
		}
	}

	public ScopeStatement Scope => _scope;

	public VariableKind Kind
	{
		get
		{
			return _kind;
		}
		set
		{
			_kind = value;
		}
	}

	internal bool Deleted
	{
		get
		{
			return _deleted;
		}
		set
		{
			_deleted = value;
		}
	}

	internal int Index
	{
		get
		{
			return _index;
		}
		set
		{
			_index = value;
		}
	}

	public bool ReadBeforeInitialized
	{
		get
		{
			return _readBeforeInitialized;
		}
		set
		{
			_readBeforeInitialized = value;
		}
	}

	public bool AccessedInNestedScope
	{
		get
		{
			return _accessedInNestedScope;
		}
		set
		{
			_accessedInNestedScope = value;
		}
	}

	public PythonVariable(string name, VariableKind kind, ScopeStatement scope)
	{
		_name = name;
		_kind = kind;
		_scope = scope;
	}
}
