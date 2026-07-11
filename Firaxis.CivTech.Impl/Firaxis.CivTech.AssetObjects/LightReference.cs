using System;
using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

internal class LightReference : ILightReference
{
	private unsafe global::AssetObjects.LightReference* m_pUnmanaged;

	public unsafe virtual string LightName
	{
		get
		{
			IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002ELightReference_002EGetName(m_pUnmanaged));
			return Marshal.PtrToStringAnsi(ptr);
		}
	}

	public unsafe LightReference(global::AssetObjects.LightReference* pUnmanaged)
	{
		m_pUnmanaged = pUnmanaged;
		base._002Ector();
	}

	internal unsafe global::AssetObjects.LightReference* GetUnmanaged()
	{
		return m_pUnmanaged;
	}

	internal unsafe void SetUnmanaged(global::AssetObjects.LightReference* pRef)
	{
		m_pUnmanaged = pRef;
	}

	internal unsafe virtual void RemoveReferences()
	{
		//IL_0008: Expected I, but got I8
		m_pUnmanaged = null;
	}
}
