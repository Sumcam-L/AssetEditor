using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Platform;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class ArtDefTemplateSet : IArtDefTemplateSet
{
	private IList<IArtDefTemplate> m_pmArtDefTemplates;

	private unsafe global::AssetObjects.ArtDefTemplateSet* m_pkArtDefTemplates;

	private unsafe global::AssetObjects.Deserializer* m_pkDeserializer;

	public virtual IEnumerable<IArtDefTemplate> Items => m_pmArtDefTemplates;

	public unsafe ArtDefTemplateSet(global::AssetObjects.ArtDefTemplateSet* pkArtDefTemplates, global::AssetObjects.Deserializer* pkDeserializer)
	{
		//IL_0047: Expected I, but got I8
		m_pmArtDefTemplates = new List<IArtDefTemplate>();
		m_pkArtDefTemplates = pkArtDefTemplates;
		m_pkDeserializer = pkDeserializer;
		base._002Ector();
		if (!global::_003CModule_003E._003FA0x91a901e8_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ArtDefTemplateSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040PEAVDeserializer_00402_0040_0040Z_00404_NA && m_pkArtDefTemplates == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BE_0040KHLMIKFF_0040m_pkArtDefTemplates_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040GKMBCDCB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 18u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x91a901e8_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ArtDefTemplateSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040PEAVDeserializer_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AArtDefTemplate_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EArtDefTemplateSet_002Ebegin(m_pkArtDefTemplates, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AArtDefTemplate_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefTemplate_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EArtDefTemplateSet_002Eend(m_pkArtDefTemplates, &iterator2)))
		{
			do
			{
				m_pmArtDefTemplates.Add(new ArtDefTemplate(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefTemplate_002C4096_003E_002Eiterator_002E_002A(&iterator), pkDeserializer));
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefTemplate_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefTemplate_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EArtDefTemplateSet_002Eend(m_pkArtDefTemplates, &iterator2)));
		}
	}

	public unsafe virtual T Push<T>() where T : IArtDefTemplate
	{
		object[] array = new object[2];
		object obj = Pointer.Box(m_pkArtDefTemplates, typeof(global::AssetObjects.ArtDefTemplateSet*));
		array[0] = obj;
		object obj2 = Pointer.Box(m_pkDeserializer, typeof(global::AssetObjects.Deserializer*));
		array[1] = obj2;
		Type[] array2 = new Type[2];
		Type typeFromHandle = typeof(global::AssetObjects.ArtDefTemplateSet*);
		array2[0] = typeFromHandle;
		Type typeFromHandle2 = typeof(global::AssetObjects.Deserializer*);
		array2[1] = typeFromHandle2;
		T val = global::_003CModule_003E.Firaxis_002ECivTech_002EReflectionHelperEx_002E_003FA0x91a901e8_002ETypeLoader<T>(Assembly.GetExecutingAssembly(), array, array2);
		m_pmArtDefTemplates.Add(val);
		return val;
	}

	public unsafe virtual void Clear()
	{
		m_pmArtDefTemplates.Clear();
		global::_003CModule_003E.AssetObjects_002EArtDefTemplateSet_002EClear(m_pkArtDefTemplates);
	}

	public unsafe virtual void Remove(IArtDefTemplate pmArtDefTemplate)
	{
		//IL_0023: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x91a901e8_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemove_0040ArtDefTemplateSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIArtDefTemplate_0040345_0040_0040Z_00404_NA && pmArtDefTemplate == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BM_0040FNHBENCL_0040pmArtDefTemplate_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040GKMBCDCB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 50u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x91a901e8_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemove_0040ArtDefTemplateSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIArtDefTemplate_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmArtDefTemplate.Name).ToPointer();
		global::_003CModule_003E.AssetObjects_002EArtDefTemplateSet_002ERemoveByName(m_pkArtDefTemplates, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		int num = m_pmArtDefTemplates.IndexOf(pmArtDefTemplate);
		if (num >= 0)
		{
			m_pmArtDefTemplates.RemoveAt(num);
		}
	}

	public virtual IList<IArtDefTemplate> GetTemplates()
	{
		return m_pmArtDefTemplates;
	}

	internal unsafe void AddReferences()
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AArtDefTemplate_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EArtDefTemplateSet_002Ebegin(m_pkArtDefTemplates, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AArtDefTemplate_002C4096_003E.iterator iterator2);
		global::_003CModule_003E.AssetObjects_002EArtDefTemplateSet_002Eend(m_pkArtDefTemplates, &iterator2);
		if (!global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefTemplate_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, &iterator2))
		{
			return;
		}
		do
		{
			ArtDefTemplate artDefTemplate = new ArtDefTemplate(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefTemplate_002C4096_003E_002Eiterator_002E_002A(&iterator), m_pkDeserializer);
			if (artDefTemplate != null)
			{
				m_pmArtDefTemplates.Add(artDefTemplate);
			}
			global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefTemplate_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
		}
		while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefTemplate_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, &iterator2));
	}

	internal unsafe void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool bDisposing)
	{
		//IL_0016: Expected I, but got I8
		//IL_001e: Expected I, but got I8
		m_pmArtDefTemplates.Clear();
		if (bDisposing)
		{
			m_pkArtDefTemplates = null;
			m_pkDeserializer = null;
			m_pmArtDefTemplates = null;
		}
	}
}
