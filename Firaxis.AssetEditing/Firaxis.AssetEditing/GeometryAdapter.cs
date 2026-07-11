using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class GeometryAdapter : ImportedEntityAdapter
{
	public ulong BoneCount
	{
		get
		{
			return GetAttribute<ulong>(EntitySchema.GeometryEntityType.BoneCountAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.GeometryEntityType.BoneCountAttribute, value);
		}
	}

	public IGeometryInstance Geometry => InstanceEntity as IGeometryInstance;

	public GeometryModelAdapter GeometryModel => GetChild<GeometryModelAdapter>(EntitySchema.GeometryEntityType.GeoModelTypeChild);

	public override IImportedEntity ImportedEntity => Geometry;

	public ulong PrimitiveCount
	{
		get
		{
			return GetAttribute<ulong>(EntitySchema.GeometryEntityType.PrimitiveCountAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.GeometryEntityType.PrimitiveCountAttribute, value);
		}
	}

	public ulong VertexCount
	{
		get
		{
			return GetAttribute<ulong>(EntitySchema.GeometryEntityType.VertexCountAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.GeometryEntityType.VertexCountAttribute, value);
		}
	}

	protected override AttributeInfo ClassNameAttribute => EntitySchema.GeometryEntityType.ClassNameAttribute;

	protected override ChildInfo CookParametersChild => EntitySchema.GeometryEntityType.CookParametersChild;

	protected override ChildInfo DataFilesChild => EntitySchema.GeometryEntityType.DataFilesChild;

	protected override AttributeInfo DescriptionAttribute => EntitySchema.GeometryEntityType.DescriptionAttribute;

	protected override AttributeInfo ExportedTimeAttribute => EntitySchema.GeometryEntityType.ExportedTimeAttribute;

	protected override AttributeInfo ImportedTimeAttribute => EntitySchema.GeometryEntityType.ImportedTimeAttribute;

	protected override AttributeInfo NameAttribute => EntitySchema.GeometryEntityType.NameAttribute;

	protected override ChildInfo SourceFilePathChild => EntitySchema.GeometryEntityType.SourceFilePathChild;

	protected override AttributeInfo SourceObjectNameAttribute => EntitySchema.GeometryEntityType.SourceObjectNameAttribute;

	protected override ChildInfo TagsChild => EntitySchema.GeometryEntityType.TagsChild;

	protected override void AssignPropertiesFromEntity(bool updateUI)
	{
		base.AssignPropertiesFromEntity(updateUI);
		GeometryModel.AssignPropertiesFromEntity(Geometry);
		ulong num = 0uL;
		ulong num2 = 0uL;
		ulong num3 = 0uL;
		foreach (GeometryMeshAdapter geometryMesh in GeometryModel.GeometryMeshes)
		{
			num += geometryMesh.VertexCount;
			num2 += geometryMesh.PrimitiveCount;
			num3 += geometryMesh.BoundBoneCount;
		}
		VertexCount = num;
		PrimitiveCount = num2;
		BoneCount = num3;
	}

	protected override void OnNodeSet()
	{
		DomNode domNode = new DomNode(EntitySchema.GeoModelType.Type);
		domNode.InitializeExtensions();
		base.DomNode.SetChild(EntitySchema.GeometryEntityType.GeoModelTypeChild, domNode);
		base.OnNodeSet();
	}
}
