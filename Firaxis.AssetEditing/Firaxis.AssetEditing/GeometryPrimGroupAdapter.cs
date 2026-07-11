using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class GeometryPrimGroupAdapter : DomNodeAdapter
{
	public uint FirstPrimIndex
	{
		get
		{
			return GetAttribute<uint>(EntitySchema.GeoPrimGroupType.FirstPrimIndexAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.GeoPrimGroupType.FirstPrimIndexAttribute, value);
		}
	}

	public string Name
	{
		get
		{
			return GetAttribute<string>(EntitySchema.GeoPrimGroupType.NameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.GeoPrimGroupType.NameAttribute, value);
		}
	}

	public uint PrimitiveCount
	{
		get
		{
			return GetAttribute<uint>(EntitySchema.GeoPrimGroupType.PrimCountAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.GeoPrimGroupType.PrimCountAttribute, value);
		}
	}
}
