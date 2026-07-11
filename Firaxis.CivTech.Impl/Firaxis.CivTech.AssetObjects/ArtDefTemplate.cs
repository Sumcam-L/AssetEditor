using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public class ArtDefTemplate : IArtDefTemplate, IDisposable
{
	private string m_pmTemplateName;

	private string m_pmDescription;

	private IList<IArtDefElementTemplate> m_pmArtDefElementTemplates;

	private unsafe global::AssetObjects.ArtDefTemplate* m_pkArtDefTemplate;

	private unsafe global::AssetObjects.Deserializer* m_pkDeserializer;

	public unsafe virtual string Description
	{
		get
		{
			return m_pmDescription;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			if (!(value == Description))
			{
				m_pmDescription = value;
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002EArtDefTemplate_002ESetDescription(m_pkArtDefTemplate, standardStringWrapper.Value);
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

	public unsafe virtual string Name
	{
		get
		{
			return m_pmTemplateName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			if (!(value == Name))
			{
				m_pmTemplateName = value;
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002EArtDefTemplate_002ESetName(m_pkArtDefTemplate, standardStringWrapper.Value);
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

	public virtual IEnumerable<IArtDefElementTemplate> Collections => m_pmArtDefElementTemplates;

	public unsafe ArtDefTemplate(global::AssetObjects.ArtDefTemplateSet* pkArtDefTmplSet, global::AssetObjects.Deserializer* pkDeserializer)
	{
		m_pmTemplateName = string.Empty;
		m_pmDescription = string.Empty;
		m_pmArtDefElementTemplates = new List<IArtDefElementTemplate>();
		m_pkDeserializer = pkDeserializer;
		base._002Ector();
		m_pkArtDefTemplate = global::_003CModule_003E.AssetObjects_002EArtDefTemplateSet_002EAddTemplate(pkArtDefTmplSet);
	}

	public unsafe ArtDefTemplate(global::AssetObjects.ArtDefTemplate* pkArtDefTmpl, global::AssetObjects.Deserializer* pkDeserializer)
	{
		//IL_0047: Expected I, but got I8
		m_pmArtDefElementTemplates = new List<IArtDefElementTemplate>();
		m_pkArtDefTemplate = pkArtDefTmpl;
		m_pkDeserializer = pkDeserializer;
		base._002Ector();
		if (!global::_003CModule_003E._003FA0xcc8f0c50_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ArtDefTemplate_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040PEAVDeserializer_00402_0040_0040Z_00404_NA && m_pkArtDefTemplate == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BD_0040BCKABEGC_0040m_pkArtDefTemplate_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HB_0040DIDAIPIP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 20u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xcc8f0c50_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ArtDefTemplate_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040PEAVDeserializer_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		AddReferences();
	}

	private void _007EArtDefTemplate()
	{
		RemoveReferences(disposing: true);
	}

	private void _0021ArtDefTemplate()
	{
		RemoveReferences(disposing: true);
	}

	public unsafe virtual IArtDefElementTemplate Add(string name)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(name);
		IArtDefElementTemplate artDefElementTemplate;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			artDefElementTemplate = new ArtDefElementTemplate(m_pkArtDefTemplate, global::_003CModule_003E.AssetObjects_002EArtDefTemplate_002ECreateCollection(m_pkArtDefTemplate, standardStringWrapper.Value), m_pkDeserializer);
			m_pmArtDefElementTemplates.Add(artDefElementTemplate);
			RefreshReferences();
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return artDefElementTemplate;
	}

	public unsafe virtual void Remove(string name)
	{
		//IL_0030: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		IArtDefElementTemplate artDefElementTemplate = FindRoot(name);
		if (!global::_003CModule_003E._003FA0xcc8f0c50_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemove_0040ArtDefTemplate_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && artDefElementTemplate == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0N_0040KLKIECID_0040rootElemTmpl_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HB_0040DIDAIPIP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 136u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xcc8f0c50_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemove_0040ArtDefTemplate_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (m_pmArtDefElementTemplates.Remove(artDefElementTemplate))
		{
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(name);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EArtDefTemplate_002ERemove(m_pkArtDefTemplate, standardStringWrapper.Value);
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper).Dispose();
		}
		RefreshReferences();
	}

	public unsafe global::AssetObjects.ArtDefTemplate* GetNativeTemplate()
	{
		return m_pkArtDefTemplate;
	}

	internal unsafe void AddReferences()
	{
		//IL_0072: Expected I, but got I8
		//IL_002d: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0xcc8f0c50_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddReferences_0040ArtDefTemplate_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXXZ_00404_NA && m_pmArtDefElementTemplates.Count != 0 && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CH_0040PFDJHFFN_0040m_pmArtDefElementTemplates_003F9_003F_0024DOCoun_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HB_0040DIDAIPIP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 93u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xcc8f0c50_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddReferences_0040ArtDefTemplate_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		global::AssetObjects.ArtDefElementTemplate* ptr = global::_003CModule_003E.AssetObjects_002EArtDefTemplate_002Ebegin(m_pkArtDefTemplate);
		if (ptr != global::_003CModule_003E.AssetObjects_002EArtDefTemplate_002Eend(m_pkArtDefTemplate))
		{
			do
			{
				IArtDefElementTemplate item = new ArtDefElementTemplate(m_pkArtDefTemplate, ptr, m_pkDeserializer);
				m_pmArtDefElementTemplates.Add(item);
				ptr = (global::AssetObjects.ArtDefElementTemplate*)((ulong)(nint)ptr + 176uL);
			}
			while (ptr != global::_003CModule_003E.AssetObjects_002EArtDefTemplate_002Eend(m_pkArtDefTemplate));
		}
		m_pmTemplateName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EArtDefTemplate_002EGetName(m_pkArtDefTemplate));
		m_pmDescription = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EArtDefTemplate_002EGetDescription(m_pkArtDefTemplate));
	}

	internal void AddReference(IArtDefElementTemplate artDefElemTmpl)
	{
		m_pmArtDefElementTemplates.Add(artDefElemTmpl);
	}

	internal void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool disposing)
	{
		m_pmTemplateName = string.Empty;
		m_pmDescription = string.Empty;
		m_pmArtDefElementTemplates.Clear();
		if (disposing)
		{
			m_pmArtDefElementTemplates = null;
		}
	}

	internal unsafe void RefreshReferences()
	{
		//IL_0040: Expected I, but got I8
		uint num = 0u;
		global::AssetObjects.ArtDefElementTemplate* ptr = global::_003CModule_003E.AssetObjects_002EArtDefTemplate_002Ebegin(m_pkArtDefTemplate);
		if (ptr != global::_003CModule_003E.AssetObjects_002EArtDefTemplate_002Eend(m_pkArtDefTemplate))
		{
			do
			{
				((ArtDefElementTemplate)m_pmArtDefElementTemplates[(int)num]).SetNativeTemplate(ptr);
				num++;
				ptr = (global::AssetObjects.ArtDefElementTemplate*)((ulong)(nint)ptr + 176uL);
			}
			while (ptr != global::_003CModule_003E.AssetObjects_002EArtDefTemplate_002Eend(m_pkArtDefTemplate));
		}
	}

	private IArtDefElementTemplate FindRoot(string rootName)
	{
		foreach (IArtDefElementTemplate pmArtDefElementTemplate in m_pmArtDefElementTemplates)
		{
			if (pmArtDefElementTemplate.Name == rootName)
			{
				return pmArtDefElementTemplate;
			}
		}
		return null;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EArtDefTemplate();
			return;
		}
		try
		{
			_0021ArtDefTemplate();
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

	~ArtDefTemplate()
	{
		Dispose(A_0: false);
	}
}
