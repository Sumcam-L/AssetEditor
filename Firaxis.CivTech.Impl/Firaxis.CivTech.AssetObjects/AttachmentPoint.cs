using System;
using System.Runtime.CompilerServices;
using AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public class AttachmentPoint : IAttachmentPoint
{
	private string m_name;

	private string m_boneName;

	private string m_modelInstanceName;

	private unsafe global::AssetObjects.AttachmentPoint* m_pkAttachmentPoint;

	private unsafe global::AssetObjects.Deserializer* m_pkDeserializer;

	private IValueSet m_pmValueSet;

	public unsafe virtual float Scale
	{
		get
		{
			return *global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002EGetScale(m_pkAttachmentPoint);
		}
		set
		{
			global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002ESetScale(m_pkAttachmentPoint, &value);
		}
	}

	public unsafe virtual IFloatVector3 Orientation
	{
		get
		{
			FGXVector3* ptr = global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002EGetOrientation(m_pkAttachmentPoint);
			return new FloatVector3(*(float*)ptr, *(float*)((ulong)(nint)ptr + 4uL), *(float*)((ulong)(nint)ptr + 8uL));
		}
		set
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector);
			global::_003CModule_003E.FGXVector3_002E_007Bctor_007D(&fGXVector, value.X, value.Y, value.Z);
			global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002ESetOrientation(m_pkAttachmentPoint, &fGXVector);
		}
	}

	public unsafe virtual IFloatVector3 Position
	{
		get
		{
			FGXVector3* ptr = global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002EGetPosition(m_pkAttachmentPoint);
			return new FloatVector3(*(float*)ptr, *(float*)((ulong)(nint)ptr + 4uL), *(float*)((ulong)(nint)ptr + 8uL));
		}
		set
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector);
			global::_003CModule_003E.FGXVector3_002E_007Bctor_007D(&fGXVector, value.X, value.Y, value.Z);
			global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002ESetPosition(m_pkAttachmentPoint, &fGXVector);
		}
	}

	public virtual IValueSet CookParameters => m_pmValueSet;

	public unsafe virtual string ModelInstanceName
	{
		get
		{
			return m_modelInstanceName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_modelInstanceName = value;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002ESetModelInstanceName(m_pkAttachmentPoint, standardStringWrapper.Value);
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

	public unsafe virtual string BoneName
	{
		get
		{
			return m_boneName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_boneName = value;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002ESetBoneName(m_pkAttachmentPoint, standardStringWrapper.Value);
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

	public unsafe virtual string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_name = value;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002ESetName(m_pkAttachmentPoint, standardStringWrapper.Value);
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

	public unsafe AttachmentPoint(global::AssetObjects.AttachmentPoint* pkAttachmentPoint, global::AssetObjects.Deserializer* pkDeserializer)
	{
		//IL_0049: Expected I, but got I8
		m_pkAttachmentPoint = pkAttachmentPoint;
		m_pkDeserializer = pkDeserializer;
		m_pmValueSet = new ValueSet(global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002EGetCookParameters(pkAttachmentPoint), pkDeserializer);
		base._002Ector();
		if (!global::_003CModule_003E._003FA0x80f2f0d0_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0AttachmentPoint_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040PEAVDeserializer_00402_0040_0040Z_00404_NA && pkAttachmentPoint == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BC_0040LJNLMOOK_0040pkAttachmentPoint_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HG_0040HNMCAFIC_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 22u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x80f2f0d0_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0AttachmentPoint_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040PEAVDeserializer_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		m_name = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002EGetName(pkAttachmentPoint));
		m_boneName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002EGetBoneName(pkAttachmentPoint));
		m_modelInstanceName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002EGetModelInstanceName(pkAttachmentPoint));
	}

	internal unsafe void RemoveReferences()
	{
		//IL_0008: Expected I, but got I8
		m_pkAttachmentPoint = null;
	}

	internal unsafe global::AssetObjects.AttachmentPoint* GetNativeObject()
	{
		return m_pkAttachmentPoint;
	}

	internal unsafe void UpdateNativeObject(global::AssetObjects.AttachmentPoint* pkAttachmentPoint)
	{
		m_pkAttachmentPoint = pkAttachmentPoint;
	}
}
