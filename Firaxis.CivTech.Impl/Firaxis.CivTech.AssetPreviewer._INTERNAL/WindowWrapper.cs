using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AssetPreviewer;
using Firaxis.CivTech.AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetPreviewer._INTERNAL;

public class WindowWrapper : IPreviewWindow
{
	internal WidgetManager m_pmWidgetManager;

	private unsafe IPreviewer* m_previewer;

	private unsafe IPreviewModule* m_pPreviewModule;

	private WindowID m_windowID;

	private string m_previewModule;

	private KnobManager m_pmKnobManager;

	private List<AssetPreviewPack> m_openedAssets;

	private List<AssetPreviewPack> m_pmAssetsToClose;

	private IXLPRegistry m_pmXLPRegistry;

	private ICivTechService m_pmCivTechSvc;

	private IInstanceSet m_pmInstanceSet;

	private readonly Dictionary<int, IKnobSet> m_openedKnobSets;

	private IVirtualPantry VirtualPantry => m_pmCivTechSvc.ProjectMapService.LayeredPantry;

	public KnobManager KnobManagerProp
	{
		get
		{
			return m_pmKnobManager;
		}
		set
		{
			m_pmKnobManager = value;
		}
	}

	public string PreviewModule => m_previewModule;

	public virtual string PreviewModuleName => m_previewModule;

	public virtual string AssetName
	{
		get
		{
			AssetPreviewPack packInSlot = GetPackInSlot(0);
			if (packInSlot != null)
			{
				return packInSlot.Asset.Name;
			}
			return string.Empty;
		}
	}

	public unsafe WindowWrapper(WindowID pWin, IPreviewer* pPreviewer, ICivTechService pmCivTechSvc, IXLPRegistry pmXLPRegistry, IInstanceSet pmInstanceSet)
	{
		//IL_000f: Expected I, but got I8
		//IL_0086: Expected I, but got I8
		m_previewer = pPreviewer;
		m_pPreviewModule = null;
		m_windowID = pWin;
		m_previewModule = "None";
		m_openedAssets = new List<AssetPreviewPack>(10);
		m_pmAssetsToClose = new List<AssetPreviewPack>(10);
		m_pmXLPRegistry = pmXLPRegistry;
		m_pmCivTechSvc = pmCivTechSvc;
		m_pmInstanceSet = pmInstanceSet;
		m_openedKnobSets = new Dictionary<int, IKnobSet>();
		base._002Ector();
		if (!global::_003CModule_003E._003FA0xbaea9428_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0WindowWrapper_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040W4WindowID_00403_0040PEAVIPreviewer_00403_0040PE_0024AAUICivTechService_004045_0040PE_0024AAUIXLPRegistry_004045_0040PE_0024AAUIInstanceSet_0040AssetObjects_004045_0040_0040Z_00404_NA && pWin == (WindowID)0u && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DA_0040CEENFMBO_0040pWin_003F5_003F_0024CB_003F_0024DN_003F5_003F3_003F3AssetPreviewer_003F3_003F3Window_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GK_0040JEEFPBNK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 48u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xbaea9428_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0WindowWrapper_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040W4WindowID_00403_0040PEAVIPreviewer_00403_0040PE_0024AAUICivTechService_004045_0040PE_0024AAUIXLPRegistry_004045_0040PE_0024AAUIInstanceSet_0040AssetObjects_004045_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
	}

	private void _007EWindowWrapper()
	{
		RemoveReferences();
	}

	private void _0021WindowWrapper()
	{
		RemoveReferences();
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool SetPreviewModule(string pmModuleName)
	{
		//IL_005b: Expected I, but got I8
		//IL_0084: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		if (m_previewer == null)
		{
			return false;
		}
		bool result = false;
		if (m_previewModule != pmModuleName)
		{
			m_openedKnobSets.Clear();
			UnbindAssetsFromWindow();
			result = true;
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(pmModuleName);
		try
		{
			standardStringWrapper = standardStringWrapper2;
			m_previewModule = pmModuleName;
			IPreviewer* previewer = m_previewer;
			if ((m_pPreviewModule = ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*, IPreviewModule*>)(*(ulong*)(*(long*)previewer + 112)))((nint)previewer, standardStringWrapper.Value)) != null)
			{
				previewer = m_previewer;
				((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowID, sbyte*, void>)(*(ulong*)(*(ulong*)previewer)))((nint)previewer, m_windowID, standardStringWrapper.Value);
			}
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

	public virtual string GetPreviewModule()
	{
		return m_previewModule;
	}

	public unsafe virtual IEnumerable<IPreviewerSlotInfo> GetSlotsInfo()
	{
		if (m_previewModule != null && m_previewer != null)
		{
			try
			{
				return global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002EGetSlotsInfo(m_previewer, m_previewModule);
			}
			catch (Exception)
			{
				return Enumerable.Empty<IPreviewerSlotInfo>();
			}
		}
		return Enumerable.Empty<IPreviewerSlotInfo>();
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public virtual bool IsUnbound()
	{
		return m_openedAssets.Count == 0;
	}

	public unsafe virtual void OpenAsset(IInstanceEntity pmAsset, int slotID, [MarshalAs(UnmanagedType.U1)] bool delayBind)
	{
		if (m_previewer == null || m_windowID == (WindowID)0u)
		{
			return;
		}
		AssetPreviewPack packInSlot = GetPackInSlot(slotID);
		if (packInSlot == null || packInSlot.Asset != pmAsset)
		{
			CloseRootAsset(slotID);
			if (pmAsset != null)
			{
				AssetPreviewPack item = new AssetPreviewPack(m_previewModule, m_previewer, m_pmCivTechSvc, m_pmXLPRegistry, m_pmInstanceSet, m_windowID, slotID, pmAsset);
				m_openedAssets.Add(item);
			}
		}
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public bool CloseRootAsset(int slotID)
	{
		AssetPreviewPack packInSlot = GetPackInSlot(slotID);
		if (packInSlot == null)
		{
			return false;
		}
		m_openedAssets.Remove(packInSlot);
		packInSlot.RemoveReferences();
		return true;
	}

	public virtual void ClearSlot(int slotID, [MarshalAs(UnmanagedType.U1)] bool delayBind)
	{
		AssetPreviewPack packInSlot = GetPackInSlot(slotID);
		if (packInSlot != null)
		{
			m_openedAssets.Remove(packInSlot);
			packInSlot.RemoveReferences();
			if (!delayBind)
			{
				BindAssetsToWindow();
			}
		}
	}

	public virtual void AddAttachment(IInstanceEntity attachedAsset, IAttachmentPointData attachmentData, int slotID, [MarshalAs(UnmanagedType.U1)] bool delayBind)
	{
		AssetPreviewPack packInSlot = GetPackInSlot(slotID);
		if (attachedAsset != null && packInSlot != null && packInSlot.AddKnobAttachment(attachedAsset, attachmentData) && !delayBind)
		{
			BindAssetsToWindow();
		}
	}

	public virtual IEnumerable<string> GetAttachmentOverrides(string attachmentPointName, InstanceType attachedAssetType, int slotID)
	{
		AssetPreviewPack packInSlot = GetPackInSlot(slotID);
		if (packInSlot != null && !string.IsNullOrEmpty(attachmentPointName))
		{
			return packInSlot.GetKnobAttachments(attachmentPointName, attachedAssetType);
		}
		return Enumerable.Empty<string>();
	}

	public virtual void RemoveAttachment(string attachmentPointName, string attachedAssetName, InstanceType attachedAssetType, int slotID, [MarshalAs(UnmanagedType.U1)] bool delayBind)
	{
		AssetPreviewPack packInSlot = GetPackInSlot(slotID);
		if (packInSlot != null && !string.IsNullOrEmpty(attachmentPointName) && !string.IsNullOrEmpty(attachedAssetName) && packInSlot.RemoveKnobAttachment(attachmentPointName, attachedAssetName, attachedAssetType) && !delayBind)
		{
			BindAssetsToWindow();
		}
	}

	public unsafe virtual void BindAssetsToWindow()
	{
		//IL_0025: Expected I, but got I8
		WindowID windowID = m_windowID;
		if (windowID != 0)
		{
			IPreviewer* previewer = m_previewer;
			if (previewer != null)
			{
				((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowID, void>)(*(ulong*)(*(long*)previewer + 8)))((nint)previewer, windowID);
			}
		}
	}

	public virtual void UpdateAsset(IEnumerable<IEntityChangedEvent> pmChanges, int slotID)
	{
		if (pmChanges.Any())
		{
			AssetPreviewPack packInSlot = GetPackInSlot(slotID);
			if (m_windowID != 0 && packInSlot != null && packInSlot.ApplyChanges(pmChanges))
			{
				BindAssetsToWindow();
			}
		}
	}

	public virtual void ForceRefreshAsset(int slotID)
	{
		GetPackInSlot(slotID)?.ReloadAssets();
	}

	public void Close()
	{
		if (m_windowID != 0)
		{
			RemoveReferences();
		}
	}

	public virtual IKnobSet GetPreviewModuleKnobSet()
	{
		object result = null;
		KnobManager pmKnobManager = m_pmKnobManager;
		if (pmKnobManager != null)
		{
			result = pmKnobManager.GetKnobSet(m_previewModule);
		}
		return (IKnobSet)result;
	}

	public virtual IKnobSet GetEntityKnobSet(IInstanceEntity entity, int slotID)
	{
		IKnobSet knobSet = null;
		AssetPreviewPack packInSlot = GetPackInSlot(0);
		if (m_openedKnobSets.ContainsKey(slotID))
		{
			m_openedKnobSets.Remove(slotID);
		}
		if (m_pmKnobManager != null && packInSlot != null)
		{
			knobSet = m_pmKnobManager.GetEntityKnobSet(packInSlot.Asset.Name, entity.Name, slotID);
		}
		if (knobSet != null)
		{
			m_openedKnobSets[slotID] = knobSet;
		}
		return knobSet;
	}

	public unsafe virtual void OnMouseDown(System.Windows.Forms.MouseButtons button)
	{
		//IL_0044: Expected I, but got I8
		//IL_0056: Expected I, but got I8
		if (m_windowID != 0 && !IsUnbound())
		{
			IPreviewer* previewer = m_previewer;
			if (previewer != null)
			{
				global::AssetPreviewer.MouseButtons mouseButtons = ((button != System.Windows.Forms.MouseButtons.Left) ? ((button == System.Windows.Forms.MouseButtons.Right) ? ((global::AssetPreviewer.MouseButtons)1u) : ((button != System.Windows.Forms.MouseButtons.Middle) ? ((global::AssetPreviewer.MouseButtons)3u) : ((global::AssetPreviewer.MouseButtons)2u))) : ((global::AssetPreviewer.MouseButtons)0u));
				IPreviewer* ptr = (IPreviewer*)((ulong)(nint)previewer + 8uL);
				((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowID, global::AssetPreviewer.MouseButtons, void>)(*(ulong*)(*(ulong*)ptr)))((nint)ptr, m_windowID, mouseButtons);
			}
		}
	}

	public unsafe virtual void OnMouseUp(System.Windows.Forms.MouseButtons button)
	{
		//IL_0044: Expected I, but got I8
		//IL_0059: Expected I, but got I8
		if (m_windowID != 0 && !IsUnbound())
		{
			IPreviewer* previewer = m_previewer;
			if (previewer != null)
			{
				global::AssetPreviewer.MouseButtons mouseButtons = ((button != System.Windows.Forms.MouseButtons.Left) ? ((button == System.Windows.Forms.MouseButtons.Right) ? ((global::AssetPreviewer.MouseButtons)1u) : ((button != System.Windows.Forms.MouseButtons.Middle) ? ((global::AssetPreviewer.MouseButtons)3u) : ((global::AssetPreviewer.MouseButtons)2u))) : ((global::AssetPreviewer.MouseButtons)0u));
				IPreviewer* ptr = (IPreviewer*)((ulong)(nint)previewer + 8uL);
				((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowID, global::AssetPreviewer.MouseButtons, void>)(*(ulong*)(*(long*)ptr + 8)))((nint)ptr, m_windowID, mouseButtons);
			}
		}
	}

	public unsafe virtual void OnMouseDoubleClick(System.Windows.Forms.MouseButtons button)
	{
		//IL_0044: Expected I, but got I8
		//IL_005a: Expected I, but got I8
		if (m_windowID != 0 && !IsUnbound())
		{
			IPreviewer* previewer = m_previewer;
			if (previewer != null)
			{
				global::AssetPreviewer.MouseButtons mouseButtons = ((button != System.Windows.Forms.MouseButtons.Left) ? ((button == System.Windows.Forms.MouseButtons.Right) ? ((global::AssetPreviewer.MouseButtons)1u) : ((button != System.Windows.Forms.MouseButtons.Middle) ? ((global::AssetPreviewer.MouseButtons)3u) : ((global::AssetPreviewer.MouseButtons)2u))) : ((global::AssetPreviewer.MouseButtons)0u));
				IPreviewer* ptr = (IPreviewer*)((ulong)(nint)previewer + 8uL);
				((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowID, global::AssetPreviewer.MouseButtons, void>)(*(ulong*)(*(long*)ptr + 32)))((nint)ptr, m_windowID, mouseButtons);
			}
		}
	}

	public unsafe virtual void OnMouseWheel(int wheelDelta)
	{
		//IL_001f: Expected I, but got I8
		//IL_0033: Expected I, but got I8
		if (m_windowID != 0 && !IsUnbound())
		{
			IPreviewer* previewer = m_previewer;
			if (previewer != null)
			{
				IPreviewer* ptr = (IPreviewer*)((ulong)(nint)previewer + 8uL);
				((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowID, uint, void>)(*(ulong*)(*(long*)ptr + 16)))((nint)ptr, m_windowID, (uint)wheelDelta);
			}
		}
	}

	public unsafe virtual void OnMouseMove(int mouseX, int mouseY)
	{
		//IL_001f: Expected I, but got I8
		//IL_0034: Expected I, but got I8
		if (m_windowID != 0 && !IsUnbound())
		{
			IPreviewer* previewer = m_previewer;
			if (previewer != null)
			{
				IPreviewer* ptr = (IPreviewer*)((ulong)(nint)previewer + 8uL);
				((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowID, uint, uint, void>)(*(ulong*)(*(long*)ptr + 24)))((nint)ptr, m_windowID, (uint)mouseX, (uint)mouseY);
			}
		}
	}

	public unsafe virtual void OnKeyDown(int keyValue)
	{
		//IL_001f: Expected I, but got I8
		//IL_0033: Expected I, but got I8
		if (m_windowID != 0 && !IsUnbound())
		{
			IPreviewer* previewer = m_previewer;
			if (previewer != null)
			{
				IPreviewer* ptr = (IPreviewer*)((ulong)(nint)previewer + 8uL);
				((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowID, int, void>)(*(ulong*)(*(long*)ptr + 40)))((nint)ptr, m_windowID, keyValue);
			}
		}
	}

	public unsafe virtual void OnKeyUp(int keyValue)
	{
		//IL_001f: Expected I, but got I8
		//IL_0033: Expected I, but got I8
		if (m_windowID != 0 && !IsUnbound())
		{
			IPreviewer* previewer = m_previewer;
			if (previewer != null)
			{
				IPreviewer* ptr = (IPreviewer*)((ulong)(nint)previewer + 8uL);
				((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowID, int, void>)(*(ulong*)(*(long*)ptr + 48)))((nint)ptr, m_windowID, keyValue);
			}
		}
	}

	public unsafe virtual IEnumerable<PickResult> PickPoint(int x, int y, float slack)
	{
		//IL_0020: Expected I, but got I8
		//IL_0035: Expected I, but got I8
		System.Runtime.CompilerServices.Unsafe.SkipInit(out PointQueryWrapper pointQueryWrapper);
		global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002EPointQueryWrapper_002E_007Bctor_007D(&pointQueryWrapper, x, y, slack, m_pmWidgetManager);
		IEnumerable<PickResult> result;
		try
		{
			IPreviewer* previewer = m_previewer;
			if (previewer != null)
			{
				IPreviewer* ptr = (IPreviewer*)((ulong)(nint)previewer + 8uL);
				((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowID, IPointPickingQuery*, void>)(*(ulong*)(*(long*)ptr + 56)))((nint)ptr, m_windowID, (IPointPickingQuery*)(&pointQueryWrapper));
			}
			result = global::_003CModule_003E.gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003AIList_003CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003APickResult_0020_005E_003E_0020_005E_003E_002E_002EPE_0024AAU_003F_0024IList_0040PE_0024AAVPickResult_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040_0040Generic_0040Collections_0040System_0040_0040((gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003AIList_003CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003APickResult_0020_005E_003E_0020_005E_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref pointQueryWrapper, 8)));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<PointQueryWrapper*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002EPointQueryWrapper_002E_007Bdtor_007D), &pointQueryWrapper);
			throw;
		}
		try
		{
			global::_003CModule_003E.gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AWidgetManager_0020_005E_003E_002E_007Bdtor_007D((gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AWidgetManager_0020_005E_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref pointQueryWrapper, 16)));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003AIList_003CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003APickResult_0020_005E_003E_0020_005E_003E*, void>)(&global::_003CModule_003E.gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003AIList_003CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003APickResult_0020_005E_003E_0020_005E_003E_002E_007Bdtor_007D), System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref pointQueryWrapper, 8)));
			throw;
		}
		global::_003CModule_003E.gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003AIList_003CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003APickResult_0020_005E_003E_0020_005E_003E_002E_007Bdtor_007D((gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003AIList_003CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003APickResult_0020_005E_003E_0020_005E_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref pointQueryWrapper, 8)));
		return result;
	}

	public virtual IWidget CreateWidget(string WidgetType, IValueSet arguments, object BoundObject)
	{
		WindowID windowID = m_windowID;
		if (windowID == (WindowID)0u)
		{
			throw new Exception("Cannot create widgets on closed windows");
		}
		return m_pmWidgetManager.CreateWidget(windowID, WidgetType, arguments, BoundObject);
	}

	public WindowID GetWindowID()
	{
		return m_windowID;
	}

	internal unsafe void UnbindAssetsFromWindow()
	{
		//IL_0054: Expected I, but got I8
		if (m_windowID == (WindowID)0u || m_previewer == null)
		{
			return;
		}
		List<AssetPreviewPack>.Enumerator enumerator = m_openedAssets.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				enumerator.Current.RemoveReferences();
			}
			while (enumerator.MoveNext());
		}
		IPreviewer* previewer = m_previewer;
		((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowID, void>)(*(ulong*)(*(long*)previewer + 64)))((nint)previewer, m_windowID);
		m_previewModule = string.Empty;
		m_openedAssets.Clear();
	}

	internal unsafe void RemoveReferences()
	{
		//IL_005a: Expected I, but got I8
		//IL_0062: Expected I, but got I8
		//IL_004b: Expected I, but got I8
		m_openedKnobSets.Clear();
		UnbindAssetsFromWindow();
		WindowID windowID = m_windowID;
		if (windowID != 0)
		{
			m_pmWidgetManager.DestroyWindowWidgets(windowID);
			IPreviewer* previewer = m_previewer;
			if (previewer != null)
			{
				((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowID, void>)(*(ulong*)(*(long*)previewer + 128)))((nint)previewer, m_windowID);
			}
			m_windowID = (WindowID)0u;
		}
		m_pPreviewModule = null;
		m_previewer = null;
		GC.SuppressFinalize(this);
	}

	public AssetPreviewPack GetPackInSlot(int slotID)
	{
		AssetPreviewPack result = null;
		List<AssetPreviewPack>.Enumerator enumerator = m_openedAssets.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				AssetPreviewPack current = enumerator.Current;
				if (current.SlotID == slotID)
				{
					result = current;
					break;
				}
			}
			while (enumerator.MoveNext());
		}
		return result;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EWindowWrapper();
			return;
		}
		try
		{
			_0021WindowWrapper();
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

	~WindowWrapper()
	{
		Dispose(A_0: false);
	}
}
