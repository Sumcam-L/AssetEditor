using System.Collections.Generic;
using System.Linq;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class GeometryMeshAdapter : DomNodeAdapter
{
	private IList<GeometryPrimGroupAdapter> m_primGroups;

	public uint BoundBoneCount
	{
		get
		{
			return GetAttribute<uint>(EntitySchema.GeoMeshType.BoundBoneCountAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.GeoMeshType.BoundBoneCountAttribute, value);
		}
	}

	public string Name
	{
		get
		{
			return GetAttribute<string>(EntitySchema.GeoMeshType.NameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.GeoMeshType.NameAttribute, value);
		}
	}

	public IList<GeometryPrimGroupAdapter> PrimGroups => m_primGroups;

	public uint PrimitiveCount
	{
		get
		{
			return GetAttribute<uint>(EntitySchema.GeoMeshType.PrimitiveCountAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.GeoMeshType.PrimitiveCountAttribute, value);
		}
	}

	public uint VertexCount
	{
		get
		{
			return GetAttribute<uint>(EntitySchema.GeoMeshType.VertexCountAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.GeoMeshType.VertexCountAttribute, value);
		}
	}

	public void Update(IGeoMesh geoMesh)
	{
		Name = geoMesh.Name;
		BoundBoneCount = geoMesh.BoundBoneCount;
		PrimitiveCount = geoMesh.PrimitiveCount;
		VertexCount = geoMesh.VertexCount;
		UpdateGeometryPrimGroups(geoMesh);
	}

	protected override void OnNodeSet()
	{
		m_primGroups = new DomNodeListAdapter<GeometryPrimGroupAdapter>(base.DomNode, EntitySchema.GeoMeshType.PrimGroupsChild);
		base.OnNodeSet();
	}

	private void UpdateGeometryPrimGroups(IGeoMesh geoMesh)
	{
		IList<GeometryPrimGroupAdapter> list = new List<GeometryPrimGroupAdapter>();
		foreach (IGeoPrimGroup primGroup in geoMesh.GeoPrimGroups)
		{
			GeometryPrimGroupAdapter geometryPrimGroupAdapter = PrimGroups.FirstOrDefault((GeometryPrimGroupAdapter ugm) => ugm.Name == primGroup.Name);
			if (geometryPrimGroupAdapter == null)
			{
				DomNode domNode = new DomNode(EntitySchema.GeoPrimGroupType.Type);
				domNode.InitializeExtensions();
				geometryPrimGroupAdapter = domNode.As<GeometryPrimGroupAdapter>();
				geometryPrimGroupAdapter.Name = primGroup.Name;
				geometryPrimGroupAdapter.FirstPrimIndex = primGroup.NumFirstPrim;
				geometryPrimGroupAdapter.PrimitiveCount = primGroup.NumPrims;
				PrimGroups.Add(geometryPrimGroupAdapter);
			}
			list.Add(geometryPrimGroupAdapter);
		}
		foreach (var entryAdapter in PrimGroups.Except(list).ToArray())
		{
			PrimGroups.Remove(entryAdapter);
		}
	}
}
