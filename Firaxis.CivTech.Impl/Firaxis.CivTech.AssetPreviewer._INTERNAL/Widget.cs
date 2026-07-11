using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetPreviewer;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Reflection;

namespace Firaxis.CivTech.AssetPreviewer._INTERNAL;

public class Widget : IWidget
{
	private EventHandler _003Cbacking_store_003EOnStartEdit;

	private EventHandler _003Cbacking_store_003EOnEdit;

	private EventHandler _003Cbacking_store_003EOnFinishEdit;

	private EventHandler _003Cbacking_store_003EOnCancelEdit;

	internal bool m_bVisible;

	internal bool m_bEditing;

	internal object m_pmBoundObject;

	internal string m_pmWidgetType;

	internal WidgetManager m_pmOwner;

	internal WidgetID m_nID;

	public virtual string WidgetType => m_pmWidgetType;

	public virtual bool Visible
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return m_bVisible;
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			WidgetManager pmOwner = m_pmOwner;
			if (pmOwner != null)
			{
				pmOwner.SetWidgetVisibility(m_nID, value);
				m_bVisible = value;
			}
		}
	}

	public virtual object BoundObject
	{
		get
		{
			return m_pmBoundObject;
		}
		set
		{
			m_pmBoundObject = value;
		}
	}

	[SpecialName]
	public virtual event EventHandler OnCancelEdit
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		add
		{
			_003Cbacking_store_003EOnCancelEdit = (EventHandler)Delegate.Combine(_003Cbacking_store_003EOnCancelEdit, value);
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		remove
		{
			_003Cbacking_store_003EOnCancelEdit = (EventHandler)Delegate.Remove(_003Cbacking_store_003EOnCancelEdit, value);
		}
	}

	[SpecialName]
	public virtual event EventHandler OnFinishEdit
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		add
		{
			_003Cbacking_store_003EOnFinishEdit = (EventHandler)Delegate.Combine(_003Cbacking_store_003EOnFinishEdit, value);
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		remove
		{
			_003Cbacking_store_003EOnFinishEdit = (EventHandler)Delegate.Remove(_003Cbacking_store_003EOnFinishEdit, value);
		}
	}

	[SpecialName]
	public virtual event EventHandler OnEdit
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		add
		{
			_003Cbacking_store_003EOnEdit = (EventHandler)Delegate.Combine(_003Cbacking_store_003EOnEdit, value);
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		remove
		{
			_003Cbacking_store_003EOnEdit = (EventHandler)Delegate.Remove(_003Cbacking_store_003EOnEdit, value);
		}
	}

	[SpecialName]
	public virtual event EventHandler OnStartEdit
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		add
		{
			_003Cbacking_store_003EOnStartEdit = (EventHandler)Delegate.Combine(_003Cbacking_store_003EOnStartEdit, value);
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		remove
		{
			_003Cbacking_store_003EOnStartEdit = (EventHandler)Delegate.Remove(_003Cbacking_store_003EOnStartEdit, value);
		}
	}

	private void _007EWidget()
	{
		if (m_pmOwner != null)
		{
			byte condition = ((!m_bEditing) ? ((byte)1) : ((byte)0));
			BugSubmitter.Assert(condition != 0, "@summary Widget destroyed mid-edit @assign tmaselko");
			_0021Widget();
		}
	}

	private void _0021Widget()
	{
		m_pmOwner.DestroyWidget(m_nID);
		m_pmOwner = null;
		m_nID = (WidgetID)0u;
	}

	[SpecialName]
	protected virtual void raise_OnStartEdit(object value0, EventArgs value1)
	{
		_003Cbacking_store_003EOnStartEdit?.Invoke(value0, value1);
	}

	[SpecialName]
	protected virtual void raise_OnEdit(object value0, EventArgs value1)
	{
		_003Cbacking_store_003EOnEdit?.Invoke(value0, value1);
	}

	[SpecialName]
	protected virtual void raise_OnFinishEdit(object value0, EventArgs value1)
	{
		_003Cbacking_store_003EOnFinishEdit?.Invoke(value0, value1);
	}

	[SpecialName]
	protected virtual void raise_OnCancelEdit(object value0, EventArgs value1)
	{
		_003Cbacking_store_003EOnCancelEdit?.Invoke(value0, value1);
	}

	public virtual void Alter(IValueSet NewParametrs)
	{
		m_pmOwner.AlterWidget(m_nID, NewParametrs);
	}

	public virtual void CancelPendingEdits()
	{
		if (m_bEditing)
		{
			RaiseCancelEditEvent();
		}
	}

	internal Widget(WidgetManager pmOwner, WidgetID id, object pmObject, string pmType)
	{
		m_bVisible = true;
		m_pmBoundObject = pmObject;
		m_pmWidgetType = pmType;
		m_pmOwner = pmOwner;
		m_nID = id;
		base._002Ector();
	}

	internal void RaiseStartEditEvent()
	{
		byte condition = ((!m_bEditing) ? ((byte)1) : ((byte)0));
		BugSubmitter.Assert(condition != 0, string.Concat("@summary " + WidgetType, " raised StartEdit while editing @assign tmaselko"));
		m_bEditing = true;
		raise_OnStartEdit(this, new EventArgs());
	}

	internal void RaiseEditEvent()
	{
		BugSubmitter.Assert(m_bEditing, string.Concat("@summary " + WidgetType, " raised Edit without StartEdit @assign tmaselko"));
		raise_OnEdit(this, new EventArgs());
	}

	internal void RaiseFinishEditEvent()
	{
		BugSubmitter.Assert(m_bEditing, string.Concat("@summary " + WidgetType, " raised FinishEdit without StartEdit @assign tmaselko"));
		m_bEditing = false;
		raise_OnFinishEdit(this, new EventArgs());
	}

	internal void RaiseCancelEditEvent()
	{
		BugSubmitter.Assert(m_bEditing, string.Concat("@summary " + WidgetType, " raised CancelEdit without StartEdit @assign tmaselko"));
		m_bEditing = false;
		raise_OnCancelEdit(this, new EventArgs());
	}

	internal unsafe void Edit(sbyte* pProperty, float fValue)
	{
		string expression = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(pProperty);
		object pmBoundObject = m_pmBoundObject;
		if (pmBoundObject != null)
		{
			ReflectionHelper.SetMemberByExpression(pmBoundObject, expression, fValue);
		}
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EWidget();
			return;
		}
		try
		{
			_0021Widget();
		}
		finally
		{
			base.Finalize();
		}
	}

	public virtual sealed void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}

	~Widget()
	{
		Dispose(A_0: false);
	}
}
