using System.Collections.Generic;
using System.Linq;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class GeometryModelAdapter : DomNodeAdapter
{
	private IList<TextElementAdapter> m_bones;

	private IList<GeometryMeshAdapter> m_geoMeshes;

	public IList<TextElementAdapter> Bones => m_bones;

	public IList<GeometryMeshAdapter> GeometryMeshes => m_geoMeshes;

	public void AssignPropertiesFromEntity(IGeometryInstance geo)
	{
		UpdateBoneList(geo);
		UpdateGeometryMeshList(geo);
	}

	protected override void OnNodeSet()
	{
		m_geoMeshes = new DomNodeListAdapter<GeometryMeshAdapter>(base.DomNode, EntitySchema.GeoModelType.GeometryMeshesChild);
		m_bones = new DomNodeListAdapter<TextElementAdapter>(base.DomNode, EntitySchema.GeoModelType.BonesChild);
		base.OnNodeSet();
	}

	private void UpdateBoneList(IGeometryInstance geo)
	{
		IList<TextElementAdapter> list = new List<TextElementAdapter>();
		foreach (string bone in geo.BoneNames)
		{
			TextElementAdapter textElementAdapter = Bones.FirstOrDefault((TextElementAdapter tea) => tea.Text == bone);
			if (textElementAdapter == null)
			{
				DomNode domNode = new DomNode(BaseSchema.TextElementType.Type);
				domNode.InitializeExtensions();
				textElementAdapter = domNode.As<TextElementAdapter>();
				textElementAdapter.Text = bone;
				Bones.Add(textElementAdapter);
			}
			list.Add(textElementAdapter);
		}
		foreach (var entryAdapter in Bones.Except(list).ToArray())
		{
			Bones.Remove(entryAdapter);
		}
	}

	private void UpdateGeometryMeshList(IGeometryInstance geo)
	{
		IList<GeometryMeshAdapter> list = new List<GeometryMeshAdapter>();
		foreach (IGeoMesh mesh in geo.GeometryMeshes)
		{
			GeometryMeshAdapter geometryMeshAdapter = GeometryMeshes.FirstOrDefault((GeometryMeshAdapter ugm) => ugm.Name == mesh.Name);
			if (geometryMeshAdapter == null)
			{
				DomNode domNode = new DomNode(EntitySchema.GeoMeshType.Type);
				domNode.InitializeExtensions();
				geometryMeshAdapter = domNode.As<GeometryMeshAdapter>();
				geometryMeshAdapter.Update(mesh);
				GeometryMeshes.Add(geometryMeshAdapter);
			}
			else
			{
				geometryMeshAdapter.Update(mesh);
			}
			list.Add(geometryMeshAdapter);
		}
		foreach (var entryAdapter in GeometryMeshes.Except(list).ToArray())
		{
			GeometryMeshes.Remove(entryAdapter);
		}
	}
}
