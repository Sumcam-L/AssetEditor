using System;
using DatabaseWrapper;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class TextureEntityAdapter : ImportedEntityAdapter
{
	public uint Depth
	{
		get
		{
			return GetAttribute<uint>(EntitySchema.TextureEntityType.DepthAttribute);
		}
		private set
		{
			SetAttribute(EntitySchema.TextureEntityType.DepthAttribute, value);
		}
	}

	public override IInstanceEntity InstanceEntity
	{
		get
		{
			return base.InstanceEntity;
		}
		internal set
		{
			if (base.InstanceEntity != value)
			{
				base.InstanceEntity = value;
				if (ExportSettings.ExportSettings == null)
				{
					ExportSettings.Update(Texture.ExportSettings, Texture);
				}
			}
		}
	}

	public TextureExportSettingsAdapter ExportSettings
	{
		get
		{
			return GetChild<TextureExportSettingsAdapter>(EntitySchema.TextureEntityType.ExportSettingsChild);
		}
		private set
		{
			SetChild(EntitySchema.TextureEntityType.ExportSettingsChild, value);
		}
	}

	public uint Height
	{
		get
		{
			return GetAttribute<uint>(EntitySchema.TextureEntityType.HeightAttribute);
		}
		private set
		{
			SetAttribute(EntitySchema.TextureEntityType.HeightAttribute, value);
		}
	}

	public override IImportedEntity ImportedEntity => Texture;

	public uint NumMips
	{
		get
		{
			return GetAttribute<uint>(EntitySchema.TextureEntityType.NumMipsAttribute);
		}
		private set
		{
			SetAttribute(EntitySchema.TextureEntityType.NumMipsAttribute, value);
		}
	}

	public ITextureInstance Texture => InstanceEntity as ITextureInstance;

	public uint Width
	{
		get
		{
			return GetAttribute<uint>(EntitySchema.TextureEntityType.WidthAttribute);
		}
		private set
		{
			SetAttribute(EntitySchema.TextureEntityType.WidthAttribute, value);
		}
	}

	protected override AttributeInfo ClassNameAttribute => EntitySchema.TextureEntityType.ClassNameAttribute;

	protected override ChildInfo CookParametersChild => EntitySchema.TextureEntityType.CookParametersChild;

	protected override ChildInfo DataFilesChild => EntitySchema.TextureEntityType.DataFilesChild;

	protected override AttributeInfo DescriptionAttribute => EntitySchema.TextureEntityType.DescriptionAttribute;

	protected override AttributeInfo ExportedTimeAttribute => EntitySchema.TextureEntityType.ExportedTimeAttribute;

	protected override AttributeInfo ImportedTimeAttribute => EntitySchema.TextureEntityType.ImportedTimeAttribute;

	protected override AttributeInfo NameAttribute => EntitySchema.TextureEntityType.NameAttribute;

	protected override ChildInfo SourceFilePathChild => EntitySchema.TextureEntityType.SourceFilePathChild;

	protected override AttributeInfo SourceObjectNameAttribute => EntitySchema.TextureEntityType.SourceObjectNameAttribute;

	protected override ChildInfo TagsChild => EntitySchema.TextureEntityType.TagsChild;

	protected override void AssignPropertiesFromEntity(bool updateUI)
	{
		base.AssignPropertiesFromEntity(updateUI);
		if (Texture.Height != Height)
		{
			Height = Texture.Height;
		}
		if (Texture.Width != Width)
		{
			Width = Texture.Width;
		}
		if (Texture.Depth != Depth)
		{
			Depth = Texture.Depth;
		}
		if (Texture.NumMipMaps != NumMips)
		{
			NumMips = Texture.NumMipMaps;
		}
		ExportSettings.Update(Texture.ExportSettings, Texture);
	}

	protected virtual void HandleExportSettingsChanged(object sender, EventArgs e)
	{
		if (!string.IsNullOrEmpty(base.SourceFilePath))
		{
			PerformImport();
		}
	}

	protected override void OnClassChange()
	{
		if (global::DatabaseWrapper.DatabaseWrapper.GetClass(base.CivTechService.PrimaryProject.Name, InstanceType.IT_TEXTURE, ClassName) is ITextureClass textureClass)
		{
			ExportSettings.Update(textureClass.ExportOptions);
		}
		base.OnClassChange();
	}

	protected override void OnNodeSet()
	{
		DomNode domNode = new DomNode(EntitySchema.TextureExportSettingsType.Type);
		domNode.InitializeExtensions();
		base.DomNode.SetChild(EntitySchema.TextureEntityType.ExportSettingsChild, domNode);
		base.OnNodeSet();
	}

	protected override void RegisterForDomChanges()
	{
		base.RegisterForDomChanges();
		ExportSettings.ExportSettingsChanged += HandleExportSettingsChanged;
	}

	protected override void UnregisterFromDomChanges()
	{
		base.UnregisterFromDomChanges();
		ExportSettings.ExportSettingsChanged -= HandleExportSettingsChanged;
	}
}
