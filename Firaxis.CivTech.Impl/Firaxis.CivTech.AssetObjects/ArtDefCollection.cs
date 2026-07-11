using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Platform;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class ArtDefCollection : IArtDefCollection, IDisposable
{
	private IList<IArtDefElement> m_artDefElements;

	private string m_collectionName;

	private ArtDef m_root;

	private ArtDefElement m_parent;

	private unsafe global::AssetObjects.ArtDefCollection* m_lastValidCollection;

	public unsafe virtual bool ReplaceMergedCollectionElements
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			global::AssetObjects.ArtDefCollection* collectionPointer = GetCollectionPointer();
			if (collectionPointer == null)
			{
				return false;
			}
			return global::_003CModule_003E.AssetObjects_002EArtDefCollection_002EGetReplaceMergedCollectionElements(collectionPointer);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			global::AssetObjects.ArtDefCollection* collectionPointer = GetCollectionPointer();
			if (collectionPointer != null)
			{
				global::_003CModule_003E.AssetObjects_002EArtDefCollection_002ESetReplaceMergedCollectionElements(collectionPointer, value);
			}
		}
	}

	public unsafe virtual IEnumerable<IArtDefElement> Elements
	{
		get
		{
			global::AssetObjects.ArtDefCollection* collectionPointer = GetCollectionPointer();
			if (m_lastValidCollection != collectionPointer)
			{
				m_lastValidCollection = collectionPointer;
				CacheUnmanagedValues(collectionPointer);
			}
			return m_artDefElements;
		}
	}

	public unsafe virtual string CollectionName
	{
		get
		{
			global::AssetObjects.ArtDefCollection* collectionPointer = GetCollectionPointer();
			if (m_lastValidCollection != collectionPointer)
			{
				m_lastValidCollection = collectionPointer;
				CacheUnmanagedValues(collectionPointer);
			}
			return m_collectionName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			if (string.IsNullOrWhiteSpace(value))
			{
				if (!global::_003CModule_003E._003FA0xd4acbafe_002E_003FbIgnoreAlways_0040_003F6_003F_003Fset_0040CollectionName_0040ArtDefCollection_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA)
				{
					System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
					global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EC_0040IJHJGKNB_0040Unable_003F5to_003F5assign_003F5empty_003F5or_003F5whites_0040), __arglist());
					if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DH_0040LKEFFIDK_0040_003F_0024CB_003F_0024CC_003F_0024CBSystem_003F3_003F3String_003F3_003F3IsNullOrWhite_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HD_0040NAHIBPKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 119u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xd4acbafe_002E_003FbIgnoreAlways_0040_003F6_003F_003Fset_0040CollectionName_0040ArtDefCollection_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
					{
						/*OpCode not supported: DebugBreak*/;
					}
				}
				return;
			}
			global::AssetObjects.ArtDefCollection* collectionPointer = GetCollectionPointer();
			if (collectionPointer != null)
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002EArtDefCollection_002ESetName(collectionPointer, standardStringWrapper.Value);
				}
				catch
				{
					//try-fault
					((IDisposable)standardStringWrapper).Dispose();
					throw;
				}
				((IDisposable)standardStringWrapper).Dispose();
			}
			m_collectionName = value;
		}
	}

	public unsafe ArtDefCollection(ArtDef root, ArtDefElement parent, global::AssetObjects.ArtDefCollection* artDefCollection)
	{
		//IL_0054: Expected I, but got I8
		//IL_007a: Expected I, but got I8
		m_artDefElements = new List<IArtDefElement>();
		m_collectionName = string.Empty;
		m_root = root;
		m_parent = parent;
		m_lastValidCollection = artDefCollection;
		base._002Ector();
		if (!global::_003CModule_003E._003FA0xd4acbafe_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ArtDefCollection_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVArtDef_0040234_0040PE_0024AAVArtDefElement_0040234_0040PEAV12_0040_0040Z_00404_NA && artDefCollection == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BB_0040GNCBKOKG_0040artDefCollection_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HD_0040NAHIBPKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 20u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xd4acbafe_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ArtDefCollection_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVArtDef_0040234_0040PE_0024AAVArtDefElement_0040234_0040PEAV12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FA0xd4acbafe_002E_003FbIgnoreAlways_0040_003F9_003F_003F_003F0ArtDefCollection_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVArtDef_0040234_0040PE_0024AAVArtDefElement_0040234_0040PEAV12_0040_0040Z_00404_NA && root == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_04NBFCGMPH_0040root_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HD_0040NAHIBPKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 21u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xd4acbafe_002E_003FbIgnoreAlways_0040_003F9_003F_003F_003F0ArtDefCollection_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVArtDef_0040234_0040PE_0024AAVArtDefElement_0040234_0040PEAV12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		CacheUnmanagedValues(artDefCollection);
	}

	private void _007EArtDefCollection()
	{
		Destroy();
		GC.SuppressFinalize(this);
	}

	private void _0021ArtDefCollection()
	{
		Destroy();
	}

	public unsafe virtual IArtDefElement AddElement(string name)
	{
		//IL_0089: Expected I, but got I8
		//IL_00cd: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		if (string.IsNullOrWhiteSpace(name))
		{
			if (!global::_003CModule_003E._003FA0xd4acbafe_002E_003FbIgnoreAlways_0040_003F6_003F_003FAddElement_0040ArtDefCollection_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIArtDefElement_0040345_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CP_0040GELAOPAH_0040Unable_003F5to_003F5create_003F5a_003F5new_003F5element_003F5w_0040), __arglist());
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CN_0040MHKGIICD_0040_003F_0024CB_003F_0024CC_003F_0024CBSystem_003F3_003F3String_003F3_003F3IsNullOrWhite_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HD_0040NAHIBPKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 57u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xd4acbafe_002E_003FbIgnoreAlways_0040_003F6_003F_003FAddElement_0040ArtDefCollection_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIArtDefElement_0040345_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
			return null;
		}
		global::AssetObjects.ArtDefCollection* collectionPointer = GetCollectionPointer();
		if (collectionPointer == null)
		{
			return null;
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(name);
		IArtDefElement artDefElement;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::AssetObjects.ArtDefElement* ptr = global::_003CModule_003E.AssetObjects_002EArtDefCollection_002ECreateNewElement(collectionPointer, standardStringWrapper.Value);
			if (!global::_003CModule_003E._003FA0xd4acbafe_002E_003FbIgnoreAlways_0040_003FBA_0040_003F_003FAddElement_0040ArtDefCollection_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIArtDefElement_0040345_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040EKADOGKM_0040nativeElement_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HD_0040NAHIBPKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 66u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xd4acbafe_002E_003FbIgnoreAlways_0040_003FBA_0040_003F_003FAddElement_0040ArtDefCollection_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIArtDefElement_0040345_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			artDefElement = new ArtDefElement(m_root, this, ptr);
			m_artDefElements.Add(artDefElement);
			if (!global::_003CModule_003E._003FA0xd4acbafe_002E_003FbIgnoreAlways_0040_003FBH_0040_003F_003FAddElement_0040ArtDefCollection_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIArtDefElement_0040345_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && artDefElement == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040KNEGFMLA_0040artDefElement_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HD_0040NAHIBPKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 71u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xd4acbafe_002E_003FbIgnoreAlways_0040_003FBH_0040_003F_003FAddElement_0040ArtDefCollection_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIArtDefElement_0040345_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return artDefElement;
	}

	public unsafe virtual void RemoveElement(string name)
	{
		StandardStringWrapper standardStringWrapper = null;
		int num = 0;
		if (0 >= m_artDefElements.Count)
		{
			return;
		}
		while (!(m_artDefElements[num].Name == name))
		{
			num++;
			if (num >= m_artDefElements.Count)
			{
				return;
			}
		}
		global::AssetObjects.ArtDefCollection* collectionPointer = GetCollectionPointer();
		if (collectionPointer != null)
		{
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(name);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EArtDefCollection_002ERemoveElement(collectionPointer, standardStringWrapper.Value);
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper).Dispose();
		}
		m_artDefElements.RemoveAt(num);
	}

	internal unsafe void Destroy()
	{
		//IL_0057: Expected I, but got I8
		foreach (ArtDefElement artDefElement in m_artDefElements)
		{
			artDefElement.Destroy();
		}
		m_artDefElements.Clear();
		m_collectionName = string.Empty;
		m_root = null;
		m_lastValidCollection = null;
	}

	internal void BuildCollectionStack(Stack<string> nameCollection)
	{
		nameCollection.Push(m_collectionName);
		m_parent?.BuildCollectionStack(nameCollection);
	}

	private unsafe global::AssetObjects.ArtDefCollection* GetCollectionPointer()
	{
		Stack<string> stack = new Stack<string>();
		BuildCollectionStack(stack);
		global::AssetObjects.ArtDefCollection* ptr = m_root.FindChildCollection(stack);
		byte condition = (byte)(((long)(nint)ptr != 0) ? 1 : 0);
		BugSubmitter.SilentAssert(condition != 0, string.Format("Failed to find collection at path: {0} @assign bwhitman @summary Failed to find collection from path", string.Join("::", stack)));
		return ptr;
	}

	private unsafe void UpdateCachedValuesIfNecessary(global::AssetObjects.ArtDefCollection* collection)
	{
		if (m_lastValidCollection != collection)
		{
			m_lastValidCollection = collection;
			CacheUnmanagedValues(collection);
		}
	}

	private unsafe void CacheUnmanagedValues(global::AssetObjects.ArtDefCollection* collection)
	{
		List<IArtDefElement> list = new List<IArtDefElement>();
		if (collection != null)
		{
			m_collectionName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EArtDefCollection_002EGetName(collection));
			System.Runtime.CompilerServices.Unsafe.SkipInit(out IteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AArtDefElement_0020_002A_002C4096_003E_003A_003Aiterator_002CAssetObjects_003A_003AArtDefElement_002CAssetObjects_003A_003AArtDefCollection_003A_003ADereference_003E obj);
			global::_003CModule_003E.AssetObjects_002EArtDefCollection_002Ebegin(collection, &obj);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out IteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AArtDefElement_0020_002A_002C4096_003E_003A_003Aiterator_002CAssetObjects_003A_003AArtDefElement_002CAssetObjects_003A_003AArtDefCollection_003A_003ADereference_003E obj2);
			if (global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AArtDefElement_0020_002A_002C4096_003E_003A_003Aiterator_002CAssetObjects_003A_003AArtDefElement_002CAssetObjects_003A_003AArtDefCollection_003A_003ADereference_003E_002E_0021_003D(&obj, global::_003CModule_003E.AssetObjects_002EArtDefCollection_002Eend(collection, &obj2)))
			{
				do
				{
					global::AssetObjects.ArtDefElement* pkArtDefElem = global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AArtDefElement_0020_002A_002C4096_003E_003A_003Aiterator_002CAssetObjects_003A_003AArtDefElement_002CAssetObjects_003A_003AArtDefCollection_003A_003ADereference_003E_002E_002A(&obj);
					IArtDefElement item = new ArtDefElement(m_root, this, pkArtDefElem);
					list.Add(item);
					global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AArtDefElement_0020_002A_002C4096_003E_003A_003Aiterator_002CAssetObjects_003A_003AArtDefElement_002CAssetObjects_003A_003AArtDefCollection_003A_003ADereference_003E_002E_002B_002B(&obj);
				}
				while (global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AArtDefElement_0020_002A_002C4096_003E_003A_003Aiterator_002CAssetObjects_003A_003AArtDefElement_002CAssetObjects_003A_003AArtDefCollection_003A_003ADereference_003E_002E_0021_003D(&obj, global::_003CModule_003E.AssetObjects_002EArtDefCollection_002Eend(collection, &obj2)));
			}
		}
		m_artDefElements = list;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EArtDefCollection();
			return;
		}
		try
		{
			_0021ArtDefCollection();
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

	~ArtDefCollection()
	{
		Dispose(A_0: false);
	}
}
