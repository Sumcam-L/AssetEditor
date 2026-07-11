using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Platform;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class ReadOnlyArtDefCollection : IArtDefCollection, IDisposable
{
	private IList<IArtDefElement> m_artDefElements;

	private string m_collectionName;

	private unsafe global::AssetObjects.ArtDefCollection* m_artDefCollection;

	private bool m_isValid;

	public virtual bool ReplaceMergedCollectionElements
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return false;
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
		}
	}

	public virtual IEnumerable<IArtDefElement> Elements
	{
		get
		{
			if (m_isValid)
			{
				return m_artDefElements;
			}
			return Enumerable.Empty<IArtDefElement>();
		}
	}

	public unsafe virtual string CollectionName
	{
		get
		{
			return m_collectionName;
		}
		set
		{
			if (!global::_003CModule_003E._003FA0xbdc68fea_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040CollectionName_0040ReadOnlyArtDefCollection_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FD_0040ELBBMMGB_0040Unable_003F5to_003F5set_003F5the_003F5collection_003F5nam_0040), __arglist());
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HL_0040BKDPADLA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 121u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xbdc68fea_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040CollectionName_0040ReadOnlyArtDefCollection_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
		}
	}

	public unsafe ReadOnlyArtDefCollection(global::AssetObjects.ArtDefCollection* artDefCollection)
	{
		//IL_004d: Expected I, but got I8
		m_artDefElements = new List<IArtDefElement>();
		m_collectionName = string.Empty;
		m_artDefCollection = artDefCollection;
		m_isValid = true;
		base._002Ector();
		if (!global::_003CModule_003E._003FA0xbdc68fea_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ReadOnlyArtDefCollection_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVArtDefCollection_00402_0040_0040Z_00404_NA && artDefCollection == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BB_0040GNCBKOKG_0040artDefCollection_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HL_0040BKDPADLA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 16u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xbdc68fea_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ReadOnlyArtDefCollection_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVArtDefCollection_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		m_collectionName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EArtDefCollection_002EGetName(artDefCollection));
		System.Runtime.CompilerServices.Unsafe.SkipInit(out IteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AArtDefElement_0020_002A_002C4096_003E_003A_003Aiterator_002CAssetObjects_003A_003AArtDefElement_002CAssetObjects_003A_003AArtDefCollection_003A_003ADereference_003E obj);
		global::_003CModule_003E.AssetObjects_002EArtDefCollection_002Ebegin(artDefCollection, &obj);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out IteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AArtDefElement_0020_002A_002C4096_003E_003A_003Aiterator_002CAssetObjects_003A_003AArtDefElement_002CAssetObjects_003A_003AArtDefCollection_003A_003ADereference_003E obj2);
		if (global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AArtDefElement_0020_002A_002C4096_003E_003A_003Aiterator_002CAssetObjects_003A_003AArtDefElement_002CAssetObjects_003A_003AArtDefCollection_003A_003ADereference_003E_002E_0021_003D(&obj, global::_003CModule_003E.AssetObjects_002EArtDefCollection_002Eend(artDefCollection, &obj2)))
		{
			do
			{
				IArtDefElement item = new ReadOnlyArtDefElement(global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AArtDefElement_0020_002A_002C4096_003E_003A_003Aiterator_002CAssetObjects_003A_003AArtDefElement_002CAssetObjects_003A_003AArtDefCollection_003A_003ADereference_003E_002E_002A(&obj));
				m_artDefElements.Add(item);
				global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AArtDefElement_0020_002A_002C4096_003E_003A_003Aiterator_002CAssetObjects_003A_003AArtDefElement_002CAssetObjects_003A_003AArtDefCollection_003A_003ADereference_003E_002E_002B_002B(&obj);
			}
			while (global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AArtDefElement_0020_002A_002C4096_003E_003A_003Aiterator_002CAssetObjects_003A_003AArtDefElement_002CAssetObjects_003A_003AArtDefCollection_003A_003ADereference_003E_002E_0021_003D(&obj, global::_003CModule_003E.AssetObjects_002EArtDefCollection_002Eend(artDefCollection, &obj2)));
		}
	}

	private void _007EReadOnlyArtDefCollection()
	{
		Destroy();
		GC.SuppressFinalize(this);
	}

	private void _0021ReadOnlyArtDefCollection()
	{
		Destroy();
	}

	public unsafe virtual IArtDefElement AddElement(string name)
	{
		if (!global::_003CModule_003E._003FA0xbdc68fea_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddElement_0040ReadOnlyArtDefCollection_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIArtDefElement_0040345_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EI_0040COHBEPLG_0040Unable_003F5to_003F5add_003F5elements_003F5to_003F5a_003F5Read_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HL_0040BKDPADLA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 127u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xbdc68fea_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddElement_0040ReadOnlyArtDefCollection_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIArtDefElement_0040345_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		return null;
	}

	public unsafe virtual void RemoveElement(string name)
	{
		if (!global::_003CModule_003E._003FA0xbdc68fea_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemoveElement_0040ReadOnlyArtDefCollection_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EN_0040PGNIOLAJ_0040Unable_003F5to_003F5remove_003F5elements_003F5from_003F5a_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HL_0040BKDPADLA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 133u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xbdc68fea_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemoveElement_0040ReadOnlyArtDefCollection_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
	}

	internal void Destroy()
	{
		m_isValid = false;
		foreach (ReadOnlyArtDefElement artDefElement in m_artDefElements)
		{
			artDefElement.Destroy();
		}
		m_artDefElements.Clear();
		m_collectionName = string.Empty;
	}

	internal void Invalidate()
	{
		m_isValid = false;
		foreach (ReadOnlyArtDefElement artDefElement in m_artDefElements)
		{
			artDefElement.Invalidate();
		}
		m_artDefElements.Clear();
	}

	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe bool IsBackedBy(global::AssetObjects.ArtDefCollection* artDefCollection)
	{
		return m_artDefCollection == artDefCollection;
	}

	internal unsafe void ResolveReferences()
	{
		global::AssetObjects.ArtDefCollection* artDefCollection = m_artDefCollection;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out IteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AArtDefElement_0020_002A_002C4096_003E_003A_003Aiterator_002CAssetObjects_003A_003AArtDefElement_002CAssetObjects_003A_003AArtDefCollection_003A_003ADereference_003E obj);
		global::_003CModule_003E.AssetObjects_002EArtDefCollection_002Ebegin(artDefCollection, &obj);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out IteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AArtDefElement_0020_002A_002C4096_003E_003A_003Aiterator_002CAssetObjects_003A_003AArtDefElement_002CAssetObjects_003A_003AArtDefCollection_003A_003ADereference_003E obj2);
		global::_003CModule_003E.AssetObjects_002EArtDefCollection_002Eend(artDefCollection, &obj2);
		if (!global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AArtDefElement_0020_002A_002C4096_003E_003A_003Aiterator_002CAssetObjects_003A_003AArtDefElement_002CAssetObjects_003A_003AArtDefCollection_003A_003ADereference_003E_002E_0021_003D(&obj, &obj2))
		{
			return;
		}
		do
		{
			global::AssetObjects.ArtDefElement* ptr = global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AArtDefElement_0020_002A_002C4096_003E_003A_003Aiterator_002CAssetObjects_003A_003AArtDefElement_002CAssetObjects_003A_003AArtDefCollection_003A_003ADereference_003E_002E_002A(&obj);
			ReadOnlyArtDefElement readOnlyArtDefElement = FindArtDefElement(ptr);
			if (readOnlyArtDefElement != null)
			{
				readOnlyArtDefElement.ResolveReferences();
			}
			else
			{
				IArtDefElement item = new ReadOnlyArtDefElement(ptr);
				m_artDefElements.Add(item);
			}
			global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AArtDefElement_0020_002A_002C4096_003E_003A_003Aiterator_002CAssetObjects_003A_003AArtDefElement_002CAssetObjects_003A_003AArtDefCollection_003A_003ADereference_003E_002E_002B_002B(&obj);
		}
		while (global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AArtDefElement_0020_002A_002C4096_003E_003A_003Aiterator_002CAssetObjects_003A_003AArtDefElement_002CAssetObjects_003A_003AArtDefCollection_003A_003ADereference_003E_002E_0021_003D(&obj, &obj2));
	}

	private unsafe ReadOnlyArtDefElement FindArtDefElement(global::AssetObjects.ArtDefElement* element)
	{
		foreach (ReadOnlyArtDefElement artDefElement in m_artDefElements)
		{
			if (artDefElement.IsBackedBy(element))
			{
				return artDefElement;
			}
		}
		return null;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EReadOnlyArtDefCollection();
			return;
		}
		try
		{
			_0021ReadOnlyArtDefCollection();
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

	~ReadOnlyArtDefCollection()
	{
		Dispose(A_0: false);
	}
}
