using System;
using System.Runtime.CompilerServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class SplineVertexChanged : EntityChangedEvent, ISplineVertexChanged
{
	private string m_splineName = string.Empty;

	private float[] m_position;

	private int m_vertIndex = 0;

	public virtual int VertexIndex
	{
		get
		{
			return m_vertIndex;
		}
		set
		{
			m_vertIndex = value;
		}
	}

	public virtual float[] Position
	{
		get
		{
			return m_position;
		}
		set
		{
			byte condition = (byte)(((nint)value.LongLength >= 3) ? 1 : 0);
			BugSubmitter.Assert(condition != 0, "Attachment points require position arrays of at least length 3!  @assign bwhitman");
			if ((nint)value.LongLength >= 3)
			{
				m_position = value;
			}
		}
	}

	public virtual string SplineName
	{
		get
		{
			return m_splineName;
		}
		set
		{
			m_splineName = value;
		}
	}

	public SplineVertexChanged()
	{
		SetChangeType(EntityChangeType.ECT_SPLINE_VERTEX_CHANGED);
		m_position = new float[3] { 0f, 0f, 0f };
	}

	public unsafe override void AddToChangeList(global::AssetObjects.EntityChangeList* changeList)
	{
		StandardStringWrapper standardStringWrapper = null;
		global::AssetObjects.SplineVertexChanged* ptr = AddToChangeList_Checked_003CAssetObjects_003A_003ASplineVertexChanged_003E(changeList);
		if (ptr != null)
		{
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(m_splineName);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				float[] position = m_position;
				System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector);
				global::_003CModule_003E.FGXVector3_002E_007Bctor_007D(&fGXVector, position[0], position[1], position[2]);
				global::_003CModule_003E.AssetObjects_002ESplineVertexChanged_002ESetSplineName(ptr, standardStringWrapper.Value);
				global::_003CModule_003E.AssetObjects_002ESplineVertexChanged_002ESetPosition(ptr, &fGXVector);
				global::_003CModule_003E.AssetObjects_002ESplineVertexChanged_002ESetVertexIndex(ptr, m_vertIndex);
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper).Dispose();
		}
	}
}
