using System;
using System.Reflection;

namespace Firaxis.MVVMBase;

public class WeakAction : IWeakAction
{
	protected Action _staticAction;

	protected MethodInfo _methodInfo;

	protected WeakReference _actionTargetReference;

	public bool IsAlive
	{
		get
		{
			if (_staticAction == null)
			{
				return _methodInfo != null && _actionTargetReference.IsAlive;
			}
			return _actionTargetReference == null || _actionTargetReference.IsAlive;
		}
	}

	public WeakAction(Action action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (action.Method.IsStatic)
		{
			_staticAction = action;
		}
		else
		{
			_methodInfo = action.Method;
		}
		if (action.Target != null)
		{
			_actionTargetReference = new WeakReference(action.Target);
		}
	}

	public bool Execute(object parameter)
	{
		return Execute();
	}

	public bool Execute()
	{
		if ((_actionTargetReference != null && !_actionTargetReference.IsAlive) || (_actionTargetReference == null && _staticAction == null))
		{
			return false;
		}
		if (_staticAction != null)
		{
			_staticAction();
			return true;
		}
		if (_methodInfo == null)
		{
			return false;
		}
		WeakReference actionTargetReference = _actionTargetReference;
		if (actionTargetReference == null)
		{
			return false;
		}
		_methodInfo.Invoke(actionTargetReference.Target, new object[0]);
		return true;
	}

	public void ClearReferences()
	{
		_methodInfo = null;
		_actionTargetReference = null;
		_staticAction = null;
	}

	public bool ActionEquals(Action a)
	{
		if (a == null || (a.Method.IsStatic && _staticAction != a) || _methodInfo != a.Method)
		{
			return false;
		}
		return a.Target == null || (_actionTargetReference != null && _actionTargetReference.IsAlive && _actionTargetReference.Target == a.Target);
	}

	public bool ActionEquals(Delegate other)
	{
		Action a = other as Action;
		return (object)other != null && ActionEquals(a);
	}
}
public class WeakAction<InputType> : IWeakAction
{
	protected Action<InputType> _staticAction;

	protected MethodInfo _methodInfo;

	protected WeakReference _actionTargetReference;

	public bool IsAlive
	{
		get
		{
			if (_staticAction == null)
			{
				return _methodInfo != null && _actionTargetReference.IsAlive;
			}
			return _actionTargetReference == null || _actionTargetReference.IsAlive;
		}
	}

	public WeakAction(Action<InputType> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (action.Method.IsStatic)
		{
			_staticAction = action;
		}
		else
		{
			_methodInfo = action.Method;
		}
		if (action.Target != null)
		{
			_actionTargetReference = new WeakReference(action.Target);
		}
	}

	public bool Execute(InputType parameter)
	{
		if ((_actionTargetReference != null && !_actionTargetReference.IsAlive) || (_actionTargetReference == null && _staticAction == null))
		{
			return false;
		}
		if (_staticAction != null)
		{
			_staticAction(parameter);
			return true;
		}
		if (_methodInfo == null)
		{
			return false;
		}
		WeakReference actionTargetReference = _actionTargetReference;
		if (actionTargetReference == null)
		{
			return false;
		}
		_methodInfo.Invoke(actionTargetReference.Target, new object[1] { parameter });
		return true;
	}

	bool IWeakAction.Execute(object parameter)
	{
		return !(parameter is InputType) || Execute((InputType)parameter);
	}

	public void ClearReferences()
	{
		_methodInfo = null;
		_actionTargetReference = null;
		_staticAction = null;
	}

	public bool ActionEquals(Action<InputType> a)
	{
		if (a == null || (a.Method.IsStatic && _staticAction != a) || _methodInfo != a.Method)
		{
			return false;
		}
		return a.Target == null || (_actionTargetReference != null && _actionTargetReference.IsAlive && _actionTargetReference.Target == a.Target);
	}

	public bool ActionEquals(Delegate other)
	{
		Action<InputType> a = other as Action<InputType>;
		return (object)other != null && ActionEquals(a);
	}
}
