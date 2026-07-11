using System;
using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class AnimationBinding : IAnimationBinding
{
	private unsafe global::AssetObjects.AnimationBinding* m_pkBinding;

	public unsafe virtual string AnimationName
	{
		get
		{
			IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EAnimationBinding_002EGetAnimationName(m_pkBinding));
			return Marshal.PtrToStringAnsi(ptr);
		}
	}

	public unsafe virtual string SlotName
	{
		get
		{
			IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EAnimationBinding_002EGetName(m_pkBinding));
			return Marshal.PtrToStringAnsi(ptr);
		}
	}

	public unsafe AnimationBinding(global::AssetObjects.AnimationBinding* pkBinding)
	{
		m_pkBinding = pkBinding;
		base._002Ector();
	}

	internal unsafe global::AssetObjects.AnimationBinding* GetAssetObject()
	{
		return m_pkBinding;
	}

	internal unsafe virtual void RemoveReferences()
	{
		//IL_0008: Expected I, but got I8
		m_pkBinding = null;
	}
}
