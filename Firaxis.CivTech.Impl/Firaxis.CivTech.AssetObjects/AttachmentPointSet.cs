using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Platform;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class AttachmentPointSet : IAttachmentPointSet
{
	private IList<IAttachmentPoint> m_pmAttachmentPoints;

	private unsafe global::AssetObjects.AttachmentPointSet* m_pkAttachmentPointSet;

	private unsafe global::AssetObjects.Deserializer* m_pkDeserializer;

	public virtual IEnumerable<IAttachmentPoint> Items => m_pmAttachmentPoints;

	public unsafe virtual IAttachmentPoint FindByName(string name)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(name);
		IAttachmentPoint result;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			result = FindAttachmentPoint(global::_003CModule_003E.AssetObjects_002EAttachmentPointSet_002EFindByName(m_pkAttachmentPointSet, standardStringWrapper.Value));
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result;
	}

	private unsafe IAttachmentPoint AddAttachmentPoint(global::AssetObjects.AttachmentPoint* pkAttachment)
	{
		if (pkAttachment != null)
		{
			IAttachmentPoint attachmentPoint = FindAttachmentPoint(pkAttachment);
			if (attachmentPoint == null)
			{
				attachmentPoint = new AttachmentPoint(pkAttachment, m_pkDeserializer);
				m_pmAttachmentPoints.Add(attachmentPoint);
			}
			return attachmentPoint;
		}
		return null;
	}

	public unsafe virtual IAttachmentPoint AddAttachmentPoint(string name, string boneName, string modelInstName)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		StandardStringWrapper standardStringWrapper3 = null;
		StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(name);
		IAttachmentPoint result;
		try
		{
			standardStringWrapper = standardStringWrapper4;
			StandardStringWrapper standardStringWrapper5 = new StandardStringWrapper(boneName);
			try
			{
				standardStringWrapper2 = standardStringWrapper5;
				StandardStringWrapper standardStringWrapper6 = new StandardStringWrapper(modelInstName);
				try
				{
					standardStringWrapper3 = standardStringWrapper6;
					global::AssetObjects.AttachmentPoint* pkAttachment = global::_003CModule_003E.AssetObjects_002EAttachmentPointSet_002EAddAttachmentPoint(m_pkAttachmentPointSet, standardStringWrapper.Value, standardStringWrapper2.Value, standardStringWrapper3.Value);
					result = AddAttachmentPoint(pkAttachment);
				}
				catch
				{
					//try-fault
					((IDisposable)standardStringWrapper3).Dispose();
					throw;
				}
				((IDisposable)standardStringWrapper3).Dispose();
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
		return result;
	}

	public unsafe virtual void RemoveAttachmentPoint(string name)
	{
		global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002ERemoveAttachmentPointFromSet(m_pkAttachmentPointSet, name);
		PatchReferences();
	}

	public unsafe virtual void RemoveAttachmentPoints(IEnumerable<string> names)
	{
		foreach (string name in names)
		{
			global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002ERemoveAttachmentPointFromSet(m_pkAttachmentPointSet, name);
		}
		PatchReferences();
	}

	public unsafe AttachmentPointSet(global::AssetObjects.AttachmentPointSet* pkAttachmentPointSet, global::AssetObjects.Deserializer* pkDeserializer)
	{
		m_pmAttachmentPoints = new List<IAttachmentPoint>();
		m_pkAttachmentPointSet = pkAttachmentPointSet;
		m_pkDeserializer = pkDeserializer;
		base._002Ector();
		AddReferences();
	}

	internal unsafe void ClearAttachmentPoints()
	{
		foreach (AttachmentPoint pmAttachmentPoint in m_pmAttachmentPoints)
		{
			pmAttachmentPoint.RemoveReferences();
		}
		m_pmAttachmentPoints.Clear();
		global::AssetObjects.AttachmentPointSet* pkAttachmentPointSet = m_pkAttachmentPointSet;
		if (pkAttachmentPointSet != null)
		{
			global::_003CModule_003E.AssetObjects_002EAttachmentPointSet_002EClearAttachmentPointSet(pkAttachmentPointSet);
		}
	}

	internal void RemoveReferences()
	{
		m_pmAttachmentPoints.Clear();
	}

	private unsafe void AddReferences()
	{
		if (!global::_003CModule_003E._003FA0xe621b914_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddReferences_0040AttachmentPointSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA && m_pmAttachmentPoints.Count != 0)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DJ_0040LBMLGDPM_0040Added_003F5attachment_003F5points_003F5without_003F5_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CB_0040MOJHLCDM_0040m_pmAttachmentPoints_003F9_003F_0024DOCount_003F5_003F_0024DN_003F_0024DN_003F50_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HJ_0040PAMGFDMG_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 119u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xe621b914_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddReferences_0040AttachmentPointSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAttachmentPoint_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EAttachmentPointSet_002Ebegin(m_pkAttachmentPointSet, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAttachmentPoint_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAttachmentPoint_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EAttachmentPointSet_002Eend(m_pkAttachmentPointSet, &iterator2)))
		{
			do
			{
				AttachmentPoint item = new AttachmentPoint(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAttachmentPoint_002C4096_003E_002Eiterator_002E_002A(&iterator), m_pkDeserializer);
				m_pmAttachmentPoints.Add(item);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAttachmentPoint_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAttachmentPoint_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EAttachmentPointSet_002Eend(m_pkAttachmentPointSet, &iterator2)));
		}
	}

	private unsafe void PatchReferences()
	{
		//IL_004d: Expected I, but got I8
		//IL_00a6: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		IList<IAttachmentPoint> list = new List<IAttachmentPoint>();
		foreach (IAttachmentPoint pmAttachmentPoint in m_pmAttachmentPoints)
		{
			if (!global::_003CModule_003E._003FA0xe621b914_002E_003FbIgnoreAlways_0040_003F7_003F_003FPatchReferences_0040AttachmentPointSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA && pmAttachmentPoint == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_07NHAIKGGM_0040pmAttPt_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HJ_0040PAMGFDMG_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 134u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xe621b914_002E_003FbIgnoreAlways_0040_003F7_003F_003FPatchReferences_0040AttachmentPointSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(pmAttachmentPoint.Name);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::AssetObjects.AttachmentPoint* ptr = global::_003CModule_003E.AssetObjects_002EAttachmentPointSet_002EFindByName(m_pkAttachmentPointSet, standardStringWrapper.Value);
				if (ptr != null)
				{
					AttachmentPoint attachmentPoint = (AttachmentPoint)pmAttachmentPoint;
					if (!global::_003CModule_003E._003FA0xe621b914_002E_003FbIgnoreAlways_0040_003FBC_0040_003F_003FPatchReferences_0040AttachmentPointSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA && attachmentPoint == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_09MKNLDDCN_0040pmTypedAP_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HJ_0040PAMGFDMG_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 141u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xe621b914_002E_003FbIgnoreAlways_0040_003FBC_0040_003F_003FPatchReferences_0040AttachmentPointSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
					{
						/*OpCode not supported: DebugBreak*/;
					}
					attachmentPoint.UpdateNativeObject(ptr);
				}
				else
				{
					list.Add(pmAttachmentPoint);
				}
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper).Dispose();
		}
		foreach (IAttachmentPoint item in list)
		{
			m_pmAttachmentPoints.Remove(item);
		}
	}

	private unsafe IAttachmentPoint FindAttachmentPoint(global::AssetObjects.AttachmentPoint* pkAttachment)
	{
		if (pkAttachment != null)
		{
			foreach (IAttachmentPoint pmAttachmentPoint in m_pmAttachmentPoints)
			{
				if (((AttachmentPoint)pmAttachmentPoint).GetNativeObject() == pkAttachment)
				{
					return pmAttachmentPoint;
				}
			}
		}
		return null;
	}
}
