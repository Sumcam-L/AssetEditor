using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Firaxis.CivTech.TextureExport;
using TextureExport;

namespace Firaxis.CivTech.AssetObjects;

public class TextureInstance : ImportedEntity, ITextureInstance
{
	private IExportSettingsParams m_ExportSettings;

	public unsafe virtual uint NumMipMaps
	{
		get
		{
			return global::_003CModule_003E.AssetObjects_002ETextureInstance_002EGetNumMipMaps((global::AssetObjects.TextureInstance*)m_pkEntity);
		}
		set
		{
			global::_003CModule_003E.AssetObjects_002ETextureInstance_002ESetNumMipMaps((global::AssetObjects.TextureInstance*)m_pkEntity, value);
		}
	}

	public unsafe virtual uint Depth
	{
		get
		{
			return global::_003CModule_003E.AssetObjects_002ETextureInstance_002EGetDepth((global::AssetObjects.TextureInstance*)m_pkEntity);
		}
		set
		{
			global::_003CModule_003E.AssetObjects_002ETextureInstance_002ESetDepth((global::AssetObjects.TextureInstance*)m_pkEntity, value);
		}
	}

	public unsafe virtual uint Width
	{
		get
		{
			return global::_003CModule_003E.AssetObjects_002ETextureInstance_002EGetWidth((global::AssetObjects.TextureInstance*)m_pkEntity);
		}
		set
		{
			global::_003CModule_003E.AssetObjects_002ETextureInstance_002ESetWidth((global::AssetObjects.TextureInstance*)m_pkEntity, value);
		}
	}

	public unsafe virtual uint Height
	{
		get
		{
			return global::_003CModule_003E.AssetObjects_002ETextureInstance_002EGetHeight((global::AssetObjects.TextureInstance*)m_pkEntity);
		}
		set
		{
			global::_003CModule_003E.AssetObjects_002ETextureInstance_002ESetHeight((global::AssetObjects.TextureInstance*)m_pkEntity, value);
		}
	}

	public unsafe virtual IExportSettingsParams ExportSettings
	{
		get
		{
			return m_ExportSettings;
		}
		set
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out global::TextureExport.ExportSettingsParams exportSettingsParams);
			global::_003CModule_003E.TextureExport_002EExportSettingsParams_002E_007Bctor_007D(&exportSettingsParams);
			System.Runtime.CompilerServices.Unsafe.As<global::TextureExport.ExportSettingsParams, bool>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref exportSettingsParams, 16)) = value.CompleteMipChain;
			System.Runtime.CompilerServices.Unsafe.As<global::TextureExport.ExportSettingsParams, bool>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref exportSettingsParams, 8)) = value.UseMips;
			System.Runtime.CompilerServices.Unsafe.As<global::TextureExport.ExportSettingsParams, global::TextureExport.ExportMode>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref exportSettingsParams, 60)) = Firaxis.CivTech.TextureExport.ExportSettingsParams.ConvertExportModes(value.ExportMode);
			System.Runtime.CompilerServices.Unsafe.As<global::TextureExport.ExportSettingsParams, Filter_Type>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref exportSettingsParams, 4)) = Firaxis.CivTech.TextureExport.ExportSettingsParams.ConvertFilterTypes(value.FilterType);
			*(global::TextureExport.PixelFormat*)(&exportSettingsParams) = Firaxis.CivTech.TextureExport.ExportSettingsParams.ConvertPixelFormat(value.PixelFormat);
			System.Runtime.CompilerServices.Unsafe.As<global::TextureExport.ExportSettingsParams, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref exportSettingsParams, 32)) = value.GammaIn;
			System.Runtime.CompilerServices.Unsafe.As<global::TextureExport.ExportSettingsParams, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref exportSettingsParams, 36)) = value.GammaOut;
			System.Runtime.CompilerServices.Unsafe.As<global::TextureExport.ExportSettingsParams, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref exportSettingsParams, 28)) = value.SupportScale;
			System.Runtime.CompilerServices.Unsafe.As<global::TextureExport.ExportSettingsParams, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref exportSettingsParams, 24)) = value.ValueClampMax;
			System.Runtime.CompilerServices.Unsafe.As<global::TextureExport.ExportSettingsParams, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref exportSettingsParams, 20)) = value.ValueClampMin;
			System.Runtime.CompilerServices.Unsafe.As<global::TextureExport.ExportSettingsParams, uint>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref exportSettingsParams, 48)) = value.ColorKeyX;
			System.Runtime.CompilerServices.Unsafe.As<global::TextureExport.ExportSettingsParams, uint>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref exportSettingsParams, 52)) = value.ColorKeyY;
			System.Runtime.CompilerServices.Unsafe.As<global::TextureExport.ExportSettingsParams, uint>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref exportSettingsParams, 56)) = value.ColorKeyZ;
			System.Runtime.CompilerServices.Unsafe.As<global::TextureExport.ExportSettingsParams, uint>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref exportSettingsParams, 12)) = value.NumManualMips;
			System.Runtime.CompilerServices.Unsafe.As<global::TextureExport.ExportSettingsParams, uint>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref exportSettingsParams, 44)) = value.SlabHeight;
			System.Runtime.CompilerServices.Unsafe.As<global::TextureExport.ExportSettingsParams, uint>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref exportSettingsParams, 40)) = value.SlabWidth;
			global::_003CModule_003E.AssetObjects_002ETextureInstance_002ESetExportSettings((global::AssetObjects.TextureInstance*)m_pkEntity, &exportSettingsParams);
		}
	}

	public unsafe TextureInstance(global::AssetObjects.TextureInstance* pkInstance, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
		: base((global::AssetObjects.InstanceEntity*)pkInstance, pkDeserializer, pkSerializer, pkVirtualPantry)
	{
		m_ExportSettings = new Firaxis.CivTech.TextureExport.ExportSettingsParams(global::_003CModule_003E.AssetObjects_002ETextureInstance_002EGetExportSettings((global::AssetObjects.TextureInstance*)m_pkEntity));
	}

	public unsafe TextureInstance(global::AssetObjects.InstanceSet* pkInstanceSet, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
		: base((global::AssetObjects.InstanceEntity*)global::_003CModule_003E.AssetObjects_002EInstanceSet_002EPush_003Cclass_0020AssetObjects_003A_003ATextureInstance_003E(pkInstanceSet), pkDeserializer, pkSerializer, pkVirtualPantry)
	{
		m_ExportSettings = new Firaxis.CivTech.TextureExport.ExportSettingsParams(global::_003CModule_003E.AssetObjects_002ETextureInstance_002EGetExportSettings((global::AssetObjects.TextureInstance*)m_pkEntity));
	}

	public override void PublishStats(IDictionary<string, int> stats)
	{
		base.PublishStats(stats);
		int height = (int)Height;
		int width = (int)Width;
		stats["Height"] = height;
		stats["Width"] = width;
		stats["Depth"] = (int)Depth;
		stats["Mip Count"] = (int)NumMipMaps;
		if (height > 0 && ((~height + 1) & height) == height && width > 0 && ((~width + 1) & width) == width && !Tags.Contains("Power of 2"))
		{
			AddTag("Power of 2");
		}
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public virtual bool IsGradientMapTexture()
	{
		return false;
	}

	public virtual string GetExportSettingsString()
	{
		return ExportSettings.SerializeToXMLString();
	}

	internal unsafe override void AddReferences()
	{
		m_ExportSettings = new Firaxis.CivTech.TextureExport.ExportSettingsParams(global::_003CModule_003E.AssetObjects_002ETextureInstance_002EGetExportSettings((global::AssetObjects.TextureInstance*)m_pkEntity));
		base.AddReferences();
	}

	internal override void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool bDisposing)
	{
		m_ExportSettings = null;
		base.RemoveReferences(bDisposing);
	}
}
