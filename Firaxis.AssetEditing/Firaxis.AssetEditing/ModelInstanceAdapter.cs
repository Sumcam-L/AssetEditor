using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ModelInstanceAdapter : AssetComponentAdapterBase
{
	private IList<TextElementAdapter> m_bones;

	private IList<PrimGroupStateAdapter> m_primGroups;

	private readonly IDictionary<int, PrimGroupStateAdapter> m_primLookupMap = new Dictionary<int, PrimGroupStateAdapter>();

	public ulong BoneCount => ((ulong?)GeometryInstance?.GeometryMeshes?.Sum((IGeoMesh gm) => gm.BoundBoneCount)).GetValueOrDefault();

	public ulong PrimitiveCount
	{
		get
		{
			IGeometryInstance geometryInstance = GeometryInstance;
			ulong? num;
			if (geometryInstance == null)
			{
				num = null;
			}
			else
			{
				IEnumerable<IGeoMesh> geometryMeshes = geometryInstance.GeometryMeshes;
				if (geometryMeshes == null)
				{
					num = null;
				}
				else
				{
					long? num2 = geometryMeshes.Sum((IGeoMesh gm) => gm.PrimitiveCount);
					num = (num2.HasValue ? new ulong?((ulong)num2.Value) : ((ulong?)null));
				}
			}
			return num.GetValueOrDefault();
		}
	}

	public ulong VertexCount => ((ulong?)GeometryInstance?.GeometryMeshes?.Sum((IGeoMesh gm) => gm.VertexCount)).GetValueOrDefault();

	public IList<TextElementAdapter> Bones => m_bones;

	public string GeometryClassName { get; set; }

	public IGeometryInstance GeometryInstance { get; set; }

	public string GeoName
	{
		get
		{
			return GetAttribute<string>(EntitySchema.ModelInstanceType.GeoNameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.ModelInstanceType.GeoNameAttribute, value);
		}
	}

	public IModelInstance ModelInstance { get; set; }

	public string Name
	{
		get
		{
			return GetAttribute<string>(EntitySchema.ModelInstanceType.NameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.ModelInstanceType.NameAttribute, value);
		}
	}

	public IList<PrimGroupStateAdapter> PrimGroups => m_primGroups;

	public void FreezeUpdates()
	{
		UnregisterFromDomChanges();
	}

	public void ClearModelInstancePrimGroups()
	{
		ModelInstance.ClearPrimGroups();
		m_primLookupMap.Clear();
	}

	public PrimGroupStateAdapter AddPrimGroupState(string meshName, string primGroupName, string stateName)
	{
		DomNode domNode = new DomNode(EntitySchema.PrimGroupStateType.Type);
		domNode.InitializeExtensions();
		PrimGroupStateAdapter primGroupStateAdapter = domNode.As<PrimGroupStateAdapter>();
		primGroupStateAdapter.ModelName = ModelInstance?.Name ?? Name;
		primGroupStateAdapter.MeshName = meshName;
		primGroupStateAdapter.StateName = stateName;
		primGroupStateAdapter.GroupName = primGroupName;
		PrimGroups.Add(primGroupStateAdapter);
		return primGroupStateAdapter;
	}

	public PrimGroupStateAdapter FindPrimGroupState(string meshName, string groupName, string stateName)
	{
		int key = GenerateHashCode(meshName, groupName, stateName);
		PrimGroupStateAdapter value = null;
		m_primLookupMap.TryGetValue(key, out value);
		return value;
	}

	public void Update(IModelInstance model)
	{
		UnregisterFromDomChanges();
		if (Name != model.Name)
		{
			Name = model.Name;
		}
		if (GeoName != model.GeoName)
		{
			GeoName = model.GeoName;
		}
		ModelInstance = model;
		UpdateClassName();
		GeometryClassName = (GeometryInstance = base.AssetAdapter.InstanceSet.LoadEntityIfUnique<IGeometryInstance>(GeoName))?.ClassName;
		if (!string.IsNullOrEmpty(GeometryClassName))
		{
			IList<PrimGroupStateAdapter> list = new List<PrimGroupStateAdapter>();
			foreach (IPrimGroupState primGroup in model.PrimGroups)
			{
				int key = GenerateHashCode(primGroup);
				PrimGroupStateAdapter value = null;
				if (!m_primLookupMap.TryGetValue(key, out value))
				{
					DomNode domNode = new DomNode(EntitySchema.PrimGroupStateType.Type);
					domNode.InitializeExtensions();
					value = domNode.As<PrimGroupStateAdapter>();
					PrimGroups.Add(value);
					m_primLookupMap[key] = value;
				}
				value.Update(GeoName, GeometryClassName, model.Name, primGroup);
				list.Add(value);
			}
			foreach (var entryAdapter in PrimGroups.Except(list).ToArray())
		{
			RemovePrimGroupFromMap(entryAdapter);
		}
		}
		RegisterForDomChanges();
	}

	protected override void OnNodeSet()
	{
		m_primGroups = new DomNodeListAdapter<PrimGroupStateAdapter>(base.DomNode, EntitySchema.ModelInstanceType.PrimGroupStatesChild);
		m_bones = new DomNodeListAdapter<TextElementAdapter>(base.DomNode, EntitySchema.GeoModelType.BonesChild);
		RegisterForDomChanges();
		base.OnNodeSet();
	}

	protected override void OnParentNodeSet()
	{
		base.OnParentNodeSet();
		if (base.AssetAdapter.AssetClass.AllowedGeometryClasses.Count() == 1)
		{
			GeometryClassName = base.AssetAdapter.AssetClass.AllowedGeometryClasses.First();
		}
	}

	protected virtual void RegisterForDomChanges()
	{
		UnregisterFromDomChanges();
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	protected virtual void UnregisterFromDomChanges()
	{
		base.DomNode.ChildInserted -= DomNode_ChildInserted;
		base.DomNode.ChildRemoved -= DomNode_ChildRemoved;
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == EntitySchema.ModelInstanceType.PrimGroupStatesChild)
		{
			PrimGroupStateAdapter primGroupStateAdapter = e.Child.As<PrimGroupStateAdapter>();
			primGroupStateAdapter.PrimGroupState = ModelInstance.AddPrimGroupState(primGroupStateAdapter.MeshName, primGroupStateAdapter.GroupName, primGroupStateAdapter.StateName);
			int key = GenerateHashCode(primGroupStateAdapter);
			m_primLookupMap[key] = primGroupStateAdapter;
		}
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == EntitySchema.ModelInstanceType.PrimGroupStatesChild)
		{
			PrimGroupStateAdapter primGroupStateAdapter = e.Child.As<PrimGroupStateAdapter>();
			int key = GenerateHashCode(primGroupStateAdapter);
			m_primLookupMap.Remove(key);
			ModelInstance.RemoveGroupState(primGroupStateAdapter.PrimGroupState);
		}
	}

	private int GenerateHashCode(string mesh, string group, string state)
	{
		return (mesh + "_" + group + "_" + state).GetHashCode();
	}

	private int GenerateHashCode(IPrimGroupState state)
	{
		return GenerateHashCode(state.MeshName, state.GroupName, state.StateName);
	}

	private int GenerateHashCode(PrimGroupStateAdapter adapter)
	{
		return GenerateHashCode(adapter.MeshName, adapter.GroupName, adapter.StateName);
	}

	private void RemovePrimGroupFromMap(PrimGroupStateAdapter primGroupStateAdapter)
	{
		int key = GenerateHashCode(primGroupStateAdapter);
		m_primLookupMap.Remove(key);
		PrimGroups.Remove(primGroupStateAdapter);
	}

	private void UpdateClassName()
	{
		if (string.IsNullOrEmpty(GeometryClassName))
		{
			IGeometryInstance geometryInstance = base.AssetAdapter.InstanceSet.LoadEntityIfUnique<IGeometryInstance>(GeoName);
			if (geometryInstance != null)
			{
				GeometryClassName = geometryInstance.ClassName;
				return;
			}
			PrimGroups.Clear();
			string text = "Could not load a geometry with the name " + GeoName + ".  Ensure that it's been added to the pantry.";
			Outputs.WriteLine(OutputMessageType.Error, text);
			MessageBox.Show(text, "Could Not Load Geometry");
		}
	}
}
