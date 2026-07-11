using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class CurveFieldValueAdapter : FieldValueAdapter
{
	private IEnumerable<System.ComponentModel.PropertyDescriptor> m_descriptors;

	public CurveAdapter CurveAdapter
	{
		get
		{
			return GetChild<CurveAdapter>(FieldSchema.CurveFieldValueType.ValueChild);
		}
		set
		{
			SetChild(FieldSchema.CurveFieldValueType.ValueChild, value);
		}
	}

	public override object ValueDataAsObject
	{
		get
		{
			return this;
		}
		set
		{
			MessageBox.Show("Tried to set value data on the CurveFieldValueAdapter.");
		}
	}

	private ICurveParameter CurveParameter => base.Parameter as ICurveParameter;

	private ICurveValue CurveValue => base.Value as ICurveValue;

	public override IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors => m_descriptors;

	public override void InitializeEditor(Func<bool> readOnlyFunctor)
	{
		base.InitializeEditor(readOnlyFunctor);
		System.ComponentModel.PropertyDescriptor[] array = new System.ComponentModel.PropertyDescriptor[1];
		int num = 0;
		string name = BindDynamicValueOrDefault(base.Parameter?.Name, "Element".Localize());
		ChildInfo valueChild = FieldSchema.CurveFieldValueType.ValueChild;
		string category = BindDynamicValueOrDefault(base.Parameter?.Category, "Value".Localize());
		array[num] = CreateProxyPropertyDescriptorIfNeeded(new ChildFieldPropertyDescriptor(name, valueChild, category, BindDynamicValueOrDefault(base.Parameter?.Description, "Curve value description".Localize()), readOnlyFunctor, new CurveValueEditor(FiraxisATFRegistry.ContextRegistry, FiraxisATFRegistry.CommandService, FiraxisATFRegistry.ThemeService)), Name);
		m_descriptors = new List<System.ComponentModel.PropertyDescriptor>(array);
	}

	public override void AddNativeField(IValueSet valSet, IParameter valParam)
	{
		base.AddNativeField(valSet, valParam);
		CurveAdapter.Initialize(CurveValue.ParameterValue);
	}

	public override void UpdateDomFromNative(IValue val)
	{
		base.UpdateDomFromNative(val);
		BugSubmitter.SilentAssert(typeof(ICurveValue).IsAssignableFrom(val.GetType()), "CurveFieldValueAdapter.UpdateDomFromNative was passed {0} which is not assignable to {1} is being updated from an incorrect type @summary CurveFieldValueAdapter is being updated from an incorrect type. @assign bwhitman", val.GetType(), typeof(ICurveValue));
		CurveAdapter.UpdateDomFromNative(CurveValue.ParameterValue);
	}

	public override void AssignDefaultValue()
	{
		ICurveValue curveValue = CurveParameter.DefaultValue as ICurveValue;
		CurveAdapter.UpdateDomFromNative(curveValue.ParameterValue);
	}

	public override bool RequiresUpdate(IValue val)
	{
		bool num = base.RequiresUpdate(val);
		bool flag = false;
		if (!num)
		{
			return !flag;
		}
		return true;
	}

	public override void UpdateNativeFromDom()
	{
		CurveValue.ParameterValue = CurveAdapter.Curve;
	}

	public override void CopyValue(IValue val)
	{
		base.CopyValue(val);
		ICurveValue curveValue = (ICurveValue)val;
		while (CurveAdapter.CurveSegments.Count > 0)
		{
			CurveAdapter.CurveSegments.RemoveAt(0);
		}
		foreach (ICurveSegmentDefinition curveSegment in curveValue.ParameterValue.CurveSegments)
		{
			CurveAdapter.Curve.AddCurveSegment(curveSegment);
		}
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
		DomNode domNode = new DomNode(FieldSchema.CurveType.Type);
		domNode.InitializeExtensions();
		CurveAdapter = domNode.As<CurveAdapter>();
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		IEntityChangeList entityChangeList = base.DomNode.GetRoot().As<BaseEntityPropertyContext>()?.BatchChangelist;
		InstanceEntityAdapter instanceEntityAdapter = base.DomNode.GetRoot().As<InstanceEntityAdapter>();
		if (instanceEntityAdapter != null && entityChangeList != null)
		{
			IInstanceEntity instanceEntity = instanceEntityAdapter.InstanceEntity;
			entityChangeList.CreateEntityCookParameterChangedEvent(instanceEntity.Type, instanceEntity.Name, Name, base.Value);
		}
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		IEntityChangeList entityChangeList = base.DomNode.GetRoot().As<BaseEntityPropertyContext>()?.BatchChangelist;
		InstanceEntityAdapter instanceEntityAdapter = base.DomNode.GetRoot().As<InstanceEntityAdapter>();
		if (instanceEntityAdapter != null && entityChangeList != null)
		{
			IInstanceEntity instanceEntity = instanceEntityAdapter.InstanceEntity;
			entityChangeList.CreateEntityCookParameterChangedEvent(instanceEntity.Type, instanceEntity.Name, Name, base.Value);
		}
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		IEntityChangeList entityChangeList = base.DomNode.GetRoot().As<BaseEntityPropertyContext>()?.BatchChangelist;
		InstanceEntityAdapter instanceEntityAdapter = base.DomNode.GetRoot().As<InstanceEntityAdapter>();
		if (instanceEntityAdapter != null && entityChangeList != null)
		{
			IInstanceEntity instanceEntity = instanceEntityAdapter.InstanceEntity;
			entityChangeList.CreateEntityCookParameterChangedEvent(instanceEntity.Type, instanceEntity.Name, Name, base.Value);
		}
	}
}
