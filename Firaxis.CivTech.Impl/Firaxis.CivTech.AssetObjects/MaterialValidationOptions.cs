using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class MaterialValidationOptions : IMaterialValidationOptions
{
	private unsafe global::AssetObjects.MaterialValidationOptions* m_pkValidationOptions;

	public unsafe virtual bool RequireUniformTextureSize
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return *(bool*)m_pkValidationOptions;
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			*(bool*)m_pkValidationOptions = value;
		}
	}

	public unsafe MaterialValidationOptions(global::AssetObjects.MaterialValidationOptions* pkValidationOptions)
	{
		m_pkValidationOptions = pkValidationOptions;
		base._002Ector();
	}

	internal unsafe void RemoveReferences()
	{
		//IL_0008: Expected I, but got I8
		m_pkValidationOptions = null;
	}
}
