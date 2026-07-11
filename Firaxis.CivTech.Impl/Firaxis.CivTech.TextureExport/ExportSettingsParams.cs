using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Firaxis.CivTech.AssetObjects;
using std;
using TextureExport;

namespace Firaxis.CivTech.TextureExport;

public class ExportSettingsParams : IExportSettingsParams
{
	protected unsafe global::TextureExport.ExportSettingsParams* m_pkEntity;

	public unsafe virtual bool SampleFromTopLayer
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return *(bool*)((ulong)(nint)m_pkEntity + 64uL);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			*(bool*)((ulong)(nint)m_pkEntity + 64uL) = value;
		}
	}

	public unsafe virtual uint ColorKeyZ
	{
		get
		{
			return *(uint*)((ulong)(nint)m_pkEntity + 56uL);
		}
		set
		{
			*(uint*)((ulong)(nint)m_pkEntity + 56uL) = value;
		}
	}

	public unsafe virtual uint ColorKeyY
	{
		get
		{
			return *(uint*)((ulong)(nint)m_pkEntity + 52uL);
		}
		set
		{
			*(uint*)((ulong)(nint)m_pkEntity + 52uL) = value;
		}
	}

	public unsafe virtual uint ColorKeyX
	{
		get
		{
			return *(uint*)((ulong)(nint)m_pkEntity + 48uL);
		}
		set
		{
			*(uint*)((ulong)(nint)m_pkEntity + 48uL) = value;
		}
	}

	public unsafe virtual uint SlabHeight
	{
		get
		{
			return *(uint*)((ulong)(nint)m_pkEntity + 44uL);
		}
		set
		{
			*(uint*)((ulong)(nint)m_pkEntity + 44uL) = value;
		}
	}

	public unsafe virtual uint SlabWidth
	{
		get
		{
			return *(uint*)((ulong)(nint)m_pkEntity + 40uL);
		}
		set
		{
			*(uint*)((ulong)(nint)m_pkEntity + 40uL) = value;
		}
	}

	public unsafe virtual float GammaOut
	{
		get
		{
			return *(float*)((ulong)(nint)m_pkEntity + 36uL);
		}
		set
		{
			*(float*)((ulong)(nint)m_pkEntity + 36uL) = value;
		}
	}

	public unsafe virtual float GammaIn
	{
		get
		{
			return *(float*)((ulong)(nint)m_pkEntity + 32uL);
		}
		set
		{
			*(float*)((ulong)(nint)m_pkEntity + 32uL) = value;
		}
	}

	public unsafe virtual float SupportScale
	{
		get
		{
			return *(float*)((ulong)(nint)m_pkEntity + 28uL);
		}
		set
		{
			*(float*)((ulong)(nint)m_pkEntity + 28uL) = value;
		}
	}

	public unsafe virtual float ValueClampMax
	{
		get
		{
			return *(float*)((ulong)(nint)m_pkEntity + 24uL);
		}
		set
		{
			*(float*)((ulong)(nint)m_pkEntity + 24uL) = value;
		}
	}

	public unsafe virtual float ValueClampMin
	{
		get
		{
			return *(float*)((ulong)(nint)m_pkEntity + 20uL);
		}
		set
		{
			*(float*)((ulong)(nint)m_pkEntity + 20uL) = value;
		}
	}

	public unsafe virtual bool CompleteMipChain
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return *(bool*)((ulong)(nint)m_pkEntity + 16uL);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			*(bool*)((ulong)(nint)m_pkEntity + 16uL) = value;
		}
	}

	public unsafe virtual uint NumManualMips
	{
		get
		{
			return *(uint*)((ulong)(nint)m_pkEntity + 12uL);
		}
		set
		{
			*(uint*)((ulong)(nint)m_pkEntity + 12uL) = value;
		}
	}

	public unsafe virtual bool UseMips
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return *(bool*)((ulong)(nint)m_pkEntity + 8uL);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			*(bool*)((ulong)(nint)m_pkEntity + 8uL) = value;
		}
	}

	public unsafe virtual ExportMode ExportMode
	{
		get
		{
			ExportMode result = ExportMode.Texture2D;
			switch (*(int*)((ulong)(nint)m_pkEntity + 60uL))
			{
			case 4:
				result = ExportMode.ColorKey;
				break;
			case 3:
				result = ExportMode.Texture3D;
				break;
			case 2:
				result = ExportMode.CubeMap;
				break;
			case 1:
				result = ExportMode.ManualMipMaps;
				break;
			case 0:
				result = ExportMode.Texture2D;
				break;
			}
			return result;
		}
		set
		{
			global::TextureExport.ExportMode exportMode = (global::TextureExport.ExportMode)0;
			switch (value)
			{
			case ExportMode.ColorKey:
				exportMode = (global::TextureExport.ExportMode)4;
				break;
			case ExportMode.CubeMap:
				exportMode = (global::TextureExport.ExportMode)2;
				break;
			case ExportMode.ManualMipMaps:
				exportMode = (global::TextureExport.ExportMode)1;
				break;
			case ExportMode.Texture3D:
				exportMode = (global::TextureExport.ExportMode)3;
				break;
			case ExportMode.Texture2D:
				exportMode = (global::TextureExport.ExportMode)0;
				break;
			}
			*(global::TextureExport.ExportMode*)((ulong)(nint)m_pkEntity + 60uL) = exportMode;
		}
	}

	public unsafe virtual FilterType FilterType
	{
		get
		{
			return ConvertFilterTypes(*(Filter_Type*)((ulong)(nint)m_pkEntity + 4uL));
		}
		set
		{
			*(Filter_Type*)((ulong)(nint)m_pkEntity + 4uL) = ConvertFilterTypes(value);
		}
	}

	public unsafe virtual PixelFormat PixelFormat
	{
		get
		{
			return ConvertPixelFormat(*(global::TextureExport.PixelFormat*)m_pkEntity);
		}
		set
		{
			*(global::TextureExport.PixelFormat*)m_pkEntity = ConvertPixelFormat(value);
		}
	}

	public unsafe ExportSettingsParams(global::TextureExport.ExportSettingsParams* @params)
	{
		m_pkEntity = @params;
	}

	private void _007EExportSettingsParams()
	{
		RemoveReferences();
	}

	private void _0021ExportSettingsParams()
	{
		RemoveReferences();
	}

	public virtual void AssignFromTextureClass(ITextureClass texClass)
	{
		ITextureExportOptions exportOptions = texClass.ExportOptions;
		PixelFormat = exportOptions.ExportPixelFormat;
		GammaIn = exportOptions.ExportGammaIn;
		GammaOut = exportOptions.ExportGammaOut;
		ValueClampMax = exportOptions.ExportClampMax;
		ValueClampMin = exportOptions.ExportClampMin;
		UseMips = exportOptions.AllowArtistMips;
	}

	public unsafe virtual void SerializeToFile(string filePath)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(filePath).ToPointer();
		global::_003CModule_003E.TextureExport_002EExportSettingsParams_002ESerializeToFile(m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual string SerializeToXMLString()
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E obj);
		global::_003CModule_003E.TextureExport_002EExportSettingsParams_002ESerializeToXMLString(m_pkEntity, &obj);
		string result;
		try
		{
			result = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002Ec_str(&obj));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E*, void>)(&global::_003CModule_003E.std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002E_007Bdtor_007D), &obj);
			throw;
		}
		global::_003CModule_003E.std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002E_007Bdtor_007D(&obj);
		return result;
	}

	public unsafe virtual void DeserializeFromFile(string filePath)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(filePath).ToPointer();
		global::_003CModule_003E.TextureExport_002EExportSettingsParams_002EDeserializeFromFile(m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void DeserializeFromXML(string xml)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(xml).ToPointer();
		global::_003CModule_003E.TextureExport_002EExportSettingsParams_002EDeserializeFromXMLString(m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public static ExportMode ConvertExportModes(global::TextureExport.ExportMode mode)
	{
		ExportMode result = ExportMode.Texture2D;
		switch (mode)
		{
		case (global::TextureExport.ExportMode)4:
			result = ExportMode.ColorKey;
			break;
		case (global::TextureExport.ExportMode)3:
			result = ExportMode.Texture3D;
			break;
		case (global::TextureExport.ExportMode)2:
			result = ExportMode.CubeMap;
			break;
		case (global::TextureExport.ExportMode)1:
			result = ExportMode.ManualMipMaps;
			break;
		case (global::TextureExport.ExportMode)0:
			result = ExportMode.Texture2D;
			break;
		}
		return result;
	}

	public static global::TextureExport.ExportMode ConvertExportModes(ExportMode mode)
	{
		global::TextureExport.ExportMode result = (global::TextureExport.ExportMode)0;
		switch (mode)
		{
		case ExportMode.ColorKey:
			result = (global::TextureExport.ExportMode)4;
			break;
		case ExportMode.CubeMap:
			result = (global::TextureExport.ExportMode)2;
			break;
		case ExportMode.ManualMipMaps:
			result = (global::TextureExport.ExportMode)1;
			break;
		case ExportMode.Texture3D:
			result = (global::TextureExport.ExportMode)3;
			break;
		case ExportMode.Texture2D:
			result = (global::TextureExport.ExportMode)0;
			break;
		}
		return result;
	}

	public static FilterType ConvertFilterTypes(Filter_Type type)
	{
		FilterType result = FilterType.Lanczos12;
		switch (type)
		{
		case (Filter_Type)0:
			result = FilterType.Box;
			break;
		case (Filter_Type)1:
			result = FilterType.Tent;
			break;
		case (Filter_Type)2:
			result = FilterType.Bell;
			break;
		case (Filter_Type)3:
			result = FilterType.B_Spline;
			break;
		case (Filter_Type)4:
			result = FilterType.Mitchell;
			break;
		case (Filter_Type)5:
			result = FilterType.Lanczos3;
			break;
		case (Filter_Type)6:
			result = FilterType.Blackman;
			break;
		case (Filter_Type)7:
			result = FilterType.Lanczos4;
			break;
		case (Filter_Type)8:
			result = FilterType.Lanczos6;
			break;
		case (Filter_Type)9:
			result = FilterType.Lanczos12;
			break;
		case (Filter_Type)10:
			result = FilterType.Kaiser;
			break;
		case (Filter_Type)11:
			result = FilterType.Gaussian;
			break;
		case (Filter_Type)12:
			result = FilterType.CatmullRom;
			break;
		case (Filter_Type)13:
			result = FilterType.QuadraticInterpolation;
			break;
		case (Filter_Type)14:
			result = FilterType.QuadraticApprox;
			break;
		case (Filter_Type)15:
			result = FilterType.QuadraticMix;
			break;
		case (Filter_Type)16:
			result = FilterType.MaxFilters;
			break;
		}
		return result;
	}

	public static Filter_Type ConvertFilterTypes(FilterType type)
	{
		Filter_Type result = (Filter_Type)9;
		switch (type)
		{
		case FilterType.Box:
			result = (Filter_Type)0;
			break;
		case FilterType.Tent:
			result = (Filter_Type)1;
			break;
		case FilterType.Bell:
			result = (Filter_Type)2;
			break;
		case FilterType.B_Spline:
			result = (Filter_Type)3;
			break;
		case FilterType.Mitchell:
			result = (Filter_Type)4;
			break;
		case FilterType.Lanczos3:
			result = (Filter_Type)5;
			break;
		case FilterType.Blackman:
			result = (Filter_Type)6;
			break;
		case FilterType.Lanczos4:
			result = (Filter_Type)7;
			break;
		case FilterType.Lanczos6:
			result = (Filter_Type)8;
			break;
		case FilterType.Lanczos12:
			result = (Filter_Type)9;
			break;
		case FilterType.Kaiser:
			result = (Filter_Type)10;
			break;
		case FilterType.Gaussian:
			result = (Filter_Type)11;
			break;
		case FilterType.CatmullRom:
			result = (Filter_Type)12;
			break;
		case FilterType.QuadraticInterpolation:
			result = (Filter_Type)13;
			break;
		case FilterType.QuadraticApprox:
			result = (Filter_Type)14;
			break;
		case FilterType.QuadraticMix:
			result = (Filter_Type)15;
			break;
		case FilterType.MaxFilters:
			result = (Filter_Type)16;
			break;
		}
		return result;
	}

	public static PixelFormat ConvertPixelFormat(global::TextureExport.PixelFormat format)
	{
		PixelFormat result = PixelFormat.R8G8B8A8_UNORM;
		switch (format)
		{
		case (global::TextureExport.PixelFormat)0:
			result = PixelFormat.Invalid;
			break;
		case (global::TextureExport.PixelFormat)28:
			result = PixelFormat.R8G8B8A8_UNORM;
			break;
		case (global::TextureExport.PixelFormat)49:
			result = PixelFormat.R8G8_UNORM;
			break;
		case (global::TextureExport.PixelFormat)61:
			result = PixelFormat.R8_UNORM;
			break;
		case (global::TextureExport.PixelFormat)65:
			result = PixelFormat.A8_UNORM;
			break;
		case (global::TextureExport.PixelFormat)11:
			result = PixelFormat.R16G16B16A16_UNORM;
			break;
		case (global::TextureExport.PixelFormat)35:
			result = PixelFormat.R16G16_UNORM;
			break;
		case (global::TextureExport.PixelFormat)56:
			result = PixelFormat.R16_UNORM;
			break;
		case (global::TextureExport.PixelFormat)2:
			result = PixelFormat.R32G32B32A32_FLOAT;
			break;
		case (global::TextureExport.PixelFormat)6:
			result = PixelFormat.R32G32B32_FLOAT;
			break;
		case (global::TextureExport.PixelFormat)16:
			result = PixelFormat.R32G32_FLOAT;
			break;
		case (global::TextureExport.PixelFormat)41:
			result = PixelFormat.R32_FLOAT;
			break;
		case (global::TextureExport.PixelFormat)10:
			result = PixelFormat.R16G16B16A16_FLOAT;
			break;
		case (global::TextureExport.PixelFormat)34:
			result = PixelFormat.R16G16_FLOAT;
			break;
		case (global::TextureExport.PixelFormat)54:
			result = PixelFormat.R16_FLOAT;
			break;
		case (global::TextureExport.PixelFormat)24:
			result = PixelFormat.R10G10B10A2_UNORM;
			break;
		case (global::TextureExport.PixelFormat)1001:
			result = PixelFormat.A8R8G8B8_DX9;
			break;
		case (global::TextureExport.PixelFormat)1010:
			result = PixelFormat.L16_DX9;
			break;
		case (global::TextureExport.PixelFormat)1009:
			result = PixelFormat.L8_DX9;
			break;
		case (global::TextureExport.PixelFormat)1007:
			result = PixelFormat.A8L8_DX9;
			break;
		}
		return result;
	}

	public static global::TextureExport.PixelFormat ConvertPixelFormat(PixelFormat format)
	{
		global::TextureExport.PixelFormat result = (global::TextureExport.PixelFormat)28;
		switch (format)
		{
		case PixelFormat.Invalid:
			result = (global::TextureExport.PixelFormat)0;
			break;
		case PixelFormat.R8G8B8A8_UNORM:
			result = (global::TextureExport.PixelFormat)28;
			break;
		case PixelFormat.R8G8_UNORM:
			result = (global::TextureExport.PixelFormat)49;
			break;
		case PixelFormat.R8_UNORM:
			result = (global::TextureExport.PixelFormat)61;
			break;
		case PixelFormat.A8_UNORM:
			result = (global::TextureExport.PixelFormat)65;
			break;
		case PixelFormat.R16G16B16A16_UNORM:
			result = (global::TextureExport.PixelFormat)11;
			break;
		case PixelFormat.R16G16_UNORM:
			result = (global::TextureExport.PixelFormat)35;
			break;
		case PixelFormat.R16_UNORM:
			result = (global::TextureExport.PixelFormat)56;
			break;
		case PixelFormat.R16G16B16A16_FLOAT:
			result = (global::TextureExport.PixelFormat)10;
			break;
		case PixelFormat.R16G16_FLOAT:
			result = (global::TextureExport.PixelFormat)34;
			break;
		case PixelFormat.R16_FLOAT:
			result = (global::TextureExport.PixelFormat)54;
			break;
		case PixelFormat.R10G10B10A2_UNORM:
			result = (global::TextureExport.PixelFormat)24;
			break;
		case PixelFormat.R32G32B32A32_FLOAT:
			result = (global::TextureExport.PixelFormat)2;
			break;
		case PixelFormat.R32G32B32_FLOAT:
			result = (global::TextureExport.PixelFormat)6;
			break;
		case PixelFormat.R32G32_FLOAT:
			result = (global::TextureExport.PixelFormat)16;
			break;
		case PixelFormat.R32_FLOAT:
			result = (global::TextureExport.PixelFormat)41;
			break;
		case PixelFormat.A8R8G8B8_DX9:
			result = (global::TextureExport.PixelFormat)1001;
			break;
		case PixelFormat.A8L8_DX9:
			result = (global::TextureExport.PixelFormat)1007;
			break;
		case PixelFormat.L8_DX9:
			result = (global::TextureExport.PixelFormat)1009;
			break;
		case PixelFormat.L16_DX9:
			result = (global::TextureExport.PixelFormat)1010;
			break;
		}
		return result;
	}

	internal unsafe global::TextureExport.ExportSettingsParams* GetExportSettings()
	{
		return m_pkEntity;
	}

	internal unsafe virtual void RemoveReferences()
	{
		//IL_0008: Expected I, but got I8
		m_pkEntity = null;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EExportSettingsParams();
			return;
		}
		try
		{
			_0021ExportSettingsParams();
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

	~ExportSettingsParams()
	{
		Dispose(A_0: false);
	}
}
