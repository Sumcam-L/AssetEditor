using System;
using System.Runtime.InteropServices;
using DDT.AnimationGraph;
using String;

namespace Firaxis.CivTech.AssetObjects;

public class AnimationGraphReader
{
	private unsafe Root* m_pkTree = null;

	private IAnimationGraphContainer m_pmContainer = null;

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe bool Load(Root* kTree, IAnimationGraphContainer pmContainer)
	{
		//IL_009a: Expected I, but got I8
		//IL_00e0: Expected I, but got I8
		//IL_00fe: Expected I, but got I8
		m_pkTree = kTree;
		m_pmContainer = pmContainer;
		pmContainer.AddRoot(*(int*)((ulong)(nint)kTree + 28uL), *(int*)((ulong)(nint)kTree + 24uL), *(int*)((ulong)(nint)kTree + 72uL), *(int*)((ulong)(nint)kTree + 120uL), *(int*)((ulong)(nint)kTree + 168uL));
		Root* pkTree = m_pkTree;
		m_pmContainer.AddOrphanRoot(*(int*)((ulong)(nint)pkTree + 220uL), *(int*)((ulong)(nint)pkTree + 216uL), *(int*)((ulong)(nint)pkTree + 264uL), *(int*)((ulong)(nint)pkTree + 312uL), *(int*)((ulong)(nint)pkTree + 360uL));
		uint num = 0u;
		long num2 = 0L;
		long num3 = 0L;
		do
		{
			pkTree = m_pkTree;
			PublishSelector(*(int*)((nint)pkTree + num2 + 28), 0, (int)num, (Selector*)(num3 + (nint)pkTree));
			num++;
			num3 += 48;
			num2 += 48;
		}
		while (num < 4);
		uint num4 = 0u;
		long num5 = 0L;
		long num6 = 192L;
		do
		{
			Root* pkTree2 = m_pkTree;
			PublishSelector(*(int*)((nint)pkTree2 + num5 + 220), 0, (int)num4, (Selector*)(num6 + (nint)pkTree2));
			num4++;
			num6 += 48;
			num5 += 48;
		}
		while (num4 < 4);
		m_pkTree = null;
		m_pmContainer = null;
		return true;
	}

	private unsafe void PublishSequence(int iParentID, int iItemInParent, Sequence* kSequence)
	{
		//IL_002c: Expected I, but got I8
		//IL_00bb: Expected I, but got I8
		//IL_014a: Expected I, but got I8
		//IL_01d8: Expected I, but got I8
		switch (*(int*)((ulong)(nint)kSequence + 16uL))
		{
		case 1:
		{
			m_pmContainer.AddAnimation(iParentID, *(int*)((ulong)(nint)kSequence + 20uL), iItemInParent);
			IntPtr ptr7 = new IntPtr(global::_003CModule_003E.String_002EBase_003C0_003E_002Ec_str((Base_003C0_003E*)((ulong)(nint)kSequence + 8uL)));
			m_pmContainer.SetAnimationID(*(int*)((ulong)(nint)kSequence + 20uL), Marshal.PtrToStringAnsi(ptr7), *(int*)((ulong)(nint)kSequence + 32uL));
			IntPtr ptr8 = new IntPtr(global::_003CModule_003E.String_002EBase_003C0_003E_002Ec_str((Base_003C0_003E*)kSequence));
			m_pmContainer.SetAnimationText(*(int*)((ulong)(nint)kSequence + 20uL), Marshal.PtrToStringAnsi(ptr8));
			m_pmContainer.SetAnimationLocation(*(int*)((ulong)(nint)kSequence + 20uL), *(int*)((ulong)(nint)kSequence + 24uL), *(int*)((ulong)(nint)kSequence + 28uL));
			break;
		}
		case 2:
		{
			m_pmContainer.AddTimeline(iParentID, *(int*)((ulong)(nint)kSequence + 20uL), iItemInParent);
			IntPtr ptr5 = new IntPtr(global::_003CModule_003E.String_002EBase_003C0_003E_002Ec_str((Base_003C0_003E*)((ulong)(nint)kSequence + 8uL)));
			m_pmContainer.SetTimelineID(*(int*)((ulong)(nint)kSequence + 20uL), Marshal.PtrToStringAnsi(ptr5), *(int*)((ulong)(nint)kSequence + 32uL));
			IntPtr ptr6 = new IntPtr(global::_003CModule_003E.String_002EBase_003C0_003E_002Ec_str((Base_003C0_003E*)kSequence));
			m_pmContainer.SetTimelineText(*(int*)((ulong)(nint)kSequence + 20uL), Marshal.PtrToStringAnsi(ptr6));
			m_pmContainer.SetTimelineLocation(*(int*)((ulong)(nint)kSequence + 20uL), *(int*)((ulong)(nint)kSequence + 24uL), *(int*)((ulong)(nint)kSequence + 28uL));
			break;
		}
		case 3:
		{
			m_pmContainer.AddMaterial(iParentID, *(int*)((ulong)(nint)kSequence + 20uL), iItemInParent);
			IntPtr ptr3 = new IntPtr(global::_003CModule_003E.String_002EBase_003C0_003E_002Ec_str((Base_003C0_003E*)((ulong)(nint)kSequence + 8uL)));
			m_pmContainer.SetMaterialID(*(int*)((ulong)(nint)kSequence + 20uL), Marshal.PtrToStringAnsi(ptr3), *(int*)((ulong)(nint)kSequence + 32uL));
			IntPtr ptr4 = new IntPtr(global::_003CModule_003E.String_002EBase_003C0_003E_002Ec_str((Base_003C0_003E*)kSequence));
			m_pmContainer.SetMaterialText(*(int*)((ulong)(nint)kSequence + 20uL), Marshal.PtrToStringAnsi(ptr4));
			m_pmContainer.SetMaterialLocation(*(int*)((ulong)(nint)kSequence + 20uL), *(int*)((ulong)(nint)kSequence + 24uL), *(int*)((ulong)(nint)kSequence + 28uL));
			break;
		}
		case 4:
		{
			m_pmContainer.AddState(iParentID, *(int*)((ulong)(nint)kSequence + 20uL), iItemInParent);
			IntPtr ptr = new IntPtr(global::_003CModule_003E.String_002EBase_003C0_003E_002Ec_str((Base_003C0_003E*)((ulong)(nint)kSequence + 8uL)));
			m_pmContainer.SetStateID(*(int*)((ulong)(nint)kSequence + 20uL), Marshal.PtrToStringAnsi(ptr), *(int*)((ulong)(nint)kSequence + 32uL));
			IntPtr ptr2 = new IntPtr(global::_003CModule_003E.String_002EBase_003C0_003E_002Ec_str((Base_003C0_003E*)kSequence));
			m_pmContainer.SetStateText(*(int*)((ulong)(nint)kSequence + 20uL), Marshal.PtrToStringAnsi(ptr2));
			m_pmContainer.SetStateLocation(*(int*)((ulong)(nint)kSequence + 20uL), *(int*)((ulong)(nint)kSequence + 24uL), *(int*)((ulong)(nint)kSequence + 28uL));
			break;
		}
		}
	}

	private unsafe void PublishSelector(int iParentID, int iItemInParent, int iLayerInParent, Selector* kSelector)
	{
		//IL_003e: Expected I, but got I8
		//IL_00c0: Expected I, but got I8
		//IL_013f: Expected I, but got I8
		//IL_020c: Expected I, but got I8
		//IL_01bb: Expected I, but got I8
		switch (iLayerInParent)
		{
		case 0:
		{
			int num3 = *(int*)((ulong)(nint)kSelector + 28uL);
			if (iParentID != num3)
			{
				m_pmContainer.AddAnimationSelector(iParentID, num3, iItemInParent, *(SelectorType*)((ulong)(nint)kSelector + 40uL), *(int*)((ulong)(nint)kSelector + 24uL));
			}
			IntPtr ptr3 = new IntPtr(global::_003CModule_003E.String_002EBase_003C0_003E_002Ec_str((Base_003C0_003E*)((ulong)(nint)kSelector + 16uL)));
			m_pmContainer.SetAnimationSelectorText(*(int*)((ulong)(nint)kSelector + 28uL), Marshal.PtrToStringAnsi(ptr3));
			m_pmContainer.SetAnimationSelectorLocation(*(int*)((ulong)(nint)kSelector + 28uL), *(int*)((ulong)(nint)kSelector + 32uL), *(int*)((ulong)(nint)kSelector + 36uL));
			break;
		}
		case 1:
		{
			int num2 = *(int*)((ulong)(nint)kSelector + 28uL);
			if (iParentID != num2)
			{
				m_pmContainer.AddTimelineSelector(iParentID, num2, iItemInParent, *(SelectorType*)((ulong)(nint)kSelector + 40uL), *(int*)((ulong)(nint)kSelector + 24uL));
			}
			IntPtr ptr2 = new IntPtr(global::_003CModule_003E.String_002EBase_003C0_003E_002Ec_str((Base_003C0_003E*)((ulong)(nint)kSelector + 16uL)));
			m_pmContainer.SetTimelineSelectorText(*(int*)((ulong)(nint)kSelector + 28uL), Marshal.PtrToStringAnsi(ptr2));
			m_pmContainer.SetTimelineSelectorLocation(*(int*)((ulong)(nint)kSelector + 28uL), *(int*)((ulong)(nint)kSelector + 32uL), *(int*)((ulong)(nint)kSelector + 36uL));
			break;
		}
		case 2:
		{
			int num4 = *(int*)((ulong)(nint)kSelector + 28uL);
			if (iParentID != num4)
			{
				m_pmContainer.AddMaterialSelector(iParentID, num4, iItemInParent, *(SelectorType*)((ulong)(nint)kSelector + 40uL), *(int*)((ulong)(nint)kSelector + 24uL));
			}
			IntPtr ptr4 = new IntPtr(global::_003CModule_003E.String_002EBase_003C0_003E_002Ec_str((Base_003C0_003E*)((ulong)(nint)kSelector + 16uL)));
			m_pmContainer.SetMaterialSelectorText(*(int*)((ulong)(nint)kSelector + 28uL), Marshal.PtrToStringAnsi(ptr4));
			m_pmContainer.SetMaterialSelectorLocation(*(int*)((ulong)(nint)kSelector + 28uL), *(int*)((ulong)(nint)kSelector + 32uL), *(int*)((ulong)(nint)kSelector + 36uL));
			break;
		}
		case 3:
		{
			int num = *(int*)((ulong)(nint)kSelector + 28uL);
			if (iParentID != num)
			{
				m_pmContainer.AddStateSelector(iParentID, num, iItemInParent, *(SelectorType*)((ulong)(nint)kSelector + 40uL), *(int*)((ulong)(nint)kSelector + 24uL));
			}
			IntPtr ptr = new IntPtr(global::_003CModule_003E.String_002EBase_003C0_003E_002Ec_str((Base_003C0_003E*)((ulong)(nint)kSelector + 16uL)));
			m_pmContainer.SetStateSelectorText(*(int*)((ulong)(nint)kSelector + 28uL), Marshal.PtrToStringAnsi(ptr));
			m_pmContainer.SetStateSelectorLocation(*(int*)((ulong)(nint)kSelector + 28uL), *(int*)((ulong)(nint)kSelector + 32uL), *(int*)((ulong)(nint)kSelector + 36uL));
			break;
		}
		}
		uint num5 = 0u;
		if (0u >= (uint)(*(int*)((ulong)(nint)kSelector + 24uL)))
		{
			return;
		}
		Selector* ptr5 = (Selector*)((ulong)(nint)kSelector + 8uL);
		do
		{
			switch (*(int*)((long)num5 * 8L + *(long*)ptr5))
			{
			case 1:
				PublishSelector(*(int*)((ulong)(nint)kSelector + 28uL), (int)num5, iLayerInParent, global::_003CModule_003E.DDT_002EAnimationGraph_002ESelector_002Eat_003Cclass_0020DDT_003A_003AAnimationGraph_003A_003ASelector_003E(kSelector, num5));
				break;
			case 2:
				PublishSequence(*(int*)((ulong)(nint)kSelector + 28uL), (int)num5, global::_003CModule_003E.DDT_002EAnimationGraph_002ESelector_002Eat_003Cclass_0020DDT_003A_003AAnimationGraph_003A_003ASequence_003E(kSelector, num5));
				break;
			}
			num5++;
		}
		while (num5 < (uint)(*(int*)((ulong)(nint)kSelector + 24uL)));
	}
}
