using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public class ArtDefElementTemplate : IArtDefElementTemplate, IDisposable
{
	private IList<IArtDefElementTemplate> m_pmChildren;

	private IParameterSet m_pmParameters;

	private unsafe global::AssetObjects.ArtDefTemplate* m_pkArtDefTemplate;

	private unsafe global::AssetObjects.ArtDefElementTemplate* m_pkArtDefElementTemplate;

	private unsafe global::AssetObjects.Deserializer* m_pkDeserializer;

	public virtual IEnumerable<IArtDefElementTemplate> Children => m_pmChildren;

	public unsafe virtual string Description
	{
		get
		{
			return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EArtDefElementTemplate_002EGetDescription(m_pkArtDefElementTemplate));
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			if (!(value == Description))
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002EArtDefElementTemplate_002ESetDescription(m_pkArtDefElementTemplate, standardStringWrapper.Value);
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

	public unsafe virtual bool ReplaceMergedCollectionElements
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return global::_003CModule_003E.AssetObjects_002EArtDefElementTemplate_002EGetReplaceMergedCollectionElements(m_pkArtDefElementTemplate);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			global::_003CModule_003E.AssetObjects_002EArtDefElementTemplate_002ESetReplaceMergedCollectionElements(m_pkArtDefElementTemplate, value);
		}
	}

	public unsafe virtual bool AppendMergedParameterCollections
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return global::_003CModule_003E.AssetObjects_002EArtDefElementTemplate_002EGetAppendMergedParameterCollections(m_pkArtDefElementTemplate);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			global::_003CModule_003E.AssetObjects_002EArtDefElementTemplate_002ESetAppendMergedParameterCollections(m_pkArtDefElementTemplate, value);
		}
	}

	public virtual IParameterSet Parameters => m_pmParameters;

	public unsafe virtual bool HasCustomEditor
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return global::_003CModule_003E.AssetObjects_002EArtDefElementTemplate_002EHasCustomEditor(m_pkArtDefElementTemplate);
		}
	}

	public unsafe virtual string CustomEditor
	{
		get
		{
			return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EArtDefElementTemplate_002EGetCustomEditor(m_pkArtDefElementTemplate));
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			if (!(CustomEditor == value))
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002EArtDefElementTemplate_002ESetCustomEditor(m_pkArtDefElementTemplate, standardStringWrapper.Value);
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
			return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EArtDefElementTemplate_002EGetName(m_pkArtDefElementTemplate));
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			if (!(value == Name))
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002EArtDefElementTemplate_002ESetName(m_pkArtDefElementTemplate, standardStringWrapper.Value);
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

	public unsafe ArtDefElementTemplate(global::AssetObjects.ArtDefTemplate* pkArtDefTmpl, global::AssetObjects.ArtDefElementTemplate* pkArtDefElemTmpl, global::AssetObjects.Deserializer* pkDeserializer)
	{
		m_pkArtDefTemplate = pkArtDefTmpl;
		m_pkArtDefElementTemplate = pkArtDefElemTmpl;
		m_pkDeserializer = pkDeserializer;
		base._002Ector();
		m_pmParameters = new ParameterSet(global::_003CModule_003E.AssetObjects_002EArtDefElementTemplate_002EGetParameters(m_pkArtDefElementTemplate));
		m_pmChildren = new List<IArtDefElementTemplate>();
		AddReferences();
	}

	private void _007EArtDefElementTemplate()
	{
		RemoveReferences(disposing: true);
	}

	private void _0021ArtDefElementTemplate()
	{
		RemoveReferences(disposing: true);
	}

	public unsafe virtual IArtDefElementTemplate AddChild(string pmName)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(pmName);
		IArtDefElementTemplate artDefElementTemplate;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::AssetObjects.ArtDefElementTemplate* pkArtDefElemTmpl = global::_003CModule_003E.AssetObjects_002EArtDefElementTemplate_002EAddChild(m_pkArtDefElementTemplate, standardStringWrapper.Value);
			artDefElementTemplate = new ArtDefElementTemplate(m_pkArtDefTemplate, pkArtDefElemTmpl, m_pkDeserializer);
			m_pmChildren.Add(artDefElementTemplate);
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

	public unsafe virtual void RemoveChild(string pmName)
	{
		//IL_0030: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		IArtDefElementTemplate artDefElementTemplate = FindChild(pmName);
		if (!global::_003CModule_003E._003FA0x6029dfbf_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemoveChild_0040ArtDefElementTemplate_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && artDefElementTemplate == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BA_0040KMBEAGJK_0040pmChildToRemove_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HI_0040DGMGKPKN_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 155u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x6029dfbf_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemoveChild_0040ArtDefElementTemplate_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		m_pmChildren.Remove(artDefElementTemplate);
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(pmName);
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::_003CModule_003E.AssetObjects_002EArtDefElementTemplate_002ERemoveChild(m_pkArtDefElementTemplate, standardStringWrapper.Value);
			RefreshReferences();
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
	}

	public unsafe global::AssetObjects.ArtDefTemplate* GetNativeOwner()
	{
		return m_pkArtDefTemplate;
	}

	public unsafe global::AssetObjects.ArtDefElementTemplate* GetNativeTemplate()
	{
		return m_pkArtDefElementTemplate;
	}

	internal unsafe void SetNativeTemplate(global::AssetObjects.ArtDefElementTemplate* pNative)
	{
		m_pkArtDefElementTemplate = pNative;
		RefreshReferences();
		((ParameterSet)m_pmParameters).SetNativeParameterSet(global::_003CModule_003E.AssetObjects_002EArtDefElementTemplate_002EGetParameters(m_pkArtDefElementTemplate));
	}

	internal void AddReference(IArtDefElementTemplate artDefElemTmpl)
	{
		m_pmChildren.Add(artDefElemTmpl);
	}

	internal unsafe void AddReferences()
	{
		//IL_0042: Expected I, but got I8
		global::AssetObjects.ArtDefElementTemplate* ptr = global::_003CModule_003E.AssetObjects_002EArtDefElementTemplate_002Echild_begin(m_pkArtDefElementTemplate);
		if (ptr != global::_003CModule_003E.AssetObjects_002EArtDefElementTemplate_002Echild_end(m_pkArtDefElementTemplate))
		{
			do
			{
				IArtDefElementTemplate item = new ArtDefElementTemplate(m_pkArtDefTemplate, ptr, m_pkDeserializer);
				m_pmChildren.Add(item);
				ptr = (global::AssetObjects.ArtDefElementTemplate*)((ulong)(nint)ptr + 176uL);
			}
			while (ptr != global::_003CModule_003E.AssetObjects_002EArtDefElementTemplate_002Echild_end(m_pkArtDefElementTemplate));
		}
	}

	internal void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool disposing)
	{
		m_pmChildren.Clear();
		if (disposing)
		{
			m_pmChildren = null;
		}
	}

	internal unsafe void RefreshReferences()
	{
		//IL_0040: Expected I, but got I8
		uint num = 0u;
		global::AssetObjects.ArtDefElementTemplate* ptr = global::_003CModule_003E.AssetObjects_002EArtDefElementTemplate_002Echild_begin(m_pkArtDefElementTemplate);
		if (ptr != global::_003CModule_003E.AssetObjects_002EArtDefElementTemplate_002Echild_end(m_pkArtDefElementTemplate))
		{
			do
			{
				((ArtDefElementTemplate)m_pmChildren[(int)num]).SetNativeTemplate(ptr);
				num++;
				ptr = (global::AssetObjects.ArtDefElementTemplate*)((ulong)(nint)ptr + 176uL);
			}
			while (ptr != global::_003CModule_003E.AssetObjects_002EArtDefElementTemplate_002Echild_end(m_pkArtDefElementTemplate));
		}
	}

	private IArtDefElementTemplate FindChild(string pmName)
	{
		foreach (IArtDefElementTemplate pmChild in m_pmChildren)
		{
			if (pmChild.Name == pmName)
			{
				return pmChild;
			}
		}
		return null;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EArtDefElementTemplate();
			return;
		}
		try
		{
			_0021ArtDefElementTemplate();
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

	~ArtDefElementTemplate()
	{
		Dispose(A_0: false);
	}
}
