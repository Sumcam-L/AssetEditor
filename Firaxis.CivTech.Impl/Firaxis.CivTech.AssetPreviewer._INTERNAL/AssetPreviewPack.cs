using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using AssetPreviewer;
using fastdelegate;
using Firaxis.CivTech.AssetObjects;
using Platform;
using Serialization;
using std;
using Types;

namespace Firaxis.CivTech.AssetPreviewer._INTERNAL;

public class AssetPreviewPack : IDisposable
{
	private WindowID m_windowID;

	private int m_previewSlotID;

	private IInstanceEntity m_assetInstance;

	private readonly List<Tuple<string, IInstanceEntity>> m_manualAttachments;

	private readonly Dictionary<EntityID, Firaxis.CivTech.AssetObjects.AssetID> m_assets;

	private readonly Dictionary<string, EntityID> m_attaches;

	private readonly HashSet<EntityID> m_dependents;

	private readonly List<Tuple<string, EntityID>> m_manuallyAttachedAssets;

	private IXLPRegistry m_pmXLPRegistry;

	private ICivTechService m_pmCivTechSvc;

	private IInstanceSet m_pmInstanceSet;

	private global::AssetPreviewer.AssetID m_previewAssetID;

	private unsafe IPreviewer* m_previewer;

	private string m_previewModuleName;

	private IProjectConfig Config => m_pmCivTechSvc.PrimaryProject.Config;

	private IVirtualPantry Pantry => m_pmCivTechSvc.ProjectMapService.LayeredPantry;

	internal int SlotID => m_previewSlotID;

	internal IInstanceEntity Asset => m_assetInstance;

	internal unsafe AssetPreviewPack(string pmPreviewModule, IPreviewer* pPreviewer, ICivTechService pmCivTechSvc, IXLPRegistry pmXLPRegistry, IInstanceSet pmInstanceSet, WindowID windowID, int slotID, IInstanceEntity asset)
	{
		//IL_00ae: Expected I, but got I8
		m_windowID = windowID;
		m_previewSlotID = slotID;
		m_assetInstance = asset;
		m_manualAttachments = new List<Tuple<string, IInstanceEntity>>();
		m_assets = new Dictionary<EntityID, Firaxis.CivTech.AssetObjects.AssetID>();
		m_attaches = new Dictionary<string, EntityID>();
		m_dependents = new HashSet<EntityID>();
		m_manuallyAttachedAssets = new List<Tuple<string, EntityID>>();
		m_pmXLPRegistry = pmXLPRegistry;
		m_pmCivTechSvc = pmCivTechSvc;
		m_pmInstanceSet = pmInstanceSet;
		m_previewer = pPreviewer;
		m_previewModuleName = pmPreviewModule;
		base._002Ector();
		global::AssetPreviewer.AssetID assetID = (m_previewAssetID = LoadPreviewAsset());
		if (assetID != 0)
		{
			IPreviewer* previewer = m_previewer;
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowID, global::AssetPreviewer.AssetID, uint, void>)(*(ulong*)(*(long*)previewer + 16)))((nint)previewer, m_windowID, assetID, (uint)slotID);
		}
		RefreshContents();
	}

	private void _007EAssetPreviewPack()
	{
		RemoveReferences();
	}

	private void _0021AssetPreviewPack()
	{
		RemoveReferences();
	}

	[return: MarshalAs(UnmanagedType.U1)]
	internal bool AddKnobAttachment(IInstanceEntity pmAsset, IAttachmentPointData pmAttachmentData)
	{
		string name = pmAttachmentData.AttachmentPoint.Name;
		m_manualAttachments.Add(new Tuple<string, IInstanceEntity>(name, pmAsset));
		RefreshContents();
		return true;
	}

	internal IEnumerable<string> GetKnobAttachments(string attachmentPointName, Firaxis.CivTech.AssetObjects.InstanceType attachedAssetType)
	{
		IList<string> list = new List<string>();
		List<Tuple<string, IInstanceEntity>>.Enumerator enumerator = m_manualAttachments.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				Tuple<string, IInstanceEntity> current = enumerator.Current;
				if (current.Item1 == attachmentPointName && current.Item2.Type == attachedAssetType)
				{
					list.Add(current.Item2.Name);
				}
			}
			while (enumerator.MoveNext());
		}
		return list;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	internal bool RemoveKnobAttachment(string attachmentPointName, string attachedAssetName, Firaxis.CivTech.AssetObjects.InstanceType attachedAssetType)
	{
		List<Tuple<string, IInstanceEntity>>.Enumerator enumerator = m_manualAttachments.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				Tuple<string, IInstanceEntity> current = enumerator.Current;
				if (current.Item1 == attachmentPointName && current.Item2.Type == attachedAssetType && current.Item2.Name == attachedAssetName)
				{
					m_manualAttachments.Remove(current);
					RefreshContents();
					return true;
				}
			}
			while (enumerator.MoveNext());
		}
		return false;
	}

	internal unsafe void ReloadAssets()
	{
		//IL_0030: Expected I, but got I8
		//IL_006a: Expected I, but got I8
		global::AssetPreviewer.AssetID assetID = (m_previewAssetID = LoadPreviewAsset());
		if (assetID != 0)
		{
			IPreviewer* previewer = m_previewer;
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetPreviewer.AssetID, void>)(*(ulong*)(*(long*)previewer + 208)))((nint)previewer, assetID);
		}
		Dictionary<EntityID, Firaxis.CivTech.AssetObjects.AssetID>.ValueCollection.Enumerator enumerator = m_assets.Values.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				Firaxis.CivTech.AssetObjects.AssetID current = enumerator.Current;
				IPreviewer* previewer2 = m_previewer;
				((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetPreviewer.AssetID, void>)(*(ulong*)(*(long*)previewer2 + 208)))((nint)previewer2, (global::AssetPreviewer.AssetID)current);
			}
			while (enumerator.MoveNext());
		}
	}

	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe bool ApplyChanges(IEnumerable<IEntityChangedEvent> pmChangeList)
	{
		//IL_0112: Expected I, but got I8
		//IL_0208: Expected I, but got I8
		//IL_0225: Expected I, but got I8
		//IL_01b5: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		bool flag = false;
		foreach (IEntityChangedEvent pmChange in pmChangeList)
		{
			if (pmChange.Type != EntityChangeType.ECT_GENERIC && pmChange.Type != EntityChangeType.ECT_ATTACHMENT_REMOVED)
			{
				if (pmChange.Type != EntityChangeType.ECT_ATTACHMENT_COOK_PARAMETER_CHANGED && pmChange.Type != EntityChangeType.ECT_COOK_PARAMETER_CHANGED)
				{
					if (pmChange.Type == EntityChangeType.ECT_ATTACHMENT_CHANGED)
					{
						Firaxis.CivTech.AssetObjects.AttachmentChanged attachmentChanged = (Firaxis.CivTech.AssetObjects.AttachmentChanged)pmChange;
						if (attachmentChanged.OldAttachmentName != attachmentChanged.NewAttachmentName)
						{
							flag = true;
							break;
						}
					}
					continue;
				}
				flag = true;
				break;
			}
			flag = true;
			break;
		}
		bool result = false;
		if (flag)
		{
			result = RefreshContents();
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out global::AssetObjects.String obj);
		global::_003CModule_003E.AssetObjects_002EString_002E_007Bctor_007D(&obj);
		try
		{
			Firaxis.CivTech.AssetObjects.EntityChangeList.SerializeToXML(pmChangeList, &obj);
			foreach (IEntityChangedEvent pmChange2 in pmChangeList)
			{
				if (m_assetInstance.Type == pmChange2.InstanceType && m_assetInstance.Name == pmChange2.EntityName)
				{
					IPreviewer* previewer = m_previewer;
					((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetPreviewer.AssetID, sbyte*, void>)(*(ulong*)(*(long*)previewer + 216)))((nint)previewer, m_previewAssetID, global::_003CModule_003E.AssetObjects_002EString_002Ec_str(&obj));
					break;
				}
			}
			Dictionary<EntityID, Firaxis.CivTech.AssetObjects.AssetID>.KeyCollection.Enumerator enumerator3 = m_assets.Keys.GetEnumerator();
			while (enumerator3.MoveNext())
			{
				EntityID current3 = enumerator3.Current;
				foreach (IEntityChangedEvent pmChange3 in pmChangeList)
				{
					if (current3.Type == pmChange3.InstanceType && current3.Name == pmChange3.EntityName)
					{
						IPreviewer* previewer2 = m_previewer;
						((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetPreviewer.AssetID, sbyte*, void>)(*(ulong*)(*(long*)previewer2 + 216)))((nint)previewer2, (global::AssetPreviewer.AssetID)m_assets[current3], global::_003CModule_003E.AssetObjects_002EString_002Ec_str(&obj));
						break;
					}
				}
			}
			foreach (IEntityChangedEvent pmChange4 in pmChangeList)
			{
				if (pmChange4.Type == EntityChangeType.ECT_GENERIC)
				{
					StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(pmChange4.EntityName);
					try
					{
						standardStringWrapper = standardStringWrapper2;
						IPreviewer* ptr = (IPreviewer*)((ulong)(nint)m_previewer + 24uL);
						((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.InstanceType, sbyte*, void>)(*(ulong*)(*(long*)ptr + 24)))((nint)ptr, (global::AssetObjects.InstanceType)pmChange4.InstanceType, standardStringWrapper.Value);
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
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.String*, void>)(&global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D), &obj);
			throw;
		}
		global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D(&obj);
		return result;
	}

	internal unsafe void RemoveReferences()
	{
		//IL_00e0: Expected I, but got I8
		//IL_0079: Expected I, but got I8
		//IL_0126: Expected I, but got I8
		//IL_01c9: Expected I, but got I8
		//IL_0196: Expected I, but got I8
		//IL_01b3: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		if (m_previewer == null)
		{
			return;
		}
		List<Tuple<string, EntityID>>.Enumerator enumerator = m_manuallyAttachedAssets.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				Tuple<string, EntityID> current = enumerator.Current;
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(current.Item1);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					IPreviewer* previewer = m_previewer;
					((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowID, uint, AttachmentType, sbyte*, global::AssetPreviewer.AssetID, void>)(*(ulong*)(*(long*)previewer + 56)))((nint)previewer, m_windowID, (uint)m_previewSlotID, (AttachmentType)1u, standardStringWrapper.Value, (global::AssetPreviewer.AssetID)m_assets[current.Item2]);
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
		HashSet<EntityID>.Enumerator enumerator2 = m_dependents.GetEnumerator();
		if (enumerator2.MoveNext())
		{
			do
			{
				EntityID current2 = enumerator2.Current;
				IPreviewer* previewer2 = m_previewer;
				((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowID, global::AssetPreviewer.AssetID, uint, void>)(*(ulong*)(*(long*)previewer2 + 40)))((nint)previewer2, m_windowID, (global::AssetPreviewer.AssetID)m_assets[current2], (uint)m_previewSlotID);
			}
			while (enumerator2.MoveNext());
		}
		Dictionary<EntityID, Firaxis.CivTech.AssetObjects.AssetID>.ValueCollection.Enumerator enumerator3 = m_assets.Values.GetEnumerator();
		if (enumerator3.MoveNext())
		{
			do
			{
				Firaxis.CivTech.AssetObjects.AssetID current3 = enumerator3.Current;
				IPreviewer* previewer3 = m_previewer;
				((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetPreviewer.AssetID, void>)(*(ulong*)(*(long*)previewer3 + 192)))((nint)previewer3, (global::AssetPreviewer.AssetID)current3);
			}
			while (enumerator3.MoveNext());
		}
		m_manuallyAttachedAssets.Clear();
		m_manualAttachments.Clear();
		m_attaches.Clear();
		m_dependents.Clear();
		m_assets.Clear();
		global::AssetPreviewer.AssetID previewAssetID = m_previewAssetID;
		if (previewAssetID != 0)
		{
			IPreviewer* previewer4 = m_previewer;
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowID, global::AssetPreviewer.AssetID, uint, void>)(*(ulong*)(*(long*)previewer4 + 24)))((nint)previewer4, m_windowID, previewAssetID, (uint)m_previewSlotID);
			IPreviewer* previewer5 = m_previewer;
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetPreviewer.AssetID, void>)(*(ulong*)(*(long*)previewer5 + 192)))((nint)previewer5, m_previewAssetID);
			m_previewAssetID = (global::AssetPreviewer.AssetID)0u;
		}
		m_assetInstance = null;
		m_previewer = null;
		m_previewModuleName = null;
		GC.SuppressFinalize(this);
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe bool RefreshContents()
	{
		//IL_003a: Expected I8, but got I
		//IL_017c: Expected I, but got I8
		//IL_01e8: Expected I8, but got I
		//IL_020b: Expected I, but got I8
		//IL_02ad: Expected I, but got I8
		//IL_04ab: Expected I, but got I8
		//IL_05cf: Expected I, but got I8
		//IL_04e1: Expected I, but got I8
		//IL_04e9: Expected I, but got I8
		//IL_04f1: Expected I, but got I8
		//IL_0625: Expected I, but got I8
		//IL_0558: Expected I, but got I8
		//IL_043c: Expected I, but got I8
		//IL_0381: Expected I8, but got I
		//IL_038e: Expected I8, but got I
		//IL_039a: Expected I8, but got I
		//IL_03c4: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		StandardStringWrapper standardStringWrapper3 = null;
		StandardStringWrapper standardStringWrapper4 = null;
		StandardStringWrapper standardStringWrapper5 = null;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out SimpleTimer simpleTimer);
		global::_003CModule_003E.Platform_002ESimpleTimer_002E_007Bctor_007D(&simpleTimer);
		bool flag = false;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out gcroot_003CFiraxis_003A_003ACivTech_003A_003AIXLPRegistry_0020_005E_003E obj);
		gcroot_003CFiraxis_003A_003ACivTech_003A_003AIXLPRegistry_0020_005E_003E* ptr = &obj;
		*(long*)(&obj) = (nint)((IntPtr)GCHandle.Alloc(m_pmXLPRegistry)).ToPointer();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out RuntimeRefCrawler runtimeRefCrawler);
		global::_003CModule_003E.AssetObjects_002ERuntimeRefCrawler_002E_007Bctor_007D(&runtimeRefCrawler, &obj);
		byte result;
		try
		{
			HashSet<IInstanceEntity> hashSet = new HashSet<IInstanceEntity>();
			hashSet.Add(m_assetInstance);
			List<Tuple<string, IInstanceEntity>>.Enumerator enumerator = m_manualAttachments.GetEnumerator();
			if (enumerator.MoveNext())
			{
				do
				{
					Tuple<string, IInstanceEntity> current = enumerator.Current;
					hashSet.Add(current.Item2);
				}
				while (enumerator.MoveNext());
			}
			global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002E_003FA0xc4e72474_002ECrawlAllRuntimeDependencies(m_pmCivTechSvc.PrimaryProject.Config, Pantry, m_previewer, &runtimeRefCrawler, hashSet);
			global::_003CModule_003E.Platform_002ESimpleTimer_002EMark(&simpleTimer);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out vector_003Cenum_0020AssetPreviewer_003A_003AAssetID_002Cstd_003A_003Aallocator_003Cenum_0020AssetPreviewer_003A_003AAssetID_003E_0020_003E obj2);
			global::_003CModule_003E.std_002Evector_003Cenum_0020AssetPreviewer_003A_003AAssetID_002Cstd_003A_003Aallocator_003Cenum_0020AssetPreviewer_003A_003AAssetID_003E_0020_003E_002E_007Bctor_007D(&obj2);
			try
			{
				HashSet<EntityID> hashSet2 = new HashSet<EntityID>();
				HashSet<EntityID> hashSet3 = new HashSet<EntityID>();
				HashSet<string> hashSet4 = new HashSet<string>();
				hashSet2.UnionWith(m_assets.Keys);
				hashSet4.UnionWith(m_attaches.Keys);
				HashSet<EntityID>.Enumerator enumerator2 = m_dependents.GetEnumerator();
				if (enumerator2.MoveNext())
				{
					do
					{
						EntityID current2 = enumerator2.Current;
						hashSet3.Add(current2);
					}
					while (enumerator2.MoveNext());
				}
				StandardStringWrapper standardStringWrapper6 = new StandardStringWrapper(m_previewModuleName);
				try
				{
					standardStringWrapper = standardStringWrapper6;
					System.Runtime.CompilerServices.Unsafe.SkipInit(out _Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003ADependency_003E_0020_003E_0020_003E obj3);
					global::_003CModule_003E.std_002Evector_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003ADependency_002Cstd_003A_003Aallocator_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003ADependency_003E_0020_003E_002Ebegin((vector_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003ADependency_002Cstd_003A_003Aallocator_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003ADependency_003E_0020_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref runtimeRefCrawler, 32)), &obj3);
					System.Runtime.CompilerServices.Unsafe.SkipInit(out _Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003ADependency_003E_0020_003E_0020_003E obj4);
					global::_003CModule_003E.std_002Evector_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003ADependency_002Cstd_003A_003Aallocator_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003ADependency_003E_0020_003E_002Eend((vector_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003ADependency_002Cstd_003A_003Aallocator_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003ADependency_003E_0020_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref runtimeRefCrawler, 32)), &obj4);
					if (global::_003CModule_003E.std_002E_Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003ADependency_003E_0020_003E_0020_003E_002E_0021_003D(&obj3, &obj4))
					{
						System.Runtime.CompilerServices.Unsafe.SkipInit(out AssetParameters assetParameters);
						do
						{
							RuntimeRefCrawler.Dependency* ptr2 = global::_003CModule_003E.std_002E_Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003ADependency_003E_0020_003E_0020_003E_002E_002A(&obj3);
							global::AssetPreviewer.AssetID value = (global::AssetPreviewer.AssetID)0u;
							RuntimeRefCrawler.Dependency* ptr3 = (RuntimeRefCrawler.Dependency*)((ulong)(nint)ptr2 + 32uL);
							if (m_assets.ContainsKey(global::_003CModule_003E.gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003AEntityID_0020_005E_003E_002E_002EPE_0024AAVEntityID_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040((gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003AEntityID_0020_005E_003E*)ptr3)))
							{
								value = (global::AssetPreviewer.AssetID)m_assets[global::_003CModule_003E.gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003AEntityID_0020_005E_003E_002E_002EPE_0024AAVEntityID_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040((gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003AEntityID_0020_005E_003E*)ptr3)];
								hashSet2.Remove(global::_003CModule_003E.gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003AEntityID_0020_005E_003E_002E_002EPE_0024AAVEntityID_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040((gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003AEntityID_0020_005E_003E*)ptr3));
							}
							else
							{
								System.Runtime.CompilerServices.Unsafe.As<AssetParameters, Firaxis.CivTech.AssetObjects.InstanceType>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref assetParameters, 4)) = global::_003CModule_003E.gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003AEntityID_0020_005E_003E_002E_002D_003E((gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003AEntityID_0020_005E_003E*)ptr3).Type;
								System.Runtime.CompilerServices.Unsafe.As<AssetParameters, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref assetParameters, 16)) = *(long*)((ulong)(nint)ptr2 + 16uL);
								System.Runtime.CompilerServices.Unsafe.As<AssetParameters, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref assetParameters, 24)) = *(long*)((ulong)(nint)ptr2 + 24uL);
								System.Runtime.CompilerServices.Unsafe.As<AssetParameters, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref assetParameters, 8)) = (nint)standardStringWrapper.Value;
								*(int*)(&assetParameters) = 1;
								IPreviewer* previewer = m_previewer;
								value = ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, AssetParameters*, int, global::AssetPreviewer.AssetID>)(*(ulong*)(*(long*)previewer + 184)))((nint)previewer, &assetParameters, m_previewSlotID);
								m_assets.Add(global::_003CModule_003E.gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003AEntityID_0020_005E_003E_002E_002EPE_0024AAVEntityID_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040((gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003AEntityID_0020_005E_003E*)ptr3), (Firaxis.CivTech.AssetObjects.AssetID)value);
								flag = true;
							}
							global::_003CModule_003E.std_002Evector_003Cenum_0020AssetPreviewer_003A_003AAssetID_002Cstd_003A_003Aallocator_003Cenum_0020AssetPreviewer_003A_003AAssetID_003E_0020_003E_002Epush_back(&obj2, &value);
							global::_003CModule_003E.std_002E_Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003ADependency_003E_0020_003E_0020_003E_002E_002B_002B(&obj3);
						}
						while (global::_003CModule_003E.std_002E_Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003ADependency_003E_0020_003E_0020_003E_002E_0021_003D(&obj3, &obj4));
					}
					List<Tuple<string, EntityID>>.Enumerator enumerator3 = m_manuallyAttachedAssets.GetEnumerator();
					if (enumerator3.MoveNext())
					{
						do
						{
							Tuple<string, EntityID> current3 = enumerator3.Current;
							StandardStringWrapper standardStringWrapper7 = new StandardStringWrapper(current3.Item1);
							try
							{
								standardStringWrapper2 = standardStringWrapper7;
								IPreviewer* previewer = m_previewer;
								((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowID, uint, AttachmentType, sbyte*, global::AssetPreviewer.AssetID, void>)(*(ulong*)(*(long*)previewer + 56)))((nint)previewer, m_windowID, (uint)m_previewSlotID, (AttachmentType)1u, standardStringWrapper2.Value, (global::AssetPreviewer.AssetID)m_assets[current3.Item2]);
							}
							catch
							{
								//try-fault
								((IDisposable)standardStringWrapper2).Dispose();
								throw;
							}
							((IDisposable)standardStringWrapper2).Dispose();
						}
						while (enumerator3.MoveNext());
					}
					m_manuallyAttachedAssets.Clear();
					ValidateManualAttachments();
					List<Tuple<string, IInstanceEntity>>.Enumerator enumerator4 = m_manualAttachments.GetEnumerator();
					if (enumerator4.MoveNext())
					{
						System.Runtime.CompilerServices.Unsafe.SkipInit(out AssetParameters assetParameters2);
						do
						{
							Tuple<string, IInstanceEntity> current4 = enumerator4.Current;
							StandardStringWrapper standardStringWrapper8 = new StandardStringWrapper(current4.Item1);
							try
							{
								standardStringWrapper3 = standardStringWrapper8;
								EntityID entityID = new EntityID(current4.Item2.Name, current4.Item2.Type);
								IPreviewer* previewer;
								if (m_assets.ContainsKey(entityID))
								{
									hashSet2.Remove(entityID);
								}
								else
								{
									StandardStringWrapper standardStringWrapper9 = new StandardStringWrapper(entityID.Name);
									try
									{
										standardStringWrapper4 = standardStringWrapper9;
										StandardStringWrapper standardStringWrapper10 = new StandardStringWrapper(current4.Item2.ClassName);
										try
										{
											standardStringWrapper5 = standardStringWrapper10;
											System.Runtime.CompilerServices.Unsafe.As<AssetParameters, Firaxis.CivTech.AssetObjects.InstanceType>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref assetParameters2, 4)) = entityID.Type;
											System.Runtime.CompilerServices.Unsafe.As<AssetParameters, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref assetParameters2, 16)) = (nint)standardStringWrapper4.Value;
											System.Runtime.CompilerServices.Unsafe.As<AssetParameters, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref assetParameters2, 24)) = (nint)standardStringWrapper5.Value;
											System.Runtime.CompilerServices.Unsafe.As<AssetParameters, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref assetParameters2, 8)) = (nint)standardStringWrapper.Value;
											*(int*)(&assetParameters2) = 1;
											previewer = m_previewer;
											m_assets[entityID] = (Firaxis.CivTech.AssetObjects.AssetID)((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, AssetParameters*, int, global::AssetPreviewer.AssetID>)(*(ulong*)(*(long*)previewer + 184)))((nint)previewer, &assetParameters2, m_previewSlotID);
										}
										catch
										{
											//try-fault
											((IDisposable)standardStringWrapper5).Dispose();
											throw;
										}
										((IDisposable)standardStringWrapper5).Dispose();
									}
									catch
									{
										//try-fault
										((IDisposable)standardStringWrapper4).Dispose();
										throw;
									}
									((IDisposable)standardStringWrapper4).Dispose();
								}
								m_manuallyAttachedAssets.Add(new Tuple<string, EntityID>(current4.Item1, entityID));
								previewer = m_previewer;
								((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowID, uint, global::AssetPreviewer.AssetID, AttachmentType, sbyte*, global::AssetPreviewer.AssetID, void>)(*(ulong*)(*(long*)previewer + 48)))((nint)previewer, m_windowID, (uint)m_previewSlotID, m_previewAssetID, (AttachmentType)1u, standardStringWrapper3.Value, (global::AssetPreviewer.AssetID)m_assets[entityID]);
							}
							catch
							{
								//try-fault
								((IDisposable)standardStringWrapper3).Dispose();
								throw;
							}
							((IDisposable)standardStringWrapper3).Dispose();
						}
						while (enumerator4.MoveNext());
					}
					global::_003CModule_003E.Platform_002ESimpleTimer_002EMark(&simpleTimer);
					System.Runtime.CompilerServices.Unsafe.SkipInit(out _Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003AAssignment_003E_0020_003E_0020_003E obj5);
					global::_003CModule_003E.std_002Evector_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003AAssignment_002Cstd_003A_003Aallocator_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003AAssignment_003E_0020_003E_002Ebegin((vector_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003AAssignment_002Cstd_003A_003Aallocator_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003AAssignment_003E_0020_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref runtimeRefCrawler, 8)), &obj5);
					System.Runtime.CompilerServices.Unsafe.SkipInit(out _Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003AAssignment_003E_0020_003E_0020_003E obj6);
					global::_003CModule_003E.std_002Evector_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003AAssignment_002Cstd_003A_003Aallocator_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003AAssignment_003E_0020_003E_002Eend((vector_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003AAssignment_002Cstd_003A_003Aallocator_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003AAssignment_003E_0020_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref runtimeRefCrawler, 8)), &obj6);
					if (global::_003CModule_003E.std_002E_Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003AAssignment_003E_0020_003E_0020_003E_002E_0021_003D(&obj5, &obj6))
					{
						do
						{
							RuntimeRefCrawler.Assignment* ptr4 = global::_003CModule_003E.std_002E_Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003AAssignment_003E_0020_003E_0020_003E_002E_002A(&obj5);
							RuntimeRefCrawler.Dependency* ptr5 = global::_003CModule_003E.std_002Evector_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003ADependency_002Cstd_003A_003Aallocator_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003ADependency_003E_0020_003E_002E_005B_005D((vector_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003ADependency_002Cstd_003A_003Aallocator_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003ADependency_003E_0020_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref runtimeRefCrawler, 32)), (ulong)(*(uint*)((ulong)(nint)ptr4 + 8uL)));
							RuntimeRefCrawler.Dependency* ptr6 = (RuntimeRefCrawler.Dependency*)((ulong)(nint)ptr5 + 32uL);
							if (global::_003CModule_003E.gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003AEntityID_0020_005E_003E_002E_002D_003E((gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003AEntityID_0020_005E_003E*)ptr6).Type == Firaxis.CivTech.AssetObjects.InstanceType.IT_INVALID)
							{
								m_pmCivTechSvc.CivTechContext.CivTechLogger.AddLogItem(LogEventType.Error, "Tool", $"Dependent asset \"{(global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString((sbyte*)(*(ulong*)((ulong)(nint)ptr5 + 8uL))))}\" in xlp \"{(global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString((sbyte*)(*(ulong*)ptr5)))}\" could not be resolved for parent \"{(global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString((sbyte*)(*(ulong*)ptr4)))}\"");
							}
							EntityID item = global::_003CModule_003E.gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003AEntityID_0020_005E_003E_002E_002EPE_0024AAVEntityID_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040((gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003AEntityID_0020_005E_003E*)ptr6);
							global::AssetPreviewer.AssetID assetID = *global::_003CModule_003E.std_002Evector_003Cenum_0020AssetPreviewer_003A_003AAssetID_002Cstd_003A_003Aallocator_003Cenum_0020AssetPreviewer_003A_003AAssetID_003E_0020_003E_002E_005B_005D(&obj2, *(uint*)((ulong)(nint)ptr4 + 8uL));
							if (m_dependents.Contains(item))
							{
								hashSet3.Remove(item);
							}
							else
							{
								IPreviewer* previewer = m_previewer;
								((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowID, global::AssetPreviewer.AssetID, global::AssetPreviewer.AssetID, uint, void>)(*(ulong*)(*(long*)previewer + 32)))((nint)previewer, m_windowID, m_previewAssetID, assetID, (uint)m_previewSlotID);
								m_dependents.Add(item);
								flag = true;
							}
							global::_003CModule_003E.std_002E_Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003AAssignment_003E_0020_003E_0020_003E_002E_002B_002B(&obj5);
						}
						while (global::_003CModule_003E.std_002E_Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003CAssetObjects_003A_003ARuntimeRefCrawler_003A_003AAssignment_003E_0020_003E_0020_003E_002E_0021_003D(&obj5, &obj6));
					}
					global::_003CModule_003E.Platform_002ESimpleTimer_002EMark(&simpleTimer);
					HashSet<EntityID>.Enumerator enumerator5 = hashSet3.GetEnumerator();
					if (enumerator5.MoveNext())
					{
						do
						{
							EntityID current5 = enumerator5.Current;
							IPreviewer* previewer = m_previewer;
							((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowID, global::AssetPreviewer.AssetID, uint, void>)(*(ulong*)(*(long*)previewer + 40)))((nint)previewer, m_windowID, (global::AssetPreviewer.AssetID)m_assets[current5], (uint)m_previewSlotID);
							m_dependents.Remove(current5);
						}
						while (enumerator5.MoveNext());
					}
					HashSet<EntityID>.Enumerator enumerator6 = hashSet2.GetEnumerator();
					if (enumerator6.MoveNext())
					{
						do
						{
							EntityID current6 = enumerator6.Current;
							IPreviewer* previewer = m_previewer;
							((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetPreviewer.AssetID, void>)(*(ulong*)(*(long*)previewer + 192)))((nint)previewer, (global::AssetPreviewer.AssetID)m_assets[current6]);
							m_assets.Remove(current6);
						}
						while (enumerator6.MoveNext());
					}
					global::_003CModule_003E.Platform_002ESimpleTimer_002EMark(&simpleTimer);
					int num = ((hashSet2.Count != 0 || hashSet4.Count != 0 || hashSet3.Count != 0 || flag) ? 1 : 0);
					result = (byte)num;
				}
				catch
				{
					//try-fault
					((IDisposable)standardStringWrapper).Dispose();
					throw;
				}
				((IDisposable)standardStringWrapper).Dispose();
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<vector_003Cenum_0020AssetPreviewer_003A_003AAssetID_002Cstd_003A_003Aallocator_003Cenum_0020AssetPreviewer_003A_003AAssetID_003E_0020_003E*, void>)(&global::_003CModule_003E.std_002Evector_003Cenum_0020AssetPreviewer_003A_003AAssetID_002Cstd_003A_003Aallocator_003Cenum_0020AssetPreviewer_003A_003AAssetID_003E_0020_003E_002E_007Bdtor_007D), &obj2);
				throw;
			}
			global::_003CModule_003E.std_002Evector_003Cenum_0020AssetPreviewer_003A_003AAssetID_002Cstd_003A_003Aallocator_003Cenum_0020AssetPreviewer_003A_003AAssetID_003E_0020_003E_002E_007Bdtor_007D(&obj2);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<RuntimeRefCrawler*, void>)(&global::_003CModule_003E.AssetObjects_002ERuntimeRefCrawler_002E_007Bdtor_007D), &runtimeRefCrawler);
			throw;
		}
		global::_003CModule_003E.AssetObjects_002ERuntimeRefCrawler_002E_007Bdtor_007D(&runtimeRefCrawler);
		return result != 0;
	}

	private unsafe global::AssetPreviewer.AssetID LoadPreviewAsset()
	{
		//IL_003d: Expected I8, but got I
		string assetName = m_assetInstance.Name;
		if (string.Equals(m_assetInstance.Name, "Untitled", StringComparison.CurrentCultureIgnoreCase))
		{
			ulong num = 0uL;
			Firaxis.CivTech.AssetObjects.InstanceEntity instanceEntity = (Firaxis.CivTech.AssetObjects.InstanceEntity)m_assetInstance;
			if (instanceEntity != null)
			{
				num = (ulong)(nint)instanceEntity.GetAssetObject();
			}
			assetName = $"{num:X16}";
		}
		return OpenAsset(assetName, m_assetInstance.ClassName, m_assetInstance.Type);
	}

	private unsafe global::AssetPreviewer.AssetID OpenAsset(string assetName, string className, Firaxis.CivTech.AssetObjects.InstanceType instType)
	{
		//IL_003e: Expected I8, but got I
		//IL_004a: Expected I8, but got I
		//IL_0055: Expected I8, but got I
		//IL_0078: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		StandardStringWrapper standardStringWrapper3 = null;
		StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(assetName);
		global::AssetPreviewer.AssetID result;
		try
		{
			standardStringWrapper = standardStringWrapper4;
			StandardStringWrapper standardStringWrapper5 = new StandardStringWrapper(className);
			try
			{
				standardStringWrapper2 = standardStringWrapper5;
				StandardStringWrapper standardStringWrapper6 = new StandardStringWrapper(m_previewModuleName);
				try
				{
					standardStringWrapper3 = standardStringWrapper6;
					System.Runtime.CompilerServices.Unsafe.SkipInit(out AssetParameters assetParameters);
					System.Runtime.CompilerServices.Unsafe.As<AssetParameters, Firaxis.CivTech.AssetObjects.InstanceType>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref assetParameters, 4)) = instType;
					System.Runtime.CompilerServices.Unsafe.As<AssetParameters, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref assetParameters, 16)) = (nint)standardStringWrapper.Value;
					System.Runtime.CompilerServices.Unsafe.As<AssetParameters, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref assetParameters, 24)) = (nint)standardStringWrapper2.Value;
					System.Runtime.CompilerServices.Unsafe.As<AssetParameters, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref assetParameters, 8)) = (nint)standardStringWrapper3.Value;
					*(int*)(&assetParameters) = 1;
					IPreviewer* previewer = m_previewer;
					result = ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, AssetParameters*, int, global::AssetPreviewer.AssetID>)(*(ulong*)(*(long*)previewer + 184)))((nint)previewer, &assetParameters, m_previewSlotID);
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

	private unsafe void ValidateManualAttachments()
	{
		//IL_0071: Expected I, but got I8
		//IL_00d3: Expected I8, but got I
		//IL_00ea: Expected I8, but got I
		//IL_010c: Expected I, but got I8
		//IL_009a: Expected I, but got I8
		HashSet<string> hashSet = new HashSet<string>();
		if (m_assetInstance.Type == Firaxis.CivTech.AssetObjects.InstanceType.IT_ASSET)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out global::AssetObjects.Deserializer deserializer);
			global::_003CModule_003E.AssetObjects_002EDeserializer_002E_007Bctor_007D(&deserializer);
			try
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out global::AssetObjects.InstanceSet instanceSet);
				global::_003CModule_003E.AssetObjects_002EPolymorphicContainer_003CAssetObjects_003A_003AInstanceEntity_003E_002E_007Bctor_007D((PolymorphicContainer_003CAssetObjects_003A_003AInstanceEntity_003E*)(&instanceSet));
				try
				{
					VirtualPantry virtualPantry = (VirtualPantry)Pantry;
					Firaxis.CivTech.AssetObjects.ProjectConfig projectConfig = (Firaxis.CivTech.AssetObjects.ProjectConfig)m_pmCivTechSvc.PrimaryProject.Config;
					if (!global::_003CModule_003E._003FA0xc4e72474_002E_003FbIgnoreAlways_0040_003F5_003F_003FValidateManualAttachments_0040AssetPreviewPack_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA && virtualPantry == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BD_0040IBEDHMBB_0040typedVP_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GN_0040POPEDCMM_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 602u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xc4e72474_002E_003FbIgnoreAlways_0040_003F5_003F_003FValidateManualAttachments_0040AssetPreviewPack_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
					{
						/*OpCode not supported: DebugBreak*/;
					}
					if (!global::_003CModule_003E._003FA0xc4e72474_002E_003FbIgnoreAlways_0040_003FN_0040_003F_003FValidateManualAttachments_0040AssetPreviewPack_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA && projectConfig == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BD_0040HAEMPNLD_0040typedPC_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GN_0040POPEDCMM_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 603u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xc4e72474_002E_003FbIgnoreAlways_0040_003FN_0040_003F_003FValidateManualAttachments_0040AssetPreviewPack_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
					{
						/*OpCode not supported: DebugBreak*/;
					}
					global::AssetObjects.ProjectConfig* nativeConfig = projectConfig.GetNativeConfig();
					global::AssetObjects.VirtualPantry* nativePantry = virtualPantry.GetNativePantry();
					System.Runtime.CompilerServices.Unsafe.SkipInit(out global::AssetObjects.BehaviorInstance behaviorInstance);
					global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002E_007Bctor_007D(&behaviorInstance);
					try
					{
						global::_003CModule_003E.AssetObjects_002EEntity_002ESetName((Entity*)(&behaviorInstance), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BP_0040GNCDIAOK_0040_003F_0024CK_003F_0024CK_003F5BEHAVIOR_003F5HOLDER_003F5BEHAVIOR_003F5_003F_0024CK_003F_0024CK_003F_0024AA_0040));
						Firaxis.CivTech.AssetObjects.AssetInstance assetInstance = (Firaxis.CivTech.AssetObjects.AssetInstance)m_assetInstance;
						System.Runtime.CompilerServices.Unsafe.SkipInit(out BehaviorDependencyCrawler behaviorDependencyCrawler);
						*(long*)(&behaviorDependencyCrawler) = (nint)nativePantry;
						System.Runtime.CompilerServices.Unsafe.As<BehaviorDependencyCrawler, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref behaviorDependencyCrawler, 8)) = (nint)(&instanceSet);
						System.Runtime.CompilerServices.Unsafe.As<BehaviorDependencyCrawler, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref behaviorDependencyCrawler, 16)) = (nint)(&deserializer);
						System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024PTMType_0024P8BehaviorDependencyCrawler_0040AssetObjects_0040_0040EAAXW4InstanceType_00401_0040PEBD_0040Z _0024PTMType_0024P8BehaviorDependencyCrawler_0040AssetObjects_0040_0040EAAXW4InstanceType_00401_0040PEBD_0040Z2);
						*(long*)(&_0024PTMType_0024P8BehaviorDependencyCrawler_0040AssetObjects_0040_0040EAAXW4InstanceType_00401_0040PEBD_0040Z2) = (nint)global::_003CModule_003E.__unep_0040_003FOnDependency_0040BehaviorDependencyCrawler_0040AssetObjects_0040_0040_0024_0024FQEAAXW4InstanceType_00402_0040PEBD_0040Z;
						System.Runtime.CompilerServices.Unsafe.SkipInit(out FastDelegate2_003Cenum_0020AssetObjects_003A_003AInstanceType_002Cchar_0020const_0020_002A_002Cvoid_003E obj);
						global::_003CModule_003E.fastdelegate_002EFastDelegate2_003Cenum_0020AssetObjects_003A_003AInstanceType_002Cchar_0020const_0020_002A_002Cvoid_003E_002E_007Bctor_007D_003Cclass_0020AssetObjects_003A_003ABehaviorDependencyCrawler_002Cclass_0020AssetObjects_003A_003ABehaviorDependencyCrawler_003E(&obj, &behaviorDependencyCrawler, _0024PTMType_0024P8BehaviorDependencyCrawler_0040AssetObjects_0040_0040EAAXW4InstanceType_00401_0040PEBD_0040Z2);
						global::AssetObjects.InstanceEntity* assetObject = assetInstance.GetAssetObject();
						((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, FastDelegate2_003Cenum_0020AssetObjects_003A_003AInstanceType_002Cchar_0020const_0020_002A_002Cvoid_003E*, void>)(*(ulong*)(*(long*)assetObject + 64)))((nint)assetObject, &obj);
						global::_003CModule_003E.AssetObjects_002EAssetInstance_002EFlatten((global::AssetObjects.AssetInstance*)assetInstance.GetAssetObject(), &instanceSet, global::_003CModule_003E.AssetObjects_002EProjectConfig_002EGetClasses(nativeConfig), &behaviorInstance);
						global::AssetObjects.AttachmentPointSet* ptr = global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002EGetAttachmentPointSet(&behaviorInstance);
						System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAttachmentPoint_002C4096_003E.iterator iterator);
						global::_003CModule_003E.AssetObjects_002EAttachmentPointSet_002Ebegin(ptr, &iterator);
						System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAttachmentPoint_002C4096_003E.iterator iterator2);
						global::_003CModule_003E.AssetObjects_002EAttachmentPointSet_002Eend(ptr, &iterator2);
						if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAttachmentPoint_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, &iterator2))
						{
							do
							{
								global::AssetObjects.AttachmentPoint* ptr2 = global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAttachmentPoint_002C4096_003E_002Eiterator_002E_002A(&iterator);
								hashSet.Add(global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002EGetName(ptr2)));
								global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAttachmentPoint_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
							}
							while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAttachmentPoint_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, &iterator2));
						}
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.BehaviorInstance*, void>)(&global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002E_007Bdtor_007D), &behaviorInstance);
						throw;
					}
					global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002E_007Bdtor_007D(&behaviorInstance);
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.InstanceSet*, void>)(&global::_003CModule_003E.AssetObjects_002EInstanceSet_002E_007Bdtor_007D), &instanceSet);
					throw;
				}
				global::_003CModule_003E.AssetObjects_002EPolymorphicContainer_003CAssetObjects_003A_003AInstanceEntity_003E_002E_007Bdtor_007D((PolymorphicContainer_003CAssetObjects_003A_003AInstanceEntity_003E*)(&instanceSet));
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.Deserializer*, void>)(&global::_003CModule_003E.AssetObjects_002EDeserializer_002E_007Bdtor_007D), &deserializer);
				throw;
			}
			try
			{
				try
				{
					global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C1_003E_002C65535_002C16_003E_002E_007Bdtor_007D((ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C1_003E_002C65535_002C16_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref deserializer, 352)));
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C2_003E_002C8192_002C16_003E*, void>)(&global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C2_003E_002C8192_002C16_003E_002E_007Bdtor_007D), System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref deserializer, 312)));
					throw;
				}
				global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C2_003E_002C8192_002C16_003E_002E_007Bdtor_007D((ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C2_003E_002C8192_002C16_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref deserializer, 312)));
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Context*, void>)(&global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D), &deserializer);
				throw;
			}
			global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D((Context*)(&deserializer));
		}
		MissingPred missingPred = new MissingPred(hashSet);
		m_manualAttachments.RemoveAll(missingPred.ShouldRemove);
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			RemoveReferences();
			return;
		}
		try
		{
			RemoveReferences();
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

	~AssetPreviewPack()
	{
		Dispose(A_0: false);
	}
}
