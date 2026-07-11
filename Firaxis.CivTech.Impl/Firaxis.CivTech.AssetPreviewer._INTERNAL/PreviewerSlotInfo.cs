using System.Collections.Generic;
using System.Runtime.InteropServices;
using AssetPreviewer;
using Types;

namespace Firaxis.CivTech.AssetPreviewer._INTERNAL;

internal class PreviewerSlotInfo : IPreviewerSlotInfo
{
	private int m_slotID;

	private string m_XLPClass;

	private string m_DefaultAsset;

	private bool m_allowsNull;

	private bool m_isSettable;

	private List<string> m_AttachmentXLPClasses;

	public virtual IEnumerable<string> AttachmentXLPClasses => m_AttachmentXLPClasses;

	public virtual bool IsSettable
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return m_isSettable;
		}
	}

	public virtual bool AllowsNull
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return m_allowsNull;
		}
	}

	public virtual string DefaultAsset => m_DefaultAsset;

	public virtual string XLPClass => m_XLPClass;

	public virtual int SlotID => m_slotID;

	public unsafe PreviewerSlotInfo(int slotID, ModuleAssetSlotSettings* slotSettings)
	{
		//IL_0038: Expected I, but got I8
		//IL_0051: Expected I, but got I8
		//IL_007a: Expected I, but got I8
		//IL_00bc: Expected I, but got I8
		//IL_00b1: Expected I, but got I8
		m_slotID = slotID;
		m_allowsNull = *(bool*)((ulong)(nint)slotSettings + 24uL);
		m_isSettable = *(bool*)((ulong)(nint)slotSettings + 25uL);
		m_AttachmentXLPClasses = new List<string>();
		base._002Ector();
		m_XLPClass = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString((sbyte*)(*(ulong*)slotSettings));
		ulong num = *(ulong*)((ulong)(nint)slotSettings + 16uL);
		if (num != 0L)
		{
			m_DefaultAsset = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString((sbyte*)num);
		}
		else
		{
			m_DefaultAsset = string.Empty;
		}
		m_AttachmentXLPClasses.Add(m_XLPClass);
		StaticArray_003CAssetPreviewer_003A_003AAssetSlotSettings_002C10_003E* ptr = (StaticArray_003CAssetPreviewer_003A_003AAssetSlotSettings_002C10_003E*)((ulong)(nint)slotSettings + 32uL);
		AssetSlotSettings* ptr2 = global::_003CModule_003E.Types_002EStaticArray_003CAssetPreviewer_003A_003AAssetSlotSettings_002C10_003E_002Ebegin(ptr);
		AssetSlotSettings* ptr3 = global::_003CModule_003E.Types_002EStaticArray_003CAssetPreviewer_003A_003AAssetSlotSettings_002C10_003E_002Eend(ptr);
		if (ptr2 == ptr3)
		{
			return;
		}
		do
		{
			if (*(long*)((long)(nint)ptr2 + 16L - 8) != 0L)
			{
				ulong num2 = *(ulong*)ptr2;
				if (num2 != 0L && *(bool*)((ulong)(nint)ptr2 + 16uL))
				{
					m_AttachmentXLPClasses.Add(global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString((sbyte*)num2));
				}
			}
			ptr2 = (AssetSlotSettings*)((ulong)(nint)ptr2 + 24uL);
		}
		while (ptr2 != ptr3);
	}
}
