using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetPreviewer;

namespace Firaxis.CivTech.AssetPreviewer;

public class Knob : IKnob, IDisposable
{
	private EventHandler _003Cbacking_store_003EHasUpdateEvent;

	internal unsafe IKnobMessenger* m_pKnobMessenger;

	private unsafe sbyte* m_pNativeKnobName;

	private unsafe sbyte* m_pNativeGroupName;

	private string m_name;

	private string m_groupName;

	private string m_subgroupName;

	private string m_categoryName;

	private string m_label;

	private string m_toolTip;

	private KnobType m_type;

	public virtual KnobType KnobType => m_type;

	public virtual string ToolTip => m_toolTip;

	public virtual string Label => m_label;

	public virtual string CategoryName => m_categoryName;

	public virtual string SubgroupName => m_subgroupName;

	public virtual string GroupName => m_groupName;

	public virtual string Name => m_name;

	[SpecialName]
	public virtual event EventHandler HasUpdateEvent
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		add
		{
			_003Cbacking_store_003EHasUpdateEvent = (EventHandler)Delegate.Combine(_003Cbacking_store_003EHasUpdateEvent, value);
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		remove
		{
			_003Cbacking_store_003EHasUpdateEvent = (EventHandler)Delegate.Remove(_003Cbacking_store_003EHasUpdateEvent, value);
		}
	}

	[SpecialName]
	protected virtual void raise_HasUpdateEvent(object value0, EventArgs value1)
	{
		_003Cbacking_store_003EHasUpdateEvent?.Invoke(value0, value1);
	}

	public unsafe Knob(KnobType knobType, IKnobMessenger* pMessenger, sbyte* pKnobName, sbyte* pKnobGroup)
	{
		m_pKnobMessenger = pMessenger;
		m_pNativeKnobName = (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_00CNPNBAHC_0040_003F_0024AA_0040);
		m_pNativeGroupName = (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_00CNPNBAHC_0040_003F_0024AA_0040);
		m_subgroupName = string.Empty;
		m_categoryName = "Knobs";
		m_label = string.Empty;
		m_toolTip = string.Empty;
		m_type = knobType;
		base._002Ector();
		sbyte* ptr = (sbyte*)global::_003CModule_003E.Platform_002EMalloc(global::_003CModule_003E.Platform_002Estrlen(pKnobGroup) + 1, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GH_0040HAGOEALE_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 90, 23, 0);
		sbyte* ptr2 = (sbyte*)global::_003CModule_003E.Platform_002EMalloc(global::_003CModule_003E.Platform_002Estrlen(pKnobName) + 1, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GH_0040HAGOEALE_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 91, 23, 0);
		global::_003CModule_003E.Platform_002Estrcpy(ptr, pKnobGroup);
		global::_003CModule_003E.Platform_002Estrcpy(ptr2, pKnobName);
		m_pNativeGroupName = ptr;
		m_pNativeKnobName = ptr2;
		IntPtr ptr3 = new IntPtr(pKnobName);
		m_name = Marshal.PtrToStringAnsi(ptr3);
		IntPtr ptr4 = new IntPtr(pKnobGroup);
		m_groupName = Marshal.PtrToStringAnsi(ptr4);
	}

	private unsafe void _007EKnob()
	{
		global::_003CModule_003E.Platform_002EFree(m_pNativeKnobName);
		global::_003CModule_003E.Platform_002EFree(m_pNativeGroupName);
	}

	internal void RaiseHasUpdateEvent()
	{
		raise_HasUpdateEvent(this, EventArgs.Empty);
	}

	internal unsafe sbyte* GetNativeKnobName()
	{
		return m_pNativeKnobName;
	}

	internal unsafe sbyte* GetNativeGroupName()
	{
		return m_pNativeGroupName;
	}

	internal unsafe void SetLabel(sbyte* pLabel)
	{
		IntPtr ptr = new IntPtr(pLabel);
		m_label = Marshal.PtrToStringAnsi(ptr);
	}

	internal unsafe void SetSubGroup(sbyte* pSubGroup)
	{
		IntPtr ptr = new IntPtr(pSubGroup);
		m_subgroupName = Marshal.PtrToStringAnsi(ptr);
	}

	internal unsafe void SetToolTip(sbyte* pToolTip)
	{
		IntPtr ptr = new IntPtr(pToolTip);
		m_toolTip = Marshal.PtrToStringAnsi(ptr);
	}

	internal unsafe void SetCategory(sbyte* pCategory)
	{
		IntPtr ptr = new IntPtr(pCategory);
		m_categoryName = Marshal.PtrToStringAnsi(ptr);
	}

	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EKnob();
		}
		else
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
