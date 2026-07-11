using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Firaxis.CivTech.TextureExport;
using Platform;
using TextureExport;

namespace Firaxis.CivTech.AssetObjects;

public class TextureExportOptions : ITextureExportOptions
{
	private unsafe global::AssetObjects.TextureExportOptions* m_pkExportOptions;

	public unsafe virtual bool RequirePow2
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return *(bool*)((ulong)(nint)m_pkExportOptions + 54uL);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			*(bool*)((ulong)(nint)m_pkExportOptions + 54uL) = value;
		}
	}

	public unsafe virtual bool RequireSquare
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return *(bool*)((ulong)(nint)m_pkExportOptions + 53uL);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			*(bool*)((ulong)(nint)m_pkExportOptions + 53uL) = value;
		}
	}

	public unsafe virtual bool AllowArtistMips
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return *(bool*)((ulong)(nint)m_pkExportOptions + 52uL);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			*(bool*)((ulong)(nint)m_pkExportOptions + 52uL) = value;
		}
	}

	public unsafe virtual float ExportClampMax
	{
		get
		{
			return *(float*)((ulong)(nint)m_pkExportOptions + 48uL);
		}
		set
		{
			*(float*)((ulong)(nint)m_pkExportOptions + 48uL) = value;
		}
	}

	public unsafe virtual float ExportClampMin
	{
		get
		{
			return *(float*)((ulong)(nint)m_pkExportOptions + 44uL);
		}
		set
		{
			*(float*)((ulong)(nint)m_pkExportOptions + 44uL) = value;
		}
	}

	public unsafe virtual float ExportGammaOut
	{
		get
		{
			return *(float*)((ulong)(nint)m_pkExportOptions + 40uL);
		}
		set
		{
			*(float*)((ulong)(nint)m_pkExportOptions + 40uL) = value;
		}
	}

	public unsafe virtual float ExportGammaIn
	{
		get
		{
			return *(float*)((ulong)(nint)m_pkExportOptions + 36uL);
		}
		set
		{
			*(float*)((ulong)(nint)m_pkExportOptions + 36uL) = value;
		}
	}

	public unsafe virtual uint MinDepth
	{
		get
		{
			return *(uint*)((ulong)(nint)m_pkExportOptions + 32uL);
		}
		set
		{
			*(uint*)((ulong)(nint)m_pkExportOptions + 32uL) = value;
		}
	}

	public unsafe virtual uint MaxDepth
	{
		get
		{
			return *(uint*)((ulong)(nint)m_pkExportOptions + 28uL);
		}
		set
		{
			*(uint*)((ulong)(nint)m_pkExportOptions + 28uL) = value;
		}
	}

	public unsafe virtual uint MinHeight
	{
		get
		{
			return *(uint*)((ulong)(nint)m_pkExportOptions + 24uL);
		}
		set
		{
			*(uint*)((ulong)(nint)m_pkExportOptions + 24uL) = value;
		}
	}

	public unsafe virtual uint MaxHeight
	{
		get
		{
			return *(uint*)((ulong)(nint)m_pkExportOptions + 20uL);
		}
		set
		{
			*(uint*)((ulong)(nint)m_pkExportOptions + 20uL) = value;
		}
	}

	public unsafe virtual uint MinWidth
	{
		get
		{
			return *(uint*)((ulong)(nint)m_pkExportOptions + 16uL);
		}
		set
		{
			*(uint*)((ulong)(nint)m_pkExportOptions + 16uL) = value;
		}
	}

	public unsafe virtual uint MaxWidth
	{
		get
		{
			return *(uint*)((ulong)(nint)m_pkExportOptions + 12uL);
		}
		set
		{
			*(uint*)((ulong)(nint)m_pkExportOptions + 12uL) = value;
		}
	}

	public unsafe virtual float DefaultMipSupportScale
	{
		get
		{
			return *(float*)((ulong)(nint)m_pkExportOptions + 8uL);
		}
		set
		{
			*(float*)((ulong)(nint)m_pkExportOptions + 8uL) = value;
		}
	}

	public unsafe virtual TextureType ExportTextureType
	{
		get
		{
			return ConvertTextureTypes(*(global::AssetObjects.TextureType*)((ulong)(nint)m_pkExportOptions + 56uL));
		}
		set
		{
			*(global::AssetObjects.TextureType*)((ulong)(nint)m_pkExportOptions + 56uL) = ConvertTextureTypes(value);
		}
	}

	public unsafe virtual FilterType DefaultMipFilter
	{
		get
		{
			return Firaxis.CivTech.TextureExport.ExportSettingsParams.ConvertFilterTypes(*(Filter_Type*)((ulong)(nint)m_pkExportOptions + 4uL));
		}
		set
		{
			*(Filter_Type*)((ulong)(nint)m_pkExportOptions + 4uL) = Firaxis.CivTech.TextureExport.ExportSettingsParams.ConvertFilterTypes(value);
		}
	}

	public unsafe virtual Firaxis.CivTech.TextureExport.PixelFormat ExportPixelFormat
	{
		get
		{
			return Firaxis.CivTech.TextureExport.ExportSettingsParams.ConvertPixelFormat(*(global::TextureExport.PixelFormat*)m_pkExportOptions);
		}
		set
		{
			*(global::TextureExport.PixelFormat*)m_pkExportOptions = Firaxis.CivTech.TextureExport.ExportSettingsParams.ConvertPixelFormat(value);
		}
	}

	public unsafe TextureExportOptions(global::AssetObjects.TextureExportOptions* pkExportOptions)
	{
		m_pkExportOptions = pkExportOptions;
		base._002Ector();
	}

	public unsafe static global::AssetObjects.TextureType ConvertTextureTypes(TextureType type)
	{
		//IL_002f: Expected I, but got I8
		switch (type)
		{
		default:
			if (!global::_003CModule_003E._003FA0x263188c3_002E_003FbIgnoreAlways_0040_003F4_003F_003FConvertTextureTypes_0040TextureExportOptions_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040SM_003FAW4TextureType_00403_0040W46345_0040_0040Z_00404_NA && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HN_0040NOLHPHEH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 40u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x263188c3_002E_003FbIgnoreAlways_0040_003F4_003F_003FConvertTextureTypes_0040TextureExportOptions_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040SM_003FAW4TextureType_00403_0040W46345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			return (global::AssetObjects.TextureType)0;
		case TextureType.TEX_3D_COLORKEY:
			return (global::AssetObjects.TextureType)3;
		case TextureType.TEX_3D:
			return (global::AssetObjects.TextureType)2;
		case TextureType.TEX_CUBE:
			return (global::AssetObjects.TextureType)1;
		case TextureType.TEX_2D:
			return (global::AssetObjects.TextureType)0;
		}
	}

	public unsafe static TextureType ConvertTextureTypes(global::AssetObjects.TextureType type)
	{
		//IL_002f: Expected I, but got I8
		switch (type)
		{
		default:
			if (!global::_003CModule_003E._003FA0x263188c3_002E_003FbIgnoreAlways_0040_003F4_003F_003FConvertTextureTypes_0040TextureExportOptions_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040SM_003FAW4TextureType_0040345_0040W463_0040_0040Z_00404_NA && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HN_0040NOLHPHEH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 22u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x263188c3_002E_003FbIgnoreAlways_0040_003F4_003F_003FConvertTextureTypes_0040TextureExportOptions_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040SM_003FAW4TextureType_0040345_0040W463_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			return TextureType.TEX_2D;
		case (global::AssetObjects.TextureType)3:
			return TextureType.TEX_3D_COLORKEY;
		case (global::AssetObjects.TextureType)2:
			return TextureType.TEX_3D;
		case (global::AssetObjects.TextureType)1:
			return TextureType.TEX_CUBE;
		case (global::AssetObjects.TextureType)0:
			return TextureType.TEX_2D;
		}
	}

	internal unsafe void RemoveReferences()
	{
		//IL_0008: Expected I, but got I8
		m_pkExportOptions = null;
	}
}
