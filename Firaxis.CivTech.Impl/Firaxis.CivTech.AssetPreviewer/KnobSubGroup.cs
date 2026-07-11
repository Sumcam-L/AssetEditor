using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace Firaxis.CivTech.AssetPreviewer;

public class KnobSubGroup : IKnobSubgroup
{
	private string m_name;

	private IList<IKnob> m_knobs;

	public virtual IEnumerable<IKnob> Knobs => m_knobs;

	public virtual string SubgroupName => m_name;

	public KnobSubGroup(string pmGroupName, IList<IKnob> pmKnobs)
	{
		m_name = pmGroupName;
		m_knobs = pmKnobs;
		base._002Ector();
	}

	private void _007EKnobSubGroup()
	{
	}

	private void _0021KnobSubGroup()
	{
	}

	public virtual IKnob FindKnobByName(string pmKnobName)
	{
		return global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002EFindKnobByName(pmKnobName, m_knobs);
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EKnobSubGroup();
			return;
		}
		try
		{
			_0021KnobSubGroup();
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

	~KnobSubGroup()
	{
		Dispose(A_0: false);
	}
}
