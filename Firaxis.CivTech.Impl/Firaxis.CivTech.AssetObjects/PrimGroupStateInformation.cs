using System;
using System.Runtime.InteropServices;

namespace Firaxis.CivTech.AssetObjects;

public class PrimGroupStateInformation : IPrimGroupStateInformation
{
	private string m_MeshName = string.Empty;

	private string m_GroupName = string.Empty;

	private string m_StateName = string.Empty;

	private IValueSet m_values = new ValueSet();

	public virtual IValueSet Values => m_values;

	public virtual string StateName
	{
		get
		{
			return m_StateName;
		}
		set
		{
			m_StateName = value;
		}
	}

	public virtual string GroupName
	{
		get
		{
			return m_GroupName;
		}
		set
		{
			m_GroupName = value;
		}
	}

	public virtual string MeshName
	{
		get
		{
			return m_MeshName;
		}
		set
		{
			m_MeshName = value;
		}
	}

	private void _007EPrimGroupStateInformation()
	{
	}

	public virtual void AssignFromPrimGroupState(IPrimGroupState state)
	{
		MeshName = state.MeshName;
		GroupName = state.GroupName;
		StateName = state.StateName;
		Values.CopyFrom(state.Values);
	}

	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (!A_0)
		{
			base.Finalize();
		}
	}

	public virtual sealed void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}
}
