using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

internal class EnvironmentLightDirectionTag : IEnvironmentLightDirectionTag
{
	private unsafe global::AssetObjects.EnvironmentLightDirectionTag* m_pTag;

	public unsafe virtual bool CastsShadows
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return global::_003CModule_003E.AssetObjects_002EEnvironmentLightDirectionTag_002EGetCastsShadows(m_pTag);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			global::_003CModule_003E.AssetObjects_002EEnvironmentLightDirectionTag_002ESetCastsShadows(m_pTag, value);
		}
	}

	public unsafe virtual IFloatVector3 Intensity
	{
		get
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector);
			global::_003CModule_003E.AssetObjects_002EEnvironmentLightDirectionTag_002EGetIntensity(m_pTag, &fGXVector);
			return new FloatVector3(*(float*)(&fGXVector), System.Runtime.CompilerServices.Unsafe.As<FGXVector3, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fGXVector, 4)), System.Runtime.CompilerServices.Unsafe.As<FGXVector3, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fGXVector, 8)));
		}
	}

	public unsafe virtual float AngularFalloff
	{
		get
		{
			return global::_003CModule_003E.AssetObjects_002EEnvironmentLightDirectionTag_002EGetAngularFalloff(m_pTag);
		}
		set
		{
			global::_003CModule_003E.AssetObjects_002EEnvironmentLightDirectionTag_002ESetAngularFalloff(m_pTag, value);
		}
	}

	public unsafe virtual float Diameter
	{
		get
		{
			return global::_003CModule_003E.AssetObjects_002EEnvironmentLightDirectionTag_002EGetAngularSize(m_pTag);
		}
		set
		{
			global::_003CModule_003E.AssetObjects_002EEnvironmentLightDirectionTag_002ESetAngularSize(m_pTag, value);
		}
	}

	public unsafe virtual float Z => global::_003CModule_003E.AssetObjects_002EEnvironmentLightDirectionTag_002EGetZ(m_pTag);

	public unsafe virtual float Y => global::_003CModule_003E.AssetObjects_002EEnvironmentLightDirectionTag_002EGetY(m_pTag);

	public unsafe virtual float X => global::_003CModule_003E.AssetObjects_002EEnvironmentLightDirectionTag_002EGetX(m_pTag);

	public unsafe virtual string Name
	{
		get
		{
			IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EEnvironmentLightDirectionTag_002EGetName(m_pTag));
			return Marshal.PtrToStringAnsi(ptr);
		}
		set
		{
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(value).ToPointer();
			global::_003CModule_003E.AssetObjects_002EEnvironmentLightDirectionTag_002ESetName(m_pTag, ptr);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe EnvironmentLightDirectionTag(global::AssetObjects.EnvironmentLightDirectionTag* pTag)
	{
		m_pTag = pTag;
		base._002Ector();
	}

	public unsafe virtual void SetDirection(float x, float y, float z)
	{
		global::_003CModule_003E.AssetObjects_002EEnvironmentLightDirectionTag_002ESetDirection(m_pTag, x, y, z);
	}

	public unsafe virtual void SetIntensity(float x, float y, float z)
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector);
		global::_003CModule_003E.FGXVector3_002E_007Bctor_007D(&fGXVector, x, y, z);
		global::_003CModule_003E.AssetObjects_002EEnvironmentLightDirectionTag_002ESetIntensity(m_pTag, fGXVector);
	}

	internal unsafe void SetUnmanaged(global::AssetObjects.EnvironmentLightDirectionTag* pIt)
	{
		m_pTag = pIt;
	}

	internal unsafe global::AssetObjects.EnvironmentLightDirectionTag* GetUnmanaged()
	{
		return m_pTag;
	}
}
