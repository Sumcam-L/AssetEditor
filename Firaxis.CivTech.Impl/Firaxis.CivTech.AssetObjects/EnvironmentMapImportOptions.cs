using System;
using System.Runtime.InteropServices;
using AssetObjects;
using Firaxis.CivTech.TextureExport;
using TextureExport;

namespace Firaxis.CivTech.AssetObjects;

public class EnvironmentMapImportOptions : IEnvironmentMapImportOptions
{
	private unsafe global::AssetObjects.EnvironmentMapImportOptions* m_pOptions;

	public unsafe virtual bool RequirePow2
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return *(bool*)((ulong)(nint)m_pOptions + 5uL);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			*(bool*)((ulong)(nint)m_pOptions + 5uL) = value;
		}
	}

	public unsafe virtual bool AllowArtistMips
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return *(bool*)((ulong)(nint)m_pOptions + 4uL);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			*(bool*)((ulong)(nint)m_pOptions + 4uL) = value;
		}
	}

	public unsafe virtual uint MinWidth
	{
		get
		{
			return *(uint*)((ulong)(nint)m_pOptions + 12uL);
		}
		set
		{
			*(uint*)((ulong)(nint)m_pOptions + 12uL) = value;
		}
	}

	public unsafe virtual uint MaxWidth
	{
		get
		{
			return *(uint*)((ulong)(nint)m_pOptions + 8uL);
		}
		set
		{
			*(uint*)((ulong)(nint)m_pOptions + 8uL) = value;
		}
	}

	public unsafe virtual Firaxis.CivTech.TextureExport.PixelFormat PixelFormat
	{
		get
		{
			return Firaxis.CivTech.TextureExport.ExportSettingsParams.ConvertPixelFormat(*(global::TextureExport.PixelFormat*)m_pOptions);
		}
		set
		{
			*(global::TextureExport.PixelFormat*)m_pOptions = Firaxis.CivTech.TextureExport.ExportSettingsParams.ConvertPixelFormat(value);
		}
	}

	public unsafe EnvironmentMapImportOptions(global::AssetObjects.EnvironmentMapImportOptions* pOptions)
	{
		m_pOptions = pOptions;
		base._002Ector();
	}

	public static Firaxis.CivTech.TextureExport.EnvironmentMapParameterization ConvertParameterization(global::TextureExport.EnvironmentMapParameterization e)
	{
		return e switch
		{
			(global::TextureExport.EnvironmentMapParameterization)1 => Firaxis.CivTech.TextureExport.EnvironmentMapParameterization.ENVMAP_CUBE, 
			(global::TextureExport.EnvironmentMapParameterization)0 => Firaxis.CivTech.TextureExport.EnvironmentMapParameterization.ENVMAP_LATLONG, 
			_ => throw new Exception("Missing Enum!  Fix me!"), 
		};
	}

	public static global::TextureExport.EnvironmentMapParameterization ConvertParameterization(Firaxis.CivTech.TextureExport.EnvironmentMapParameterization e)
	{
		return e switch
		{
			Firaxis.CivTech.TextureExport.EnvironmentMapParameterization.ENVMAP_CUBE => (global::TextureExport.EnvironmentMapParameterization)1, 
			Firaxis.CivTech.TextureExport.EnvironmentMapParameterization.ENVMAP_LATLONG => (global::TextureExport.EnvironmentMapParameterization)0, 
			_ => throw new Exception("Missing Enum!  Fix me!"), 
		};
	}

	internal unsafe void RemoveReferences()
	{
		//IL_0008: Expected I, but got I8
		m_pOptions = null;
	}
}
