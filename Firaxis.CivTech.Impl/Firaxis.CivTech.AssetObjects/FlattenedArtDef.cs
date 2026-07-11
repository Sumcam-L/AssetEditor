using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Firaxis.CivTech.Packages;
using Firaxis.Error;
using Platform;
using Serialization;
using std;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class FlattenedArtDef : IFlattenedArtDef
{
	private string m_templateName = "";

	private readonly ArtDefWrapper m_artDefSet;

	private ICollection<IArtDef> m_artDefs;

	private ICollection<IArtDefCollection> m_artDefCollections;

	private IArtDef[] m_paramHolder;

	private unsafe string PrivateTemplateName
	{
		get
		{
			return m_templateName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			if (string.IsNullOrEmpty(m_templateName))
			{
				m_templateName = value;
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002EArtDefSet_002ESetTemplateName(m_artDefSet.ArtDefSet, standardStringWrapper.Value);
				}
				catch
				{
					//try-fault
					((IDisposable)standardStringWrapper).Dispose();
					throw;
				}
				((IDisposable)standardStringWrapper).Dispose();
			}
			else if (!global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F7_003F_003Fset_0040PrivateTemplateName_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && !(m_templateName == value))
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DE_0040CFFFFOON_0040A_003F5FlattenedArtDef_003F5must_003F5represent_0040), __arglist());
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BP_0040MIBEAEHG_0040m_templateName_003F5_003F_0024DN_003F_0024DN_003F5templateName_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040MIHHDGDL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 116u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F7_003F_003Fset_0040PrivateTemplateName_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
		}
	}

	public virtual Version Version => new Version(1, 0, 0, 0);

	public virtual IEnumerable<BLPInfo> PackageReferences => Enumerable.Empty<BLPInfo>();

	public virtual IEnumerable<IArtDefCollection> RootCollections => m_artDefCollections;

	public unsafe virtual string ArtDefTemplate
	{
		get
		{
			return m_templateName;
		}
		set
		{
			if (!global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040ArtDefTemplate_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EM_0040EJBAGHNK_0040Unable_003F5to_003F5change_003F5the_003F5template_003F5na_0040), __arglist());
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040MIHHDGDL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 252u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040ArtDefTemplate_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
		}
	}

	public FlattenedArtDef()
	{
		ArtDefWrapper artDefSet = new ArtDefWrapper();
		try
		{
			m_artDefSet = artDefSet;
			m_artDefs = new List<IArtDef>();
			m_artDefCollections = new List<IArtDefCollection>();
			m_paramHolder = new IArtDef[1];
			base._002Ector();
			return;
		}
		catch
		{
			//try-fault
			((IDisposable)m_artDefSet).Dispose();
			throw;
		}
	}

	private void _007EFlattenedArtDef()
	{
		Destroy();
		GC.SuppressFinalize(this);
	}

	private void _0021FlattenedArtDef()
	{
		Destroy();
	}

	public virtual void AddArtDef(IArtDef artDef)
	{
		m_paramHolder[0] = artDef;
		AddArtDefs(m_paramHolder);
	}

	public unsafe virtual void AddArtDefs(IEnumerable<IArtDef> artDefs)
	{
		ArtDef artDef = null;
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
		foreach (IArtDef artDef2 in artDefs)
		{
			PrivateTemplateName = artDef2.ArtDefTemplate;
			try
			{
				artDef = (ArtDef)artDef2;
				m_artDefs.Add(artDef2);
				ArtDefSet* artDefSet = artDef.ArtDefSet;
				global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002EHashArtDef(artDefSet);
				global::_003CModule_003E.AssetObjects_002EArtDefSet_002EMergeInto(artDefSet, m_artDefSet.ArtDefSet);
			}
			catch (InvalidCastException ex)
			{
				StandardStringWrapper standardStringWrapper3 = new StandardStringWrapper($"Attempted to add an ArtDef of an unexpected type.  Error:  {ex.ToString()}  @assign bwhitman  @summary castexception");
				try
				{
					standardStringWrapper = standardStringWrapper3;
					if (!global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F9_003F_003FAddArtDefs_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAU_003F_0024IEnumerable_0040PE_0024AAUIArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040_0040Generic_0040Collections_0040System_0040_0040_0040Z_00404_NA)
					{
						global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, standardStringWrapper.Value, __arglist());
						if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040MIHHDGDL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 67u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F9_003F_003FAddArtDefs_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAU_003F_0024IEnumerable_0040PE_0024AAUIArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040_0040Generic_0040Collections_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
						{
							/*OpCode not supported: DebugBreak*/;
						}
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
		}
		if (string.IsNullOrEmpty(m_templateName))
		{
			return;
		}
		if (m_artDefCollections.Count == 0)
		{
			StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(m_templateName);
			try
			{
				standardStringWrapper2 = standardStringWrapper4;
				Container_003CAssetObjects_003A_003AArtDefCollection_003E* ptr = global::_003CModule_003E.AssetObjects_002EArtDefSet_002EGetMergedContainerCollection(m_artDefSet.ArtDefSet, standardStringWrapper2.Value);
				if (ptr != null)
				{
					System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E.iterator iterator);
					global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AArtDefCollection_003E_002Ebegin(ptr, &iterator);
					System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E.iterator iterator2);
					global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AArtDefCollection_003E_002Eend(ptr, &iterator2);
					if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, &iterator2))
					{
						do
						{
							IArtDefCollection item = new ReadOnlyArtDefCollection(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_002A(&iterator));
							m_artDefCollections.Add(item);
							global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
						}
						while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, &iterator2));
					}
				}
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper2).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper2).Dispose();
		}
		foreach (ReadOnlyArtDefCollection artDefCollection in m_artDefCollections)
		{
			artDefCollection.ResolveReferences();
		}
	}

	public unsafe virtual void Reset()
	{
		m_templateName = string.Empty;
		foreach (ReadOnlyArtDefCollection artDefCollection in m_artDefCollections)
		{
			artDefCollection.Invalidate();
		}
		m_artDefCollections.Clear();
		global::_003CModule_003E.AssetObjects_002EArtDefSet_002EReset(m_artDefSet.ArtDefSet);
		foreach (IArtDef artDef in m_artDefs)
		{
		}
		m_artDefs.Clear();
	}

	public unsafe virtual void SetVersion(int major, int minor, int build, int revision)
	{
		if (!global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetVersion_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXHHHH_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0ED_0040FHGBPDCC_0040Unable_003F5to_003F5set_003F5the_003F5version_003F5of_003F5a_003F5f_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040MIHHDGDL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 198u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetVersion_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXHHHH_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
	}

	public unsafe virtual void SetVersion(string pmVersion)
	{
		if (!global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetVersion_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0ED_0040FHGBPDCC_0040Unable_003F5to_003F5set_003F5the_003F5version_003F5of_003F5a_003F5f_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040MIHHDGDL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 193u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetVersion_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
	}

	public unsafe virtual IArtDefCollection AddCollection(string name)
	{
		if (!global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddCollection_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIArtDefCollection_0040345_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EL_0040CGFAGFOI_0040Unable_003F5to_003F5add_003F5to_003F5root_003F5collection_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040MIHHDGDL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 224u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddCollection_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIArtDefCollection_0040345_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		return null;
	}

	public unsafe virtual void RemoveCollection(string name)
	{
		if (!global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemoveCollection_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FA_0040IPOANDPF_0040Unable_003F5to_003F5remove_003F5from_003F5root_003F5colle_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040MIHHDGDL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 230u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemoveCollection_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
	}

	public unsafe virtual void UpdateRootCollectionsFromTemplate(IArtDefTemplate artDefTmpl)
	{
		if (!global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F2_003F_003FUpdateRootCollectionsFromTemplate_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIArtDefTemplate_0040345_0040_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EL_0040HICJGOPJ_0040Unable_003F5to_003F5update_003F5root_003F5collection_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040MIHHDGDL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 235u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F2_003F_003FUpdateRootCollectionsFromTemplate_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIArtDefTemplate_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
	}

	public unsafe virtual string SerializeIntoXML()
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
				global::_003CModule_003E.AssetObjects_002ESerializer_002ESerializeIntoString_003Cclass_0020AssetObjects_003A_003AArtDefSet_003E(&serializer, &obj, m_artDefSet.ArtDefSet);
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

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool DeserializeFromXML(string xmlText)
	{
		if (!global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F2_003F_003FDeserializeFromXML_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_NPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EB_0040INBOACOL_0040Unable_003F5to_003F5deserialize_003F5into_003F5a_003F5fla_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040MIHHDGDL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 240u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F2_003F_003FDeserializeFromXML_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_NPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		return false;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool SerializeIntoFile(string pmFilename)
	{
		IOStringWrapper iOStringWrapper = null;
		IOStringWrapper iOStringWrapper2 = new IOStringWrapper(pmFilename);
		bool result;
		try
		{
			iOStringWrapper = iOStringWrapper2;
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
				result = global::_003CModule_003E.AssetObjects_002ESerializer_002ESerializeToFile_003Cclass_0020AssetObjects_003A_003AArtDefSet_003E(&serializer, m_artDefSet.ArtDefSet, iOStringWrapper.Value);
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
			((IDisposable)iOStringWrapper).Dispose();
			throw;
		}
		((IDisposable)iOStringWrapper).Dispose();
		return result;
	}

	public unsafe virtual Firaxis.Error.ResultCode DeserializeFromFile(string pmFilename)
	{
		if (!global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F2_003F_003FDeserializeFromFile_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVResultCode_0040Error_00405_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EB_0040INBOACOL_0040Unable_003F5to_003F5deserialize_003F5into_003F5a_003F5fla_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040MIHHDGDL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 246u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F2_003F_003FDeserializeFromFile_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVResultCode_0040Error_00405_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		return Firaxis.Error.ResultCode.Success;
	}

	public unsafe virtual void AddPackageReference(string xlpFile, IXLP pmXLP)
	{
		if (!global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddPackageReference_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040PE_0024AAUIXLP_0040Packages_004045_0040_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EK_0040LGJPNJMN_0040Unable_003F5to_003F5add_003F5package_003F5references_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040MIHHDGDL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 209u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddPackageReference_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040PE_0024AAUIXLP_0040Packages_004045_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
	}

	public unsafe virtual void RemovePackageReference(string xlpFile)
	{
		if (!global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemovePackageReference_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EP_0040ECBKFOKN_0040Unable_003F5to_003F5remove_003F5package_003F5referen_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040MIHHDGDL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 214u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemovePackageReference_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
	}

	public unsafe virtual void ClearPackageReferences()
	{
		if (!global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F2_003F_003FClearPackageReferences_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXXZ_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EM_0040NEKPEFBC_0040Unable_003F5to_003F5clear_003F5package_003F5referenc_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040MIHHDGDL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 219u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x2cdee089_002E_003FbIgnoreAlways_0040_003F2_003F_003FClearPackageReferences_0040FlattenedArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
	}

	public unsafe override string ToString()
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E obj);
		global::_003CModule_003E.AssetObjects_002EArtDefSet_002EToString(m_artDefSet.ArtDefSet, &obj);
		string result;
		try
		{
			result = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(&obj);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E*, void>)(&global::_003CModule_003E.std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002E_007Bdtor_007D), &obj);
			throw;
		}
		global::_003CModule_003E.std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002E_007Bdtor_007D(&obj);
		return result;
	}

	private void Destroy()
	{
		m_templateName = string.Empty;
		foreach (ReadOnlyArtDefCollection artDefCollection in m_artDefCollections)
		{
			artDefCollection.Destroy();
		}
		m_artDefCollections.Clear();
		m_artDefSet.Destroy();
		m_artDefs.Clear();
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			try
			{
				_007EFlattenedArtDef();
				return;
			}
			finally
			{
				((IDisposable)m_artDefSet).Dispose();
			}
		}
		try
		{
			Destroy();
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

	~FlattenedArtDef()
	{
		Dispose(A_0: false);
	}
}
