using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Firaxis.CivTech.AssetObjects;

public class FloatVector2 : IFloatVector2
{
	private float[] m_values;

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

	public FloatVector2(float x, float y)
	{
		m_values = new float[2];
		base._002Ector();
		float[] values = m_values;
		values[0] = x;
		values[1] = y;
	}

	public FloatVector2()
	{
		m_values = new float[3];
		base._002Ector();
	}

	private void _007EFloatVector2()
	{
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public virtual bool IsEqualTo(IList<float> values)
	{
		if (values.Count != 2)
		{
			return false;
		}
		int num = ((values[0] == m_values[0] && values[1] == m_values[1]) ? 1 : 0);
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
