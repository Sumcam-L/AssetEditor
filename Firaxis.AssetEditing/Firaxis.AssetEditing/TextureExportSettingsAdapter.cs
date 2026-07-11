using System;
using System.Collections.Generic;
using DatabaseWrapper;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.TextureExport;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class TextureExportSettingsAdapter : DomNodeAdapter, ISourceObjectAdapter
{
	private List<string> m_availableExportModes = new List<string>();

	public uint ColorKeyX
	{
		get
		{
			return GetAttribute<uint>(EntitySchema.TextureExportSettingsType.ColorKeyXAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.TextureExportSettingsType.ColorKeyXAttribute, value);
		}
	}

	public uint ColorKeyY
	{
		get
		{
			return GetAttribute<uint>(EntitySchema.TextureExportSettingsType.ColorKeyYAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.TextureExportSettingsType.ColorKeyYAttribute, value);
		}
	}

	public uint ColorKeyZ
	{
		get
		{
			return GetAttribute<uint>(EntitySchema.TextureExportSettingsType.ColorKeyZAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.TextureExportSettingsType.ColorKeyZAttribute, value);
		}
	}

	public bool CompleteMipChain
	{
		get
		{
			return GetAttribute<bool>(EntitySchema.TextureExportSettingsType.CompleteMipChainAttribute);
		}
		set
		{
			if (ClassRestrictions != null)
			{
				if (ClassRestrictions.AllowArtistMips)
				{
					SetAttribute(EntitySchema.TextureExportSettingsType.CompleteMipChainAttribute, value);
				}
				else
				{
					SetAttribute(EntitySchema.TextureExportSettingsType.CompleteMipChainAttribute, false);
				}
			}
			else
			{
				SetAttribute(EntitySchema.TextureExportSettingsType.CompleteMipChainAttribute, value);
			}
		}
	}

	public string CurrentExportMode
	{
		get
		{
			return GetAttribute<string>(EntitySchema.TextureExportSettingsType.ExportModeAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.TextureExportSettingsType.ExportModeAttribute, value);
		}
	}

	public IExportSettingsParams ExportSettings { get; private set; }

	public string FilterType
	{
		get
		{
			return GetAttribute<string>(EntitySchema.TextureExportSettingsType.FilterTypeAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.TextureExportSettingsType.FilterTypeAttribute, value);
		}
	}

	public float GammaIn
	{
		get
		{
			return GetAttribute<float>(EntitySchema.TextureExportSettingsType.GammaInAttribute);
		}
		set
		{
			if (ClassRestrictions != null)
			{
				SetAttribute(EntitySchema.TextureExportSettingsType.GammaInAttribute, ClassRestrictions.ExportGammaIn);
			}
			else
			{
				SetAttribute(EntitySchema.TextureExportSettingsType.GammaInAttribute, value);
			}
		}
	}

	public float GammaOut
	{
		get
		{
			return GetAttribute<float>(EntitySchema.TextureExportSettingsType.GammaOutAttribute);
		}
		set
		{
			if (ClassRestrictions != null)
			{
				SetAttribute(EntitySchema.TextureExportSettingsType.GammaOutAttribute, ClassRestrictions.ExportGammaOut);
			}
			else
			{
				SetAttribute(EntitySchema.TextureExportSettingsType.GammaOutAttribute, value);
			}
		}
	}

	public int NumManualMips
	{
		get
		{
			return GetAttribute<int>(EntitySchema.TextureExportSettingsType.NumManualMipsAttribute);
		}
		set
		{
			if (ClassRestrictions != null)
			{
				if (ClassRestrictions.AllowArtistMips)
				{
					SetAttribute(EntitySchema.TextureExportSettingsType.NumManualMipsAttribute, value);
				}
				else
				{
					SetAttribute(EntitySchema.TextureExportSettingsType.NumManualMipsAttribute, 0);
				}
			}
			else
			{
				SetAttribute(EntitySchema.TextureExportSettingsType.NumManualMipsAttribute, value);
			}
		}
	}

	public string PixelFormat
	{
		get
		{
			return GetAttribute<string>(EntitySchema.TextureExportSettingsType.PixelFormatAttribute);
		}
		set
		{
			if (ClassRestrictions != null)
			{
				SetAttribute(EntitySchema.TextureExportSettingsType.PixelFormatAttribute, ClassRestrictions.ExportPixelFormat.ToString());
			}
			else
			{
				SetAttribute(EntitySchema.TextureExportSettingsType.PixelFormatAttribute, value);
			}
		}
	}

	public bool SampleFromTopLayer
	{
		get
		{
			return GetAttribute<bool>(EntitySchema.TextureExportSettingsType.SampleFromTopLevelAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.TextureExportSettingsType.SampleFromTopLevelAttribute, value);
		}
	}

	public uint SlabHeight
	{
		get
		{
			return GetAttribute<uint>(EntitySchema.TextureExportSettingsType.SlabHeightAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.TextureExportSettingsType.SlabHeightAttribute, value);
		}
	}

	public uint SlabWidth
	{
		get
		{
			return GetAttribute<uint>(EntitySchema.TextureExportSettingsType.SlabWidthAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.TextureExportSettingsType.SlabWidthAttribute, value);
		}
	}

	public float SupportScale
	{
		get
		{
			return GetAttribute<float>(EntitySchema.TextureExportSettingsType.SupportScaleAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.TextureExportSettingsType.SupportScaleAttribute, value);
		}
	}

	public bool UseMips
	{
		get
		{
			return GetAttribute<bool>(EntitySchema.TextureExportSettingsType.UseMipsAttribute);
		}
		set
		{
			if (ClassRestrictions != null)
			{
				if (ClassRestrictions.AllowArtistMips)
				{
					SetAttribute(EntitySchema.TextureExportSettingsType.UseMipsAttribute, value);
				}
				else
				{
					SetAttribute(EntitySchema.TextureExportSettingsType.UseMipsAttribute, false);
				}
			}
			else
			{
				SetAttribute(EntitySchema.TextureExportSettingsType.UseMipsAttribute, value);
			}
		}
	}

	public float ValueClampMax
	{
		get
		{
			return GetAttribute<float>(EntitySchema.TextureExportSettingsType.ValueClampMaxAttribute);
		}
		set
		{
			if (ClassRestrictions != null)
			{
				SetAttribute(EntitySchema.TextureExportSettingsType.ValueClampMaxAttribute, ClassRestrictions.ExportClampMax);
			}
			else
			{
				SetAttribute(EntitySchema.TextureExportSettingsType.ValueClampMaxAttribute, value);
			}
		}
	}

	public float ValueClampMin
	{
		get
		{
			return GetAttribute<float>(EntitySchema.TextureExportSettingsType.ValueClampMinAttribute);
		}
		set
		{
			if (ClassRestrictions != null)
			{
				SetAttribute(EntitySchema.TextureExportSettingsType.ValueClampMinAttribute, ClassRestrictions.ExportClampMin);
			}
			else
			{
				SetAttribute(EntitySchema.TextureExportSettingsType.ValueClampMinAttribute, value);
			}
		}
	}

	private ITextureExportOptions ClassRestrictions { get; set; }

	private ITextureInstance Texture { get; set; }

	public event EventHandler ExportSettingsChanged;

	public string[] GetCurrentSourceObjects()
	{
		return m_availableExportModes.ToArray();
	}

	public void Update(IExportSettingsParams exportSettings, ITextureInstance texture)
	{
		UnregisterFromDomChanges();
		ExportSettings = exportSettings;
		Texture = texture;
		if (texture != null && !string.IsNullOrEmpty(texture.ClassName))
		{
			ITextureClass textureClass = global::DatabaseWrapper.DatabaseWrapper.GetTextureClass(CivTechService.AnyProject, texture.ClassName);
			if (textureClass != null)
			{
				ClassRestrictions = textureClass.ExportOptions;
				BuildAvailableExportModes(ClassRestrictions);
			}
		}
		AssignProperties(exportSettings);
		RegisterForDomChanges();
		if (m_availableExportModes.Count > 0 && !m_availableExportModes.Contains(CurrentExportMode))
		{
			CurrentExportMode = m_availableExportModes[0];
		}
	}

	public void Update(ITextureExportOptions classRestrictions)
	{
		UnregisterFromDomChanges();
		ClassRestrictions = classRestrictions;
		if (!classRestrictions.AllowArtistMips)
		{
			NumManualMips = 0;
			UseMips = false;
			CompleteMipChain = false;
		}
		BuildAvailableExportModes(classRestrictions);
		FilterType = classRestrictions.DefaultMipFilter.ToString();
		PixelFormat = classRestrictions.ExportPixelFormat.ToString();
		CurrentExportMode = m_availableExportModes[0];
		ValueClampMax = classRestrictions.ExportClampMax;
		ValueClampMin = classRestrictions.ExportClampMin;
		GammaIn = ClassRestrictions.ExportGammaIn;
		GammaOut = ClassRestrictions.ExportGammaOut;
		SupportScale = classRestrictions.DefaultMipSupportScale;
		ExportSettings.FilterType = classRestrictions.DefaultMipFilter;
		ExportSettings.PixelFormat = classRestrictions.ExportPixelFormat;
		ExportSettings.ExportMode = (ExportMode)Enum.Parse(typeof(ExportMode), CurrentExportMode, ignoreCase: true);
		ExportSettings.ValueClampMax = ValueClampMax;
		ExportSettings.ValueClampMin = ValueClampMin;
		ExportSettings.GammaIn = GammaIn;
		ExportSettings.GammaOut = GammaOut;
		ExportSettings.SupportScale = SupportScale;
		ExportSettings.UseMips = UseMips;
		ExportSettings.CompleteMipChain = CompleteMipChain;
		ExportSettings.NumManualMips = (uint)NumManualMips;
		OnExportSettingsChanged();
		RegisterForDomChanges();
	}

	protected virtual void HandleDomNodeAttributeChanged(object sender, AttributeEventArgs e)
	{
		EditingContext editingContext = base.DomNode.Parent.As<EditingContext>();
		Enum.TryParse<ExportMode>(CurrentExportMode, out var result);
		IDocument document = base.DomNode.GetRoot().As<IDocument>();
		if (document != null && document.IsReadOnly && editingContext != null && editingContext.InTransaction)
		{
			MessageBoxes.Show("Can not modify assets that are not part of the active project", "File not changed", MessageBoxButton.OK, MessageBoxImage.Error);
			throw new InvalidTransactionException("Can not modify assets that are not part of the active project");
		}
		if (e.AttributeInfo == EntitySchema.TextureExportSettingsType.ColorKeyXAttribute)
		{
			if (result == ExportMode.ColorKey && ExportSettings.ColorKeyX != ColorKeyX)
			{
				ExportSettings.ColorKeyX = ColorKeyX;
				OnExportSettingsChanged();
			}
			else
			{
				Outputs.WriteLine(OutputMessageType.Warning, "Cannot change the Color Key field unless the Export mode is set to Color Key.");
				ColorKeyX = ExportSettings.ColorKeyX;
			}
		}
		else if (e.AttributeInfo == EntitySchema.TextureExportSettingsType.ColorKeyYAttribute)
		{
			if (result == ExportMode.ColorKey && ExportSettings.ColorKeyY != ColorKeyY)
			{
				ExportSettings.ColorKeyY = ColorKeyY;
				OnExportSettingsChanged();
			}
			else
			{
				Outputs.WriteLine(OutputMessageType.Warning, "Cannot change the Color Key field unless the Export mode is set to Color Key.");
				ColorKeyY = ExportSettings.ColorKeyY;
			}
		}
		else if (e.AttributeInfo == EntitySchema.TextureExportSettingsType.ColorKeyZAttribute)
		{
			if (result == ExportMode.ColorKey && ExportSettings.ColorKeyZ != ColorKeyZ)
			{
				ExportSettings.ColorKeyZ = ColorKeyZ;
				OnExportSettingsChanged();
			}
			else
			{
				Outputs.WriteLine(OutputMessageType.Warning, "Cannot change the Color Key field unless the Export mode is set to Color Key.");
				ColorKeyZ = ExportSettings.ColorKeyZ;
			}
		}
		else if (e.AttributeInfo == EntitySchema.TextureExportSettingsType.CompleteMipChainAttribute)
		{
			if (result == ExportMode.ManualMipMaps && ExportSettings.CompleteMipChain != CompleteMipChain)
			{
				ExportSettings.CompleteMipChain = CompleteMipChain;
				OnExportSettingsChanged();
			}
			else
			{
				Outputs.WriteLine(OutputMessageType.Warning, "Cannot change the Complete Mip Change field unless the Export mode is set to Manual Mips Mode.");
				CompleteMipChain = ExportSettings.CompleteMipChain;
			}
		}
		else if (e.AttributeInfo == EntitySchema.TextureExportSettingsType.ExportModeAttribute)
		{
			ExportSettings.ExportMode = (ExportMode)Enum.Parse(typeof(ExportMode), CurrentExportMode, ignoreCase: true);
		}
		else if (e.AttributeInfo == EntitySchema.TextureExportSettingsType.FilterTypeAttribute)
		{
			FilterType filterType = (FilterType)Enum.Parse(typeof(FilterType), FilterType, ignoreCase: true);
			if (ExportSettings.FilterType != filterType)
			{
				ExportSettings.FilterType = filterType;
				OnExportSettingsChanged();
			}
		}
		else if (e.AttributeInfo == EntitySchema.TextureExportSettingsType.GammaInAttribute)
		{
			if (ClassRestrictions != null)
			{
				if (GammaIn == ClassRestrictions.ExportGammaIn && ExportSettings.GammaIn != GammaIn)
				{
					ExportSettings.GammaIn = GammaIn;
					OnExportSettingsChanged();
				}
				else
				{
					Outputs.WriteLine(OutputMessageType.Warning, "Gamma settings much match class settings.");
					GammaIn = ClassRestrictions.ExportGammaIn;
				}
			}
			else
			{
				ExportSettings.GammaIn = GammaIn;
				OnExportSettingsChanged();
			}
		}
		else if (e.AttributeInfo == EntitySchema.TextureExportSettingsType.GammaOutAttribute)
		{
			if (ClassRestrictions != null)
			{
				if (GammaOut == ClassRestrictions.ExportGammaOut && GammaOut != ExportSettings.GammaOut)
				{
					ExportSettings.GammaOut = GammaOut;
					OnExportSettingsChanged();
				}
				else
				{
					Outputs.WriteLine(OutputMessageType.Warning, "Gamma settings much match class settings.");
					GammaOut = ClassRestrictions.ExportGammaOut;
				}
			}
			else
			{
				ExportSettings.GammaOut = GammaOut;
				OnExportSettingsChanged();
			}
		}
		else if (e.AttributeInfo == EntitySchema.TextureExportSettingsType.NumManualMipsAttribute)
		{
			if (result == ExportMode.ManualMipMaps && ExportSettings.NumManualMips != NumManualMips)
			{
				ExportSettings.NumManualMips = (uint)NumManualMips;
				OnExportSettingsChanged();
			}
			else
			{
				Outputs.WriteLine(OutputMessageType.Warning, "Cannot change the Num Manual Mips field unless the Export mode is set to Manual Mips Mode.");
				NumManualMips = (int)ExportSettings.NumManualMips;
			}
		}
		else if (e.AttributeInfo == EntitySchema.TextureExportSettingsType.SlabHeightAttribute)
		{
			if (result == ExportMode.Texture3D && ExportSettings.SlabHeight != SlabHeight)
			{
				ExportSettings.SlabHeight = SlabHeight;
				OnExportSettingsChanged();
			}
			else
			{
				Outputs.WriteLine(OutputMessageType.Warning, "Cannot change 3D Slab settings unless the Export Mode is set to Texture3D");
				SlabHeight = ExportSettings.SlabHeight;
			}
		}
		else if (e.AttributeInfo == EntitySchema.TextureExportSettingsType.SlabWidthAttribute)
		{
			if (result == ExportMode.Texture3D && ExportSettings.SlabWidth != SlabWidth)
			{
				ExportSettings.SlabWidth = SlabWidth;
				OnExportSettingsChanged();
			}
			else
			{
				Outputs.WriteLine(OutputMessageType.Warning, "Cannot change 3D Slab settings unless the Export Mode is set to Texture3D");
				SlabWidth = ExportSettings.SlabWidth;
			}
		}
		else if (e.AttributeInfo == EntitySchema.TextureExportSettingsType.SupportScaleAttribute)
		{
			if (ExportSettings.SupportScale != SupportScale)
			{
				ExportSettings.SupportScale = SupportScale;
				OnExportSettingsChanged();
			}
		}
		else if (e.AttributeInfo == EntitySchema.TextureExportSettingsType.SampleFromTopLevelAttribute)
		{
			if (ExportSettings.SampleFromTopLayer != SampleFromTopLayer)
			{
				ExportSettings.SampleFromTopLayer = SampleFromTopLayer;
				OnExportSettingsChanged();
			}
		}
		else if (e.AttributeInfo == EntitySchema.TextureExportSettingsType.UseMipsAttribute)
		{
			if (ClassRestrictions != null)
			{
				if (ClassRestrictions.AllowArtistMips && ExportSettings.UseMips != UseMips)
				{
					ExportSettings.UseMips = UseMips;
					OnExportSettingsChanged();
					return;
				}
				UnregisterFromDomChanges();
				Outputs.WriteLine(OutputMessageType.Info, "Current texture class does not allow artist mip maps.");
				UseMips = false;
				ExportSettings.UseMips = false;
				RegisterForDomChanges();
			}
			else
			{
				ExportSettings.UseMips = UseMips;
				OnExportSettingsChanged();
			}
		}
		else if (e.AttributeInfo == EntitySchema.TextureExportSettingsType.ValueClampMaxAttribute)
		{
			if (ClassRestrictions != null)
			{
				if (ClassRestrictions.ExportClampMax == ValueClampMax && ExportSettings.ValueClampMax != ValueClampMax)
				{
					ExportSettings.ValueClampMax = ValueClampMax;
					OnExportSettingsChanged();
				}
				else
				{
					Outputs.WriteLine(OutputMessageType.Warning, "Current texture class does not allow clamp maximum to be modified.");
					ValueClampMax = ClassRestrictions.ExportClampMax;
				}
			}
			else
			{
				ExportSettings.ValueClampMax = ValueClampMax;
				OnExportSettingsChanged();
			}
		}
		else
		{
			if (e.AttributeInfo != EntitySchema.TextureExportSettingsType.ValueClampMinAttribute)
			{
				return;
			}
			if (ClassRestrictions != null)
			{
				if (ClassRestrictions.ExportClampMin == ValueClampMin)
				{
					ExportSettings.ValueClampMin = ValueClampMin;
					return;
				}
				Outputs.WriteLine(OutputMessageType.Warning, "Current texture class does not allow clamp minimum to be modified.");
				ValueClampMin = ClassRestrictions.ExportClampMin;
			}
			else
			{
				ExportSettings.ValueClampMin = ValueClampMin;
			}
		}
	}

	protected virtual void OnExportSettingsChanged()
	{
		this.ExportSettingsChanged?.Invoke(this, EventArgs.Empty);
	}

	private void AssignProperties(IExportSettingsParams exportSettings)
	{
		if (exportSettings.UseMips != UseMips)
		{
			UseMips = exportSettings.UseMips;
		}
		if (exportSettings.NumManualMips != NumManualMips)
		{
			NumManualMips = (int)exportSettings.NumManualMips;
		}
		if (exportSettings.ColorKeyX != ColorKeyX)
		{
			ColorKeyX = exportSettings.ColorKeyX;
		}
		if (exportSettings.ColorKeyY != ColorKeyY)
		{
			ColorKeyY = exportSettings.ColorKeyY;
		}
		if (exportSettings.ColorKeyZ != ColorKeyZ)
		{
			ColorKeyZ = exportSettings.ColorKeyZ;
		}
		if (exportSettings.CompleteMipChain != CompleteMipChain)
		{
			CompleteMipChain = exportSettings.CompleteMipChain;
		}
		if (exportSettings.ExportMode.ToString() != CurrentExportMode)
		{
			CurrentExportMode = exportSettings.ExportMode.ToString();
		}
		if (exportSettings.FilterType.ToString() != FilterType)
		{
			FilterType = exportSettings.FilterType.ToString();
		}
		if (exportSettings.GammaIn != GammaIn)
		{
			GammaIn = exportSettings.GammaIn;
		}
		if (exportSettings.GammaOut != GammaOut)
		{
			GammaOut = exportSettings.GammaOut;
		}
		if (exportSettings.PixelFormat.ToString() != PixelFormat)
		{
			PixelFormat = exportSettings.PixelFormat.ToString();
		}
		if (exportSettings.SlabHeight != SlabHeight)
		{
			SlabHeight = exportSettings.SlabHeight;
		}
		if (exportSettings.SlabWidth != SlabWidth)
		{
			SlabWidth = exportSettings.SlabWidth;
		}
		if (exportSettings.SupportScale != SupportScale)
		{
			SupportScale = exportSettings.SupportScale;
		}
		if (exportSettings.SampleFromTopLayer != SampleFromTopLayer)
		{
			SampleFromTopLayer = exportSettings.SampleFromTopLayer;
		}
		if (exportSettings.ValueClampMax != ValueClampMax)
		{
			ValueClampMax = exportSettings.ValueClampMax;
		}
		if (exportSettings.ValueClampMin != ValueClampMin)
		{
			ValueClampMin = exportSettings.ValueClampMin;
		}
	}

	private void BuildAvailableExportModes(ITextureExportOptions classRestrictions)
	{
		List<string> list = new List<string>();
		switch (classRestrictions.ExportTextureType)
		{
		case TextureType.TEX_2D:
			list.Add("Texture2D");
			if (ClassRestrictions.AllowArtistMips)
			{
				list.Add("ManualMipMaps");
			}
			break;
		case TextureType.TEX_CUBE:
			list.Add("CubeMap");
			break;
		case TextureType.TEX_3D:
			list.Add("Texture3D");
			break;
		case TextureType.TEX_3D_COLORKEY:
			list.Add("ColorKey");
			break;
		default:
			list.AddRange(Enum.GetNames(typeof(ExportMode)));
			break;
		}
		m_availableExportModes = list;
	}

	private void RegisterForDomChanges()
	{
		base.DomNode.AttributeChanged += HandleDomNodeAttributeChanged;
	}

	private void UnregisterFromDomChanges()
	{
		base.DomNode.AttributeChanged -= HandleDomNodeAttributeChanged;
	}
}
