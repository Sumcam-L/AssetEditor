using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class AssetAnimationSetChanged : EntityChangedEvent, IAssetAnimationSetChanged
{
	private SortedSet<string> m_removedSlots = new SortedSet<string>();

	private Dictionary<string, string> m_changedSlots = new Dictionary<string, string>();

	public AssetAnimationSetChanged()
	{
		SetChangeType(EntityChangeType.ECT_ASSET_ANIMATION_SET_CHANGED);
	}

	public virtual void SlotRemoved(string pmSlotName)
	{
		if (m_changedSlots.ContainsKey(pmSlotName))
		{
			byte condition = ((!m_changedSlots.ContainsKey(pmSlotName)) ? ((byte)1) : ((byte)0));
			BugSubmitter.SilentAssert(condition != 0, "Calling 'SlotRemoved' after calling 'SlotChanged' on that slot.");
			m_changedSlots.Remove(pmSlotName);
		}
		m_removedSlots.Add(pmSlotName);
	}

	public virtual void SlotChanged(string pmSlotName, string pmAnimName)
	{
		if (m_removedSlots.Contains(pmSlotName))
		{
			byte condition = ((!m_removedSlots.Contains(pmSlotName)) ? ((byte)1) : ((byte)0));
			BugSubmitter.SilentAssert(condition != 0, "Calling 'SlotChanged' after calling 'SlotRemoved' on that slot.");
			m_removedSlots.Remove(pmSlotName);
		}
		m_changedSlots[pmSlotName] = pmAnimName;
	}

	public unsafe override void AddToChangeList(global::AssetObjects.EntityChangeList* changeList)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		StandardStringWrapper standardStringWrapper3 = null;
		if (m_changedSlots.Count == 0 && m_removedSlots.Count == 0)
		{
			if (m_changedSlots.Count == 0 && m_removedSlots.Count == 0)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GH_0040IMABCMOI_0040Attempted_003F5to_003F5add_003F5an_003F5empty_003F5asset_003F5_0040), __arglist());
				global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DJ_0040NAOHNLLL_0040m_changedSlots_003F9_003F_0024DOCount_003F5_003F_0024CB_003F_0024DN_003F50_003F5_003F_0024HM_003F_0024HM_003F5m__0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0IH_0040FPOHAHAE_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 46u);
			}
			return;
		}
		global::AssetObjects.AssetAnimationSetChanged* ptr = AddToChangeList_Checked_003CAssetObjects_003A_003AAssetAnimationSetChanged_003E(changeList);
		if (ptr == null)
		{
			return;
		}
		Dictionary<string, string>.Enumerator enumerator = m_changedSlots.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				KeyValuePair<string, string> current = enumerator.Current;
				StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(current.Key);
				try
				{
					standardStringWrapper = standardStringWrapper4;
					StandardStringWrapper standardStringWrapper5 = new StandardStringWrapper(current.Value);
					try
					{
						standardStringWrapper2 = standardStringWrapper5;
						global::_003CModule_003E.AssetObjects_002EAssetAnimationSetChanged_002ESlotChanged(ptr, standardStringWrapper.Value, standardStringWrapper2.Value);
					}
					catch
					{
						//try-fault
						((IDisposable)standardStringWrapper2).Dispose();
						throw;
					}
					((IDisposable)standardStringWrapper2).Dispose();
				}
				catch
				{
					//try-fault
					((IDisposable)standardStringWrapper).Dispose();
					throw;
				}
				((IDisposable)standardStringWrapper).Dispose();
			}
			while (enumerator.MoveNext());
		}
		SortedSet<string>.Enumerator enumerator2 = m_removedSlots.GetEnumerator();
		if (!enumerator2.MoveNext())
		{
			return;
		}
		do
		{
			StandardStringWrapper standardStringWrapper6 = new StandardStringWrapper(enumerator2.Current);
			try
			{
				standardStringWrapper3 = standardStringWrapper6;
				global::_003CModule_003E.AssetObjects_002EAssetAnimationSetChanged_002ESlotRemoved(ptr, standardStringWrapper3.Value);
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper3).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper3).Dispose();
		}
		while (enumerator2.MoveNext());
	}
}
