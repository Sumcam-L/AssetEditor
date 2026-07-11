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

public class ReadOnlyArtDefElement : IArtDefElement, IDisposable
{
	private IList<IArtDefCollection> m_childCollections;

	private IValueSet m_fields;

	private string m_name;

	private unsafe global::AssetObjects.ArtDefElement* m_artDefElement;

	private bool m_isValid;

	public virtual IEnumerable<IArtDefCollection> Children
	{
		get
		{
			if (m_isValid)
			{
				return m_childCollections;
			}
			return Enumerable.Empty<IArtDefCollection>();
		}
	}

	public unsafe virtual bool AppendMergedParameterCollections
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			if (m_isValid)
			{
				return global::_003CModule_003E.AssetObjects_002EArtDefElement_002EGetAppendMergedParameterCollections(m_artDefElement);
			}
			return false;
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			if (m_isValid)
			{
				global::_003CModule_003E.AssetObjects_002EArtDefElement_002ESetAppendMergedParameterCollections(m_artDefElement, value);
			}
		}
	}

	public virtual IValueSet Fields => m_fields;

	public unsafe virtual string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			if (!global::_003CModule_003E._003FA0x1c777759_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040Name_0040ReadOnlyArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EF_0040BKDHKAIJ_0040Unable_003F5to_003F5set_003F5the_003F5name_003F5on_003F5a_003F5Read_0040), __arglist());
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HI_0040KENCNPLI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 164u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1c777759_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040Name_0040ReadOnlyArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
		}
	}

	public unsafe ReadOnlyArtDefElement(global::AssetObjects.ArtDefElement* artDefElement)
	{
		//IL_004d: Expected I, but got I8
		m_childCollections = new List<IArtDefCollection>();
		m_name = string.Empty;
		m_artDefElement = artDefElement;
		m_isValid = true;
		base._002Ector();
		if (!global::_003CModule_003E._003FA0x1c777759_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ReadOnlyArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVArtDefElement_00402_0040_0040Z_00404_NA && artDefElement == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040KNEGFMLA_0040artDefElement_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HI_0040KENCNPLI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 17u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1c777759_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ReadOnlyArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVArtDefElement_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		m_name = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EArtDefElement_002EGetName(m_artDefElement));
		global::AssetObjects.ValueSet* valueSet = global::_003CModule_003E.AssetObjects_002EArtDefElement_002EGetFieldValues(m_artDefElement);
		m_fields = new ReadOnlyValueSet(valueSet);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EArtDefElement_002Ebegin(m_artDefElement, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EArtDefElement_002Eend(m_artDefElement, &iterator2)))
		{
			do
			{
				IArtDefCollection item = new ReadOnlyArtDefCollection(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_002A(&iterator));
				m_childCollections.Add(item);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EArtDefElement_002Eend(m_artDefElement, &iterator2)));
		}
	}

	private void _007EReadOnlyArtDefElement()
	{
		Destroy();
		GC.SuppressFinalize(this);
	}

	private void _0021ReadOnlyArtDefElement()
	{
		Destroy();
	}

	public unsafe virtual IArtDefCollection AddCollection(string name, IArtDefElementTemplate tmpl)
	{
		if (!global::_003CModule_003E._003FA0x1c777759_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddCollection_0040ReadOnlyArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIArtDefCollection_0040345_0040PE_0024AAVString_0040System_0040_0040PE_0024AAUIArtDefElementTemplate_0040345_0040_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0ED_0040BFCDPBGF_0040Cannot_003F5AddCollection_003F5to_003F5a_003F5ReadOn_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HI_0040KENCNPLI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 148u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1c777759_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddCollection_0040ReadOnlyArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIArtDefCollection_0040345_0040PE_0024AAVString_0040System_0040_0040PE_0024AAUIArtDefElementTemplate_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		return null;
	}

	public unsafe virtual void RemoveCollection(string name)
	{
		if (!global::_003CModule_003E._003FA0x1c777759_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemoveCollection_0040ReadOnlyArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EG_0040KCNICLNK_0040Cannot_003F5RemoveCollection_003F5to_003F5a_003F5Rea_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HI_0040KENCNPLI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 154u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1c777759_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemoveCollection_0040ReadOnlyArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
	}

	public unsafe virtual void UpdateCollectionsFromTemplate(IArtDefElementTemplate artDefTmpl)
	{
		if (!global::_003CModule_003E._003FA0x1c777759_002E_003FbIgnoreAlways_0040_003F2_003F_003FUpdateCollectionsFromTemplate_0040ReadOnlyArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIArtDefElementTemplate_0040345_0040_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EL_0040BGOAEBBF_0040Cannot_003F5update_003F5the_003F5ReadOnlyArtDef_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HI_0040KENCNPLI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 159u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1c777759_002E_003FbIgnoreAlways_0040_003F2_003F_003FUpdateCollectionsFromTemplate_0040ReadOnlyArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIArtDefElementTemplate_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
	}

	public unsafe virtual string SerializeToXML()
	{
		if (!global::_003CModule_003E._003FA0x1c777759_002E_003FbIgnoreAlways_0040_003F2_003F_003FSerializeToXML_0040ReadOnlyArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVString_0040System_0040_0040XZ_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DO_0040NPICHCOH_0040Unable_003F5to_003F5serialize_003F5ReadOnlyArtD_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HI_0040KENCNPLI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 169u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1c777759_002E_003FbIgnoreAlways_0040_003F2_003F_003FSerializeToXML_0040ReadOnlyArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVString_0040System_0040_0040XZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		return string.Empty;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool DeserializeFromXML(string xml)
	{
		if (!global::_003CModule_003E._003FA0x1c777759_002E_003FbIgnoreAlways_0040_003F2_003F_003FDeserializeFromXML_0040ReadOnlyArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_NPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EA_0040ILGFHFCO_0040Unable_003F5to_003F5deserialize_003F5ReadOnlyAr_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HI_0040KENCNPLI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 175u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1c777759_002E_003FbIgnoreAlways_0040_003F2_003F_003FDeserializeFromXML_0040ReadOnlyArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_NPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		return false;
	}

	internal void Destroy()
	{
		m_isValid = false;
		m_name = string.Empty;
		((ReadOnlyValueSet)m_fields).Destroy();
		foreach (ReadOnlyArtDefCollection childCollection in m_childCollections)
		{
			childCollection.Destroy();
		}
		m_childCollections.Clear();
	}

	internal void Invalidate()
	{
		m_isValid = false;
		((ReadOnlyValueSet)m_fields).Invalidate();
		foreach (ReadOnlyArtDefCollection childCollection in m_childCollections)
		{
			childCollection.Invalidate();
		}
		m_childCollections.Clear();
	}

	internal unsafe void ResolveReferences()
	{
		global::AssetObjects.ArtDefElement* artDefElement = m_artDefElement;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EArtDefElement_002Ebegin(artDefElement, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E.iterator iterator2);
		global::_003CModule_003E.AssetObjects_002EArtDefElement_002Eend(artDefElement, &iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, &iterator2))
		{
			do
			{
				global::AssetObjects.ArtDefCollection* ptr = global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_002A(&iterator);
				ReadOnlyArtDefCollection readOnlyArtDefCollection = FindArtDefCollection(ptr);
				if (readOnlyArtDefCollection != null)
				{
					readOnlyArtDefCollection.ResolveReferences();
				}
				else
				{
					IArtDefCollection item = new ReadOnlyArtDefCollection(ptr);
					m_childCollections.Add(item);
				}
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, &iterator2));
		}
		((ReadOnlyValueSet)m_fields).ResolveReferences();
	}

	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe bool IsBackedBy(global::AssetObjects.ArtDefElement* artDefElement)
	{
		return m_artDefElement == artDefElement;
	}

	private unsafe ReadOnlyArtDefCollection FindArtDefCollection(global::AssetObjects.ArtDefCollection* collection)
	{
		foreach (ReadOnlyArtDefCollection childCollection in m_childCollections)
		{
			if (childCollection.IsBackedBy(collection))
			{
				return childCollection;
			}
		}
		return null;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EReadOnlyArtDefElement();
			return;
		}
		try
		{
			_0021ReadOnlyArtDefElement();
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

	~ReadOnlyArtDefElement()
	{
		Dispose(A_0: false);
	}
}
