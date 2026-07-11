using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AssetPreviewer;

namespace Firaxis.CivTech.AssetPreviewer;

public class StringContainerKnob : StringValueKnob, IContainerKnob<string>
{
	private List<string> m_valueCache = new List<string>();

	public virtual IEnumerable<string> Values => m_valueCache;

	public unsafe StringContainerKnob(IKnobMessenger* pMessenger, sbyte* pKnob, sbyte* pGroup)
		: base(pMessenger, pKnob, pGroup)
	{
	}

	public override Type GetValueType()
	{
		return typeof(string);
	}

	internal unsafe void SetRange(sbyte** begin, sbyte** end)
	{
		//IL_0018: Expected I, but got I8
		//IL_002f: Expected I, but got I8
		m_valueCache.Clear();
		if (begin != end)
		{
			do
			{
				IntPtr ptr = new IntPtr((void*)(*(ulong*)begin));
				m_valueCache.Add(Marshal.PtrToStringAnsi(ptr));
				begin = (sbyte**)((ulong)(nint)begin + 8uL);
			}
			while (begin != end);
		}
	}
}
