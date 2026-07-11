using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;

namespace IronPython.Runtime.Types;

[PythonType("event#")]
public sealed class ReflectedEvent : PythonTypeDataSlot, ICodeFormattable
{
	public class BoundEvent
	{
		private readonly ReflectedEvent _event;

		private readonly PythonType _ownerType;

		private readonly object _instance;

		public ReflectedEvent Event => _event;

		public BoundEvent(ReflectedEvent reflectedEvent, object instance, PythonType ownerType)
		{
			_event = reflectedEvent;
			_instance = instance;
			_ownerType = ownerType;
		}

		[SpecialName]
		public object op_AdditionAssignment(CodeContext context, object func)
		{
			return InPlaceAdd(context, func);
		}

		[SpecialName]
		public object InPlaceAdd(CodeContext context, object func)
		{
			if (func == null || !PythonOps.IsCallable(context, func))
			{
				throw PythonOps.TypeError("event addition expected callable object, got {0}", PythonTypeOps.GetName(func));
			}
			if (_event.Tracker.IsStatic && _ownerType != DynamicHelpers.GetPythonTypeFromType(_event.Tracker.DeclaringType))
			{
				return new BadEventChange(_ownerType, _instance);
			}
			MethodInfo methodInfo = _event.Tracker.GetCallableAddMethod();
			if (_instance != null)
			{
				methodInfo = CompilerHelpers.TryGetCallableMethod(_instance.GetType(), methodInfo);
			}
			if (CompilerHelpers.IsVisible(methodInfo) || context.LanguageContext.DomainManager.Configuration.PrivateBinding)
			{
				_event.Tracker.AddHandler(_instance, func, context.LanguageContext.DelegateCreator);
				return this;
			}
			throw new TypeErrorException("Cannot add handler to a private event.");
		}

		[SpecialName]
		public object InPlaceSubtract(CodeContext context, object func)
		{
			if (func == null)
			{
				throw PythonOps.TypeError("event subtraction expected callable object, got None");
			}
			if (_event.Tracker.IsStatic && _ownerType != DynamicHelpers.GetPythonTypeFromType(_event.Tracker.DeclaringType))
			{
				return new BadEventChange(_ownerType, _instance);
			}
			MethodInfo callableRemoveMethod = _event.Tracker.GetCallableRemoveMethod();
			if (CompilerHelpers.IsVisible(callableRemoveMethod) || context.LanguageContext.DomainManager.Configuration.PrivateBinding)
			{
				_event.Tracker.RemoveHandler(_instance, func, PythonContext.GetContext(context).EqualityComparer);
				return this;
			}
			throw new TypeErrorException("Cannot add handler to a private event.");
		}
	}

	private class BadEventChange
	{
		private readonly PythonType _ownerType;

		private readonly object _instance;

		public PythonType Owner => _ownerType;

		public object Instance => _instance;

		public BadEventChange(PythonType ownerType, object instance)
		{
			_ownerType = ownerType;
			_instance = instance;
		}
	}

	private readonly bool _clsOnly;

	private readonly EventTracker _tracker;

	internal override bool GetAlwaysSucceeds => true;

	internal override bool IsAlwaysVisible => !_clsOnly;

	public string __doc__ => DocBuilder.CreateAutoDoc(_tracker.Event);

	public EventInfo Info
	{
		[PythonHidden]
		get
		{
			return _tracker.Event;
		}
	}

	public EventTracker Tracker
	{
		[PythonHidden]
		get
		{
			return _tracker;
		}
	}

	internal ReflectedEvent(EventTracker tracker, bool clsOnly)
	{
		_clsOnly = clsOnly;
		_tracker = tracker;
	}

	internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
	{
		value = new BoundEvent(this, instance, owner);
		return true;
	}

	internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value)
	{
		if (!(value is BoundEvent et) || EventInfosDiffer(et))
		{
			if (value is BadEventChange { Owner: { } owner2 } badEventChange)
			{
				if (badEventChange.Instance == null)
				{
					throw new MissingMemberException(string.Format("attribute '{1}' of '{0}' object is read-only", owner2.Name, _tracker.Name));
				}
				throw new MissingMemberException($"'{owner2.Name}' object has no attribute '{_tracker.Name}'");
			}
			throw ReadOnlyException(DynamicHelpers.GetPythonTypeFromType(Info.DeclaringType));
		}
		return true;
	}

	private bool EventInfosDiffer(BoundEvent et)
	{
		if (et.Event.Info == Info)
		{
			return false;
		}
		if (et.Event.Info.DeclaringType != Info.DeclaringType || et.Event.Info.MetadataToken != Info.MetadataToken)
		{
			return true;
		}
		return false;
	}

	internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner)
	{
		throw ReadOnlyException(DynamicHelpers.GetPythonTypeFromType(Info.DeclaringType));
	}

	private MissingMemberException ReadOnlyException(PythonType dt)
	{
		return new MissingMemberException(string.Format("attribute '{1}' of '{0}' object is read-only", dt.Name, _tracker.Name));
	}

	public string __repr__(CodeContext context)
	{
		return $"<event# {Info.Name} on {Info.DeclaringType.Name}>";
	}
}
