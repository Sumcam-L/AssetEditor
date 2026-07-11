using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Platform;
using Serialization;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class ArtDefElement : IArtDefElement, IDisposable
{
	private IList<IArtDefCollection> m_childCollections;

	private IValueSet m_fields;

	private string m_name;

	private bool m_AppendMergedParameterCollections;

	private bool m_ReplaceMergedCollectionElements;

	private ArtDef m_root;

	private ArtDefCollection m_parent;

	private unsafe global::AssetObjects.ArtDefElement* m_lastValidElement;

	public unsafe virtual IEnumerable<IArtDefCollection> Children
	{
		get
		{
			global::AssetObjects.ArtDefElement* elementPointer = GetElementPointer();
			if (m_lastValidElement != elementPointer)
			{
				m_lastValidElement = elementPointer;
				CacheUnmanagedValues(elementPointer);
			}
			return m_childCollections;
		}
	}

	public unsafe virtual IValueSet Fields
	{
		get
		{
			global::AssetObjects.ArtDefElement* elementPointer = GetElementPointer();
			if (m_lastValidElement != elementPointer)
			{
				m_lastValidElement = elementPointer;
				CacheUnmanagedValues(elementPointer);
			}
			return m_fields;
		}
	}

	public unsafe virtual bool ReplaceMergedCollectionElements
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			global::AssetObjects.ArtDefElement* elementPointer = GetElementPointer();
			if (m_lastValidElement != elementPointer)
			{
				m_lastValidElement = elementPointer;
				CacheUnmanagedValues(elementPointer);
			}
			return m_ReplaceMergedCollectionElements;
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			if (m_ReplaceMergedCollectionElements != value)
			{
				m_ReplaceMergedCollectionElements = value;
				global::AssetObjects.ArtDefElement* elementPointer = GetElementPointer();
				if (elementPointer != null)
				{
					global::_003CModule_003E.AssetObjects_002EArtDefElement_002ESetReplaceMergedCollectionElements(elementPointer, value);
				}
			}
		}
	}

	public unsafe virtual bool AppendMergedParameterCollections
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			global::AssetObjects.ArtDefElement* elementPointer = GetElementPointer();
			if (m_lastValidElement != elementPointer)
			{
				m_lastValidElement = elementPointer;
				CacheUnmanagedValues(elementPointer);
			}
			return m_AppendMergedParameterCollections;
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			if (m_AppendMergedParameterCollections != value)
			{
				m_AppendMergedParameterCollections = value;
				global::AssetObjects.ArtDefElement* elementPointer = GetElementPointer();
				if (elementPointer != null)
				{
					global::_003CModule_003E.AssetObjects_002EArtDefElement_002ESetAppendMergedParameterCollections(elementPointer, value);
				}
			}
		}
	}

	public unsafe virtual string Name
	{
		get
		{
			global::AssetObjects.ArtDefElement* elementPointer = GetElementPointer();
			if (m_lastValidElement != elementPointer)
			{
				m_lastValidElement = elementPointer;
				CacheUnmanagedValues(elementPointer);
			}
			return m_name;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			global::AssetObjects.ArtDefElement* elementPointer = GetElementPointer();
			if (elementPointer != null)
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002EArtDefElement_002ESetName(elementPointer, standardStringWrapper.Value);
				}
				catch
				{
					//try-fault
					((IDisposable)standardStringWrapper).Dispose();
					throw;
				}
				((IDisposable)standardStringWrapper).Dispose();
			}
			m_name = value;
		}
	}

	public unsafe ArtDefElement(ArtDef root, ArtDefCollection parent, global::AssetObjects.ArtDefElement* pkArtDefElem)
	{
		//IL_0054: Expected I, but got I8
		//IL_007a: Expected I, but got I8
		//IL_00a0: Expected I, but got I8
		m_childCollections = new List<IArtDefCollection>();
		m_name = string.Empty;
		m_root = root;
		m_parent = parent;
		m_lastValidElement = pkArtDefElem;
		base._002Ector();
		if (!global::_003CModule_003E._003FA0xb7eb6188_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVArtDef_0040234_0040PE_0024AAVArtDefCollection_0040234_0040PEAV12_0040_0040Z_00404_NA && pkArtDefElem == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040KNEGFMLA_0040artDefElement_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040KLNKNPAJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 21u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xb7eb6188_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVArtDef_0040234_0040PE_0024AAVArtDefCollection_0040234_0040PEAV12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FA0xb7eb6188_002E_003FbIgnoreAlways_0040_003F9_003F_003F_003F0ArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVArtDef_0040234_0040PE_0024AAVArtDefCollection_0040234_0040PEAV12_0040_0040Z_00404_NA && parent == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_06MLKDMCBD_0040parent_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040KLNKNPAJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 22u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xb7eb6188_002E_003FbIgnoreAlways_0040_003F9_003F_003F_003F0ArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVArtDef_0040234_0040PE_0024AAVArtDefCollection_0040234_0040PEAV12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FA0xb7eb6188_002E_003FbIgnoreAlways_0040_003FBB_0040_003F_003F_003F0ArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVArtDef_0040234_0040PE_0024AAVArtDefCollection_0040234_0040PEAV12_0040_0040Z_00404_NA && root == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_04NBFCGMPH_0040root_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040KLNKNPAJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 23u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xb7eb6188_002E_003FbIgnoreAlways_0040_003FBB_0040_003F_003F_003F0ArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVArtDef_0040234_0040PE_0024AAVArtDefCollection_0040234_0040PEAV12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		CacheUnmanagedValues(pkArtDefElem);
	}

	private void _007EArtDefElement()
	{
		Destroy();
		GC.SuppressFinalize(this);
	}

	private void _0021ArtDefElement()
	{
		Destroy();
	}

	public unsafe virtual IArtDefCollection AddCollection(string name, IArtDefElementTemplate tmpl)
	{
		//IL_002e: Expected I, but got I8
		//IL_005d: Expected I, but got I8
		//IL_008b: Expected I, but got I8
		//IL_00cb: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		global::AssetObjects.ArtDefElement* elementPointer = GetElementPointer();
		if (!global::_003CModule_003E._003FA0xb7eb6188_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddCollection_0040ArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIArtDefCollection_0040345_0040PE_0024AAVString_0040System_0040_0040PE_0024AAUIArtDefElementTemplate_0040345_0040_0040Z_00404_NA && elementPointer == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_07HCLJNICE_0040element_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040KLNKNPAJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 77u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xb7eb6188_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddCollection_0040ArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIArtDefCollection_0040345_0040PE_0024AAVString_0040System_0040_0040PE_0024AAUIArtDefElementTemplate_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		ArtDefElementTemplate artDefElementTemplate = (ArtDefElementTemplate)tmpl;
		if (!global::_003CModule_003E._003FA0xb7eb6188_002E_003FbIgnoreAlways_0040_003F9_003F_003FAddCollection_0040ArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIArtDefCollection_0040345_0040PE_0024AAVString_0040System_0040_0040PE_0024AAUIArtDefElementTemplate_0040345_0040_0040Z_00404_NA && artDefElementTemplate == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BF_0040JBEDMNBO_0040castedArtDefTemplate_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040KLNKNPAJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 80u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xb7eb6188_002E_003FbIgnoreAlways_0040_003F9_003F_003FAddCollection_0040ArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIArtDefCollection_0040345_0040PE_0024AAVString_0040System_0040_0040PE_0024AAUIArtDefElementTemplate_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		global::AssetObjects.ArtDefElementTemplate* nativeTemplate = artDefElementTemplate.GetNativeTemplate();
		if (!global::_003CModule_003E._003FA0xb7eb6188_002E_003FbIgnoreAlways_0040_003FBB_0040_003F_003FAddCollection_0040ArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIArtDefCollection_0040345_0040PE_0024AAVString_0040System_0040_0040PE_0024AAUIArtDefElementTemplate_0040345_0040_0040Z_00404_NA && nativeTemplate == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BC_0040IMALNAPA_0040rawArtDefTemplate_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040KLNKNPAJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 83u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xb7eb6188_002E_003FbIgnoreAlways_0040_003FBB_0040_003F_003FAddCollection_0040ArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIArtDefCollection_0040345_0040PE_0024AAVString_0040System_0040_0040PE_0024AAUIArtDefElementTemplate_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(name);
		IArtDefCollection artDefCollection;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::AssetObjects.ArtDefCollection* ptr = global::_003CModule_003E.AssetObjects_002EArtDefElement_002EAddCollection(elementPointer, standardStringWrapper.Value, nativeTemplate);
			if (!global::_003CModule_003E._003FA0xb7eb6188_002E_003FbIgnoreAlways_0040_003FBI_0040_003F_003FAddCollection_0040ArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIArtDefCollection_0040345_0040PE_0024AAVString_0040System_0040_0040PE_0024AAUIArtDefElementTemplate_0040345_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040OFNGAMJH_0040collection_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040KLNKNPAJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 87u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xb7eb6188_002E_003FbIgnoreAlways_0040_003FBI_0040_003F_003FAddCollection_0040ArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIArtDefCollection_0040345_0040PE_0024AAVString_0040System_0040_0040PE_0024AAUIArtDefElementTemplate_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			artDefCollection = new ArtDefCollection(m_root, this, ptr);
			m_childCollections.Add(artDefCollection);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return artDefCollection;
	}

	public unsafe virtual void RemoveCollection(string name)
	{
		//IL_002c: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		global::AssetObjects.ArtDefElement* elementPointer = GetElementPointer();
		if (!global::_003CModule_003E._003FA0xb7eb6188_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemoveCollection_0040ArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && elementPointer == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_07HCLJNICE_0040element_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040KLNKNPAJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 102u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xb7eb6188_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemoveCollection_0040ArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		foreach (IArtDefCollection childCollection in m_childCollections)
		{
			if (childCollection.CollectionName == name)
			{
				m_childCollections.Remove(childCollection);
				break;
			}
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(name);
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::_003CModule_003E.AssetObjects_002EArtDefElement_002ERemoveCollection(elementPointer, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
	}

	public unsafe virtual string SerializeToXML()
	{
		global::AssetObjects.ArtDefElement* elementPointer = GetElementPointer();
		if (elementPointer != null)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out global::AssetObjects.String obj);
			global::_003CModule_003E.AssetObjects_002EString_002E_007Bctor_007D(&obj);
			string result;
			try
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out Serializer serializer);
				global::_003CModule_003E.Serialization_002EContext_002E_007Bctor_007D((Context*)(&serializer));
				try
				{
					global::_003CModule_003E.Platform_002EAllocator_002E_007Bctor_007D((Allocator*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref serializer, 312)));
					System.Runtime.CompilerServices.Unsafe.As<Serializer, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref serializer, 312)) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024HeapAllocator_0040_00240BH_0040_00240A_0040_0040Platform_0040_00406B_0040);
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Context*, void>)(&global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D), &serializer);
					throw;
				}
				try
				{
					global::_003CModule_003E.AssetObjects_002ESerializer_002ESerializeIntoString_003Cclass_0020AssetObjects_003A_003AArtDefElement_003E(&serializer, &obj, elementPointer);
					result = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(&obj));
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Serializer*, void>)(&global::_003CModule_003E.AssetObjects_002ESerializer_002E_007Bdtor_007D), &serializer);
					throw;
				}
				global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D((Context*)(&serializer));
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
		return string.Empty;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool DeserializeFromXML(string xml)
	{
		StandardStringWrapper standardStringWrapper = null;
		global::AssetObjects.ArtDefElement* elementPointer = GetElementPointer();
		if (elementPointer != null)
		{
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(xml);
			bool flag;
			try
			{
				standardStringWrapper = standardStringWrapper2;
				System.Runtime.CompilerServices.Unsafe.SkipInit(out ResultCode resultCode);
				flag = global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(global::_003CModule_003E.AssetObjects_002EDeserializer_002EDeserialize_003Cclass_0020AssetObjects_003A_003AArtDefElement_003E(m_root.Deserializer, &resultCode, elementPointer, standardStringWrapper.Value));
				if (flag)
				{
					m_lastValidElement = elementPointer;
					CacheUnmanagedValues(elementPointer);
				}
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper).Dispose();
			return flag;
		}
		return false;
	}

	public unsafe virtual void UpdateCollectionsFromTemplate(IArtDefElementTemplate artDefTmpl)
	{
		//IL_0037: Expected I, but got I8
		//IL_0064: Expected I, but got I8
		global::AssetObjects.ArtDefElement* elementPointer = GetElementPointer();
		if (elementPointer == null)
		{
			return;
		}
		ArtDefElementTemplate artDefElementTemplate = (ArtDefElementTemplate)artDefTmpl;
		if (!global::_003CModule_003E._003FA0xb7eb6188_002E_003FbIgnoreAlways_0040_003F5_003F_003FUpdateCollectionsFromTemplate_0040ArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIArtDefElementTemplate_0040345_0040_0040Z_00404_NA && artDefElementTemplate == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BF_0040JBEDMNBO_0040castedArtDefTemplate_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040KLNKNPAJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 123u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xb7eb6188_002E_003FbIgnoreAlways_0040_003F5_003F_003FUpdateCollectionsFromTemplate_0040ArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIArtDefElementTemplate_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		global::AssetObjects.ArtDefElementTemplate* nativeTemplate = artDefElementTemplate.GetNativeTemplate();
		if (!global::_003CModule_003E._003FA0xb7eb6188_002E_003FbIgnoreAlways_0040_003FN_0040_003F_003FUpdateCollectionsFromTemplate_0040ArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIArtDefElementTemplate_0040345_0040_0040Z_00404_NA && nativeTemplate == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BC_0040IMALNAPA_0040rawArtDefTemplate_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040KLNKNPAJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 126u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xb7eb6188_002E_003FbIgnoreAlways_0040_003FN_0040_003F_003FUpdateCollectionsFromTemplate_0040ArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIArtDefElementTemplate_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		global::_003CModule_003E.AssetObjects_002EArtDefElement_002EUpdateCollectionsFromTemplate(elementPointer, nativeTemplate);
		List<IArtDefCollection> list = new List<IArtDefCollection>();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EArtDefElement_002Ebegin(elementPointer, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EArtDefElement_002Eend(elementPointer, &iterator2)))
		{
			do
			{
				global::AssetObjects.ArtDefCollection* artDefCollection = global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_002A(&iterator);
				IArtDefCollection item = new ArtDefCollection(m_root, this, artDefCollection);
				list.Add(item);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EArtDefElement_002Eend(elementPointer, &iterator2)));
		}
		m_childCollections = list;
	}

	internal unsafe void Destroy()
	{
		//IL_0057: Expected I, but got I8
		m_name = string.Empty;
		m_fields = null;
		foreach (ArtDefCollection childCollection in m_childCollections)
		{
			childCollection.Destroy();
		}
		m_childCollections.Clear();
		m_lastValidElement = null;
		m_parent = null;
	}

	internal unsafe void BuildCollectionStack(Stack<string> nameCollection)
	{
		//IL_002b: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0xb7eb6188_002E_003FbIgnoreAlways_0040_003F2_003F_003FBuildCollectionStack_0040ArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPE_0024AAV_003F_0024Stack_0040PE_0024AAVString_0040System_0040_0040_0040Generic_0040Collections_0040System_0040_0040_0040Z_00404_NA && m_parent == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_08JBOMIOFK_0040m_parent_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040KLNKNPAJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 289u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xb7eb6188_002E_003FbIgnoreAlways_0040_003F2_003F_003FBuildCollectionStack_0040ArtDefElement_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPE_0024AAV_003F_0024Stack_0040PE_0024AAVString_0040System_0040_0040_0040Generic_0040Collections_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		nameCollection.Push(m_name);
		m_parent.BuildCollectionStack(nameCollection);
	}

	[return: MarshalAs(UnmanagedType.U1)]
	private static bool EqualsCollectionName(IArtDefCollection col, string name)
	{
		return col.CollectionName == name;
	}

	private unsafe global::AssetObjects.ArtDefElement* GetElementPointer()
	{
		Stack<string> nameCollection = new Stack<string>();
		BuildCollectionStack(nameCollection);
		return m_root.FindChildElement(nameCollection);
	}

	private unsafe void UpdateCachedValuesIfNecessary(global::AssetObjects.ArtDefElement* element)
	{
		if (m_lastValidElement != element)
		{
			m_lastValidElement = element;
			CacheUnmanagedValues(element);
		}
	}

	private unsafe void CacheUnmanagedValues(global::AssetObjects.ArtDefElement* element)
	{
		List<IArtDefCollection> list = new List<IArtDefCollection>();
		if (element != null)
		{
			m_name = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EArtDefElement_002EGetName(element));
			global::AssetObjects.ValueSet* pkValues = global::_003CModule_003E.AssetObjects_002EArtDefElement_002EGetFieldValues(element);
			m_fields = new ValueSet(pkValues, m_root.Deserializer);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E.iterator iterator);
			global::_003CModule_003E.AssetObjects_002EArtDefElement_002Ebegin(element, &iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E.iterator iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EArtDefElement_002Eend(element, &iterator2)))
			{
				do
				{
					global::AssetObjects.ArtDefCollection* artDefCollection = global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_002A(&iterator);
					IArtDefCollection item = new ArtDefCollection(m_root, this, artDefCollection);
					list.Add(item);
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EArtDefElement_002Eend(element, &iterator2)));
			}
			m_AppendMergedParameterCollections = global::_003CModule_003E.AssetObjects_002EArtDefElement_002EGetAppendMergedParameterCollections(element);
			m_ReplaceMergedCollectionElements = global::_003CModule_003E.AssetObjects_002EArtDefElement_002EGetReplaceMergedCollectionElements(element);
		}
		else
		{
			IValueSet fields = m_fields;
			if (fields != null)
			{
				((ValueSet)fields).RemoveReferences(bDisposing: true);
				m_fields = null;
			}
		}
		m_childCollections = list;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EArtDefElement();
			return;
		}
		try
		{
			_0021ArtDefElement();
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

	~ArtDefElement()
	{
		Dispose(A_0: false);
	}
}
