using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DatabaseWrapper;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class TextureEntityPropertyContext : BaseEntityPropertyContext
{
	private ISet<AttributeInfo> s_impAttrBlacklist = new HashSet<AttributeInfo>
	{
		EntitySchema.TextureEntityType.SourceObjectNameAttribute,
		EntitySchema.TextureEntityType.ImportedTimeAttribute,
		EntitySchema.TextureEntityType.ExportedTimeAttribute,
		EntitySchema.TextureEntityType.HeightAttribute,
		EntitySchema.TextureEntityType.WidthAttribute,
		EntitySchema.TextureEntityType.DepthAttribute,
		EntitySchema.TextureEntityType.NumMipsAttribute
	};

	private ISet<ChildInfo> s_impChildBlacklist = new HashSet<ChildInfo>
	{
		EntitySchema.TextureEntityType.DataFilesChild,
		EntitySchema.TextureEntityType.SourceFilePathChild
	};

	public override IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors
	{
		get
		{
			IEnumerable<System.ComponentModel.PropertyDescriptor> sharedProperties = PropertyUtils.GetSharedProperties(Items);
			ImportedEntityAdapter importedEntityAdapter = base.DomNode.GetRoot().As<ImportedEntityAdapter>();
			IClassEntity classEntity = global::DatabaseWrapper.DatabaseWrapper.GetClass(importedEntityAdapter.CivTechService.PrimaryProject.Name, importedEntityAdapter.InstanceType, importedEntityAdapter.ClassName);
			if (classEntity == null || classEntity.DataFiles.Count() > 0)
			{
				foreach (System.ComponentModel.PropertyDescriptor item in sharedProperties)
				{
					yield return item;
				}
				yield break;
			}
			foreach (System.ComponentModel.PropertyDescriptor prop2 in sharedProperties)
			{
				if (prop2 is AttributePropertyDescriptor attributePropertyDescriptor && !s_impAttrBlacklist.Contains(attributePropertyDescriptor.AttributeInfo))
				{
					yield return prop2;
				}
				if (prop2 is ChildPropertyDescriptor childPropertyDescriptor && !s_impChildBlacklist.Contains(childPropertyDescriptor.ChildInfo))
				{
					yield return prop2;
				}
			}
		}
	}
}
