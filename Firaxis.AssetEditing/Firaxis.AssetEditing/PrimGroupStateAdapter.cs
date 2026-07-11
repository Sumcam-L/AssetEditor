using System;
using System.Collections.Generic;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Error;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class PrimGroupStateAdapter : AssetComponentAdapterBase, IFieldContainerAdapter
{
	private string m_cachedMeshName;

	public string GroupName
	{
		get
		{
			return GetAttribute<string>(EntitySchema.PrimGroupStateType.GroupNameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.PrimGroupStateType.GroupNameAttribute, value);
		}
	}

	public string MeshName
	{
		get
		{
			return m_cachedMeshName;
		}
		set
		{
			SetAttribute(EntitySchema.PrimGroupStateType.MeshNameAttribute, value);
		}
	}

	public IPrimGroupState PrimGroupState { get; set; }

	public string StateName
	{
		get
		{
			return GetAttribute<string>(EntitySchema.PrimGroupStateType.StateNameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.PrimGroupStateType.StateNameAttribute, value);
		}
	}

	public string ModelName { get; set; }

	private IDictionary<string, IFieldValueAdapter> FieldMap { get; set; }

	private IGeometryClass GeometryClass { get; set; }

	private string GeometryClassName { get; set; }

	public IList<IFieldValueAdapter> Fields { get; private set; }

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public void FreezeUpdates()
	{
		UnregisterFromDomNotifications();
	}

	public void AddDefaultValuesAsNecessary(IParameterSet geoParamSet)
	{
		foreach (IParameter fldDef in geoParamSet.Items)
		{
			IFieldValueAdapter fieldValueAdapter = Fields.FirstOrDefault((IFieldValueAdapter fld) => fld.Name == fldDef.Name);
			if (fieldValueAdapter == null)
			{
				fieldValueAdapter = FieldValueHelper.CreateField(fldDef);
				Fields.Add(fieldValueAdapter);
				FieldMap[fieldValueAdapter.Name] = fieldValueAdapter;
			}
			fieldValueAdapter.AssignDefaultValue();
		}
	}

	public bool IsBasedOn(IPrimGroupState groupState)
	{
		if (MeshName == groupState.MeshName && GroupName == groupState.GroupName)
		{
			return StateName == groupState.StateName;
		}
		return false;
	}

	public void RestoreState(string geoClassName, IValueSet originalValueSet)
	{
		PrimGroupState.Values.CopyFrom(originalValueSet);
		GeometryClass = base.ProjectConfig.Classes.Items.OfType<IGeometryClass>().FirstOrDefault((IGeometryClass geoClass) => geoClass.Name == geoClassName);
		UnregisterFromDomNotifications();
		UpdateGroupParameters(string.Empty, GeometryClass, PrimGroupState, updatePreviewer: true);
		RegisterForDomNotifications();
	}

	public void Update(string geometryName, string geometryClassName, string modelName, IPrimGroupState groupState)
	{
		UnregisterFromDomNotifications();
		ModelName = modelName;
		MeshName = (m_cachedMeshName = groupState.MeshName);
		GroupName = groupState.GroupName;
		StateName = groupState.StateName;
		if (GeometryClassName != geometryClassName)
		{
			GeometryClassName = geometryClassName;
			GeometryClass = base.ProjectConfig.Classes.Items.OfType<IGeometryClass>().FirstOrDefault((IGeometryClass geoClass) => geoClass.Name == geometryClassName);
		}
		UpdateGroupParameters(geometryName, GeometryClass, groupState, updatePreviewer: false);
		RegisterForDomNotifications();
	}

	protected override void OnNodeSet()
	{
		FieldMap = new Dictionary<string, IFieldValueAdapter>();
		Fields = new DomNodeListAdapter<IFieldValueAdapter>(base.DomNode, EntitySchema.PrimGroupStateType.CookParametersChild);
		RegisterForDomNotifications();
		base.OnNodeSet();
	}

	private void UpdateGroupParameters(string geometryName, IGeometryClass geoClass, IPrimGroupState groupState, bool updatePreviewer)
	{
		if (geoClass == null)
		{
			Outputs.WriteLine(OutputMessageType.Error, "Could not load the class for Geometry {0}.  Its group states will be cleared.", geometryName);
			Fields.Clear();
			FieldMap.Clear();
			return;
		}
		PrimGroupState = groupState;
		IList<IFieldValueAdapter> list = new List<IFieldValueAdapter>();
		IValueSet values = groupState.Values;
		IParameterSet groupParameters = geoClass.GroupParameters;
		values.RemoveUnusedValues(groupParameters);
		values.AddDefaultValuesAsNecessary(groupParameters);
		foreach (IValue value in values.Items)
		{
			IFieldValueAdapter value2 = Fields.FirstOrDefault((IFieldValueAdapter ufv) => ufv.Parameter.Name == value.ParameterName && ufv.Parameter.ParameterValueType == value.ParameterType);
			bool flag = false;
			if (!FieldMap.TryGetValue(value.ParameterName, out value2) || value2.Parameter.ParameterValueType != value.ParameterType)
			{
				value2 = FieldValueHelper.CreateField(value.ParameterName, groupParameters);
				PlatformAssert.If(value2 == null, "Tried to create a parameter that's not supported by this parameter set.");
				Fields.Add(value2);
				FieldMap[value.ParameterName] = value2;
				((FieldValueAdapter)value2).UpdateDomFromNative(value);
				FieldValueHelper.UpdateObjectValues(value2, groupParameters);
			}
			else
			{
				flag = value2.RequiresUpdate(value);
				((FieldValueAdapter)value2).UpdateDomFromNative(value);
			}
			list.Add(value2);
			if (updatePreviewer && flag)
			{
				base.BatchChangelist?.CreateTriGroupParameterChangedEvent(base.ParentAsset, ModelName, MeshName, GroupName, StateName, value2.Name, ((FieldValueAdapter)value2).Value);
			}
		}
		foreach (var entryAdapter in Fields.Except(list).ToArray())
		{
			FieldMap.Remove(entryAdapter.Name);
			Fields.Remove(entryAdapter);
		}
	}

	protected virtual void OnItemInserted(int index, object item)
	{
		this.ItemInserted?.Invoke(this, new ItemInsertedEventArgs<object>(index, item));
	}

	protected virtual void OnItemRemoved(int index, object item)
	{
		this.ItemRemoved?.Invoke(this, new ItemRemovedEventArgs<object>(index, item));
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == EntitySchema.PrimGroupStateType.MeshNameAttribute && e.DomNode == base.DomNode)
		{
			m_cachedMeshName = (string)e.NewValue;
		}
		FieldValueAdapter fieldValueAdapter = e.DomNode.As<FieldValueAdapter>();
		if (fieldValueAdapter != null)
		{
			base.BatchChangelist?.CreateTriGroupParameterChangedEvent(base.ParentAsset, ModelName, MeshName, GroupName, StateName, fieldValueAdapter.Name, fieldValueAdapter.Value);
		}
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		IFieldValueAdapter fieldValueAdapter = e.Child.As<IFieldValueAdapter>();
		fieldValueAdapter?.AddNativeField(PrimGroupState.Values, fieldValueAdapter.Parameter);
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		IFieldValueAdapter fieldValueAdapter = e.Child.As<IFieldValueAdapter>();
		if (fieldValueAdapter != null)
		{
			PrimGroupState.Values.Remove(fieldValueAdapter.Value);
		}
	}

	private void RegisterForDomNotifications()
	{
		UnregisterFromDomNotifications();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
	}

	private void UnregisterFromDomNotifications()
	{
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		base.DomNode.ChildInserted -= DomNode_ChildInserted;
		base.DomNode.ChildRemoved -= DomNode_ChildRemoved;
	}
}
