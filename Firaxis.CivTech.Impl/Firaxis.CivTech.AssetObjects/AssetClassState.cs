using System;
using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class AssetClassState : IAssetClassState
{
	private unsafe global::AssetObjects.AssetClassState* m_pkAssetClassState;

	public unsafe virtual string Description
	{
		get
		{
			IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EAssetClassState_002EGetDescription(m_pkAssetClassState));
			return Marshal.PtrToStringAnsi(ptr);
		}
		set
		{
			IntPtr hglobal = Marshal.StringToHGlobalAnsi(value);
			sbyte* ptr = (sbyte*)hglobal.ToPointer();
			global::_003CModule_003E.AssetObjects_002EAssetClassState_002ESetDescription(m_pkAssetClassState, ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe virtual string Name
	{
		get
		{
			IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EAssetClassState_002EGetName(m_pkAssetClassState));
			return Marshal.PtrToStringAnsi(ptr);
		}
		set
		{
			IntPtr hglobal = Marshal.StringToHGlobalAnsi(value);
			sbyte* ptr = (sbyte*)hglobal.ToPointer();
			global::_003CModule_003E.AssetObjects_002EAssetClassState_002ESetName(m_pkAssetClassState, ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe AssetClassState(global::AssetObjects.AssetClassState* pkAssetClassState)
	{
		m_pkAssetClassState = pkAssetClassState;
		base._002Ector();
	}

	public unsafe global::AssetObjects.AssetClassState* GetAssetObject()
	{
		return m_pkAssetClassState;
	}

	internal unsafe void RemoveReferences()
	{
		//IL_0008: Expected I, but got I8
		m_pkAssetClassState = null;
	}

	internal unsafe global::AssetObjects.AssetClassState* GetUnmanaged()
	{
		return m_pkAssetClassState;
	}

	internal unsafe void SetUnmanaged(global::AssetObjects.AssetClassState* p)
	{
		m_pkAssetClassState = p;
	}
}
