using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Firaxis.CivTech.AssetObjects;

public class FloatVector3 : IFloatVector3
{
	private float[] m_values = new float[3];

	public virtual float Z
	{
		get
		{
			return m_values[2];
		}
		set
		{
			m_values[2] = value;
		}
	}

	public virtual float Y
	{
		get
		{
			return m_values[1];
		}
		set
		{
			m_values[1] = value;
		}
	}

	public virtual float X
	{
		get
		{
			return m_values[0];
		}
		set
		{
			m_values[0] = value;
		}
	}

	public FloatVector3(float x, float y, float z)
	{
		float[] values = m_values;
		values[0] = x;
		values[1] = y;
		values[2] = z;
	}

	public FloatVector3()
	{
	}

	private void _007EFloatVector3()
	{
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public virtual bool IsEqualTo(IList<float> values)
	{
		if (values.Count != 3)
		{
			return false;
		}
		int num = ((values[0] == m_values[0] && values[1] == m_values[1] && values[2] == m_values[2]) ? 1 : 0);
		return (byte)num != 0;
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
