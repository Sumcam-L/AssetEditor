using System;
using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class ParticleBinding : IParticleBinding
{
	private unsafe global::AssetObjects.ParticleBinding* m_pkBinding;

	public unsafe virtual string ParticleName
	{
		get
		{
			IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EParticleBinding_002EGetParticleName(m_pkBinding));
			return Marshal.PtrToStringAnsi(ptr);
		}
	}

	public unsafe virtual string SlotName
	{
		get
		{
			IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EParticleBinding_002EGetName(m_pkBinding));
			return Marshal.PtrToStringAnsi(ptr);
		}
	}

	public unsafe ParticleBinding(global::AssetObjects.ParticleBinding* pkBinding)
	{
		m_pkBinding = pkBinding;
		base._002Ector();
	}

	internal unsafe global::AssetObjects.ParticleBinding* GetAssetObject()
	{
		return m_pkBinding;
	}

	internal unsafe virtual void RemoveReferences()
	{
		//IL_0008: Expected I, but got I8
		m_pkBinding = null;
	}
}
