using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseWrapper;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class AttachmentPointAdapter : BehaviorComponentAdapterBase, IFieldContainerAdapter
{
	private bool m_firstUpdate = true;

	private IDictionary<string, IEnumerable<string>> m_modelNameBonesMap = new Dictionary<string, IEnumerable<string>>();

	public IAttachmentPoint AttachmentPoint { get; set; }

	public string BoneName
	{
		get
		{
			return GetAttribute<string>(EntitySchema.AttachmentPointType.BoneNameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.AttachmentPointType.BoneNameAttribute, value);
		}
	}

	public CookParameterSetAdapter CookParameterSet { get; private set; }

	public IEnumerable<string> CurrentBoneNames
	{
		get
		{
			if (ModelInstanceName == null)
			{
				return Enumerable.Empty<string>();
			}
			if (m_modelNameBonesMap.Count == 0)
			{
				PopulateModelNamesBoneMap();
			}
			if (m_modelNameBonesMap.ContainsKey(ModelInstanceName))
			{
				return m_modelNameBonesMap[ModelInstanceName];
			}
			return Enumerable.Empty<string>();
		}
	}

	public string ModelInstanceName
	{
		get
		{
			return GetAttribute<string>(EntitySchema.AttachmentPointType.ModelInstanceNameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.AttachmentPointType.ModelInstanceNameAttribute, value);
		}
	}

	public IEnumerable<string> ModelInstanceNames
	{
		get
		{
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			foreach (string referenceGeometryName in base.BehaviorProvider.ReferenceGeometryNames)
			{
				IGeometryInstance geometryInstance = base.EntityAdapter.Instances.LoadEntityIfUnique<IGeometryInstance>(referenceGeometryName);
				if (geometryInstance != null)
				{
					list.Add(geometryInstance.ModelName);
					IEnumerable<string> enumerable = new List<string>(geometryInstance.BoneNames);
					m_modelNameBonesMap[geometryInstance.ModelName] = enumerable;
					list2.AddRange(enumerable);
				}
			}
			list.Add(string.Empty);
			m_modelNameBonesMap[string.Empty] = list2;
			return list;
		}
	}

	public string Name
	{
		get
		{
			return GetAttribute<string>(EntitySchema.AttachmentPointType.NameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.AttachmentPointType.NameAttribute, value);
		}
	}

	public float[] Orientation
	{
		get
		{
			return GetAttribute<float[]>(EntitySchema.AttachmentPointType.OrientationAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.AttachmentPointType.OrientationAttribute, value);
		}
	}

	public float[] Position
	{
		get
		{
			return GetAttribute<float[]>(EntitySchema.AttachmentPointType.PositionAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.AttachmentPointType.PositionAttribute, value);
		}
	}

	public float Scale
	{
		get
		{
			return GetAttribute<float>(EntitySchema.AttachmentPointType.ScaleAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.AttachmentPointType.ScaleAttribute, value);
		}
	}

	public IList<IFieldValueAdapter> Fields => CookParameterSet?.Fields;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public CookParameterSetAdapter InitializeCookParameters(IClassEntity config)
	{
		CookParameterSet = CreateCookParameterSet();
		AddParametersFromConfig(config, CookParameterSet);
		return CookParameterSet;
	}

	public void RemoveCookParameters()
	{
		CookParameterSet.ValueSet.Clear();
		CookParameterSet.Fields.Clear();
	}

	public void SetPositionAndOrientationNoDomUpdate(IFloatVector3 pos, IFloatVector3 rot)
	{
		UnregisterForDomNotifications();
		GetTransactionContext().DoTransaction(delegate
		{
			Position = new float[3] { pos.X, pos.Y, pos.Z };
			Orientation = new float[3] { rot.X, rot.Y, rot.Z };
		}, "Edit Attachment Point");
		RegisterForDomNotifications();
	}

	public void Update(IAttachmentPoint attachmentPoint)
	{
		UnregisterForDomNotifications();
		if (!m_firstUpdate)
		{
			HasChanged(attachmentPoint);
		}
		else
			_ = 0;
		_ = Name;
		AttachmentPoint = attachmentPoint;
		if (CookParameterSet == null)
		{
			CookParameterSet = CreateCookParameterSet();
		}
		Name = attachmentPoint.Name;
		BoneName = attachmentPoint.BoneName;
		ModelInstanceName = attachmentPoint.ModelInstanceName;
		Position = new float[3]
		{
			attachmentPoint.Position.X,
			attachmentPoint.Position.Y,
			attachmentPoint.Position.Z
		};
		Orientation = new float[3]
		{
			attachmentPoint.Orientation.X,
			attachmentPoint.Orientation.Y,
			attachmentPoint.Orientation.Z
		};
		Scale = attachmentPoint.Scale;
		if (global::DatabaseWrapper.DatabaseWrapper.GetClass(base.EntityAdapter.CivTechService.PrimaryProject.Name, base.EntityAdapter.InstanceType, base.EntityAdapter.InstanceEntity.ClassName) is IAttachmentContainer attachmentContainer)
		{
			CookParameterSet.Update(attachmentPoint.CookParameters, attachmentContainer.AttachmentParams, CookParameterChanged, updateUI: false);
		}
		RegisterForDomNotifications();
		m_firstUpdate = false;
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		Scale = 1f;
		RegisterForDomNotifications();
	}

	private void AddParametersFromConfig(IClassEntity config, CookParameterSetAdapter cookParams)
	{
		if (!(config is IAttachmentContainer attachmentContainer))
		{
			return;
		}
		foreach (IParameter item in attachmentContainer.AttachmentParams.Items)
		{
			cookParams.AddParameter(item, CookParameterChanged);
		}
	}

	private void CookParameterChanged(object sender, AttributeEventArgs e)
	{
		IFieldValueAdapter fieldValueAdapter = e.DomNode.As<IFieldValueAdapter>();
		if (fieldValueAdapter != null)
		{
			IValue value = fieldValueAdapter.Value;
			if (value != null)
			{
				base.BatchChangelist?.CreateAttachmentCookParameterChangedEvent(base.BehaviorProvider.InstanceEntity, Name, fieldValueAdapter.Name, value);
			}
		}
	}

	private CookParameterSetAdapter CreateCookParameterSet()
	{
		DomNode domNode = new DomNode(EntitySchema.CookParametersSetType.Type);
		domNode.InitializeExtensions();
		CookParameterSetAdapter cookParameterSetAdapter = domNode.As<CookParameterSetAdapter>();
		cookParameterSetAdapter.EntityAdapter = base.EntityAdapter;
		base.DomNode.SetChild(EntitySchema.AttachmentPointType.CookParametersChild, domNode);
		return cookParameterSetAdapter;
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (base.BehaviorProvider == null)
		{
			return;
		}
		string oldAttachmentName = Name;
		if (e.AttributeInfo == EntitySchema.AttachmentPointType.NameAttribute)
		{
			oldAttachmentName = e.OldValue.ToString();
			base.DomNode.GetRoot().As<EditingContext>();
			if (base.BehaviorProvider.BehaviorData.AttachmentPointSet.FindByName(e.NewValue.ToString()) == null)
			{
				AttachmentPointAdapter attachmentPointAdapter = (sender as DomNode).As<AttachmentPointAdapter>();
				string xml = AttachmentPoint.CookParameters.SerializeIntoXML();
				base.BehaviorProvider.BehaviorData.AttachmentPointSet.RemoveAttachmentPoint(e.OldValue.ToString());
				attachmentPointAdapter.AttachmentPoint = base.BehaviorProvider.BehaviorData.AttachmentPointSet.AddAttachmentPoint(e.NewValue.ToString(), attachmentPointAdapter.BoneName, attachmentPointAdapter.ModelInstanceName);
				AttachmentPoint.Orientation = Context.EnsureCreated<CivTechContext>().CreateInstance<IFloatVector3>(new object[3]
				{
					attachmentPointAdapter.Orientation[0],
					attachmentPointAdapter.Orientation[1],
					attachmentPointAdapter.Orientation[2]
				});
				AttachmentPoint.Position = Context.EnsureCreated<CivTechContext>().CreateInstance<IFloatVector3>(new object[3]
				{
					attachmentPointAdapter.Position[0],
					attachmentPointAdapter.Position[1],
					attachmentPointAdapter.Position[2]
				});
				AttachmentPoint.Scale = attachmentPointAdapter.Scale;
				AttachmentPoint.CookParameters.DeserializeFromXML(xml);
				attachmentPointAdapter.Update(AttachmentPoint);
			}
			else
			{
				base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
				Name = (string)e.OldValue;
				base.DomNode.AttributeChanged += DomNode_AttributeChanged;
				string message = "Attachment point named \"" + (string)e.NewValue + "\" already exists!";
				Outputs.WriteLine(OutputMessageType.Error, message);
			}
		}
		else if (e.AttributeInfo == EntitySchema.AttachmentPointType.BoneNameAttribute)
		{
			base.BehaviorProvider.BehaviorData.AttachmentPointSet.FindByName(Name).BoneName = (string)e.NewValue;
		}
		else if (e.AttributeInfo == EntitySchema.AttachmentPointType.ModelInstanceNameAttribute)
		{
			base.BehaviorProvider.BehaviorData.AttachmentPointSet.FindByName(Name).ModelInstanceName = (string)e.NewValue;
		}
		else if (e.AttributeInfo == EntitySchema.AttachmentPointType.PositionAttribute)
		{
			IAttachmentPoint attachmentPoint = base.BehaviorProvider.BehaviorData.AttachmentPointSet.FindByName(Name);
			float[] position = Position;
			attachmentPoint.Position = Context.EnsureCreated<CivTechContext>().CreateInstance<IFloatVector3>(new object[3]
			{
				position[0],
				position[1],
				position[2]
			});
		}
		else if (e.AttributeInfo == EntitySchema.AttachmentPointType.OrientationAttribute)
		{
			IAttachmentPoint attachmentPoint2 = base.BehaviorProvider.BehaviorData.AttachmentPointSet.FindByName(Name);
			float[] orientation = Orientation;
			for (int i = 0; i < orientation.Length; i++)
			{
				orientation[i] = (float)MathUtil.Wrap(orientation[i], -Math.PI, Math.PI);
				if (MathUtil.AreApproxEqual(orientation[i], 0.0, 1E-06) || MathUtil.AreApproxEqual(orientation[i], 360.0, 1E-06))
				{
					orientation[i] = 0f;
				}
			}
			attachmentPoint2.Orientation = Context.EnsureCreated<CivTechContext>().CreateInstance<IFloatVector3>(new object[3]
			{
				orientation[0],
				orientation[1],
				orientation[2]
			});
		}
		else if (e.AttributeInfo == EntitySchema.AttachmentPointType.ScaleAttribute)
		{
			base.BehaviorProvider.BehaviorData.AttachmentPointSet.FindByName(Name).Scale = Scale;
		}
		base.BatchChangelist?.CreateAttachmentChangedEvent(base.BehaviorProvider.InstanceEntity, oldAttachmentName, Name, ModelInstanceName, BoneName, Position, Orientation, Scale);
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == EntitySchema.AttachmentPointType.CookParametersChild && e.Parent == base.DomNode)
		{
			e.Child.As<CookParameterSetAdapter>().ValueSet = AttachmentPoint.CookParameters;
		}
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
	}

	private bool HasChanged(IAttachmentPoint attachmentPoint)
	{
		if (AttachmentPoint != attachmentPoint)
		{
			return true;
		}
		if (Name != attachmentPoint.Name)
		{
			return true;
		}
		if (BoneName != attachmentPoint.BoneName)
		{
			return true;
		}
		if (ModelInstanceName != attachmentPoint.ModelInstanceName)
		{
			return true;
		}
		if (Scale != attachmentPoint.Scale)
		{
			return true;
		}
		if (!attachmentPoint.Position.IsEqualTo(Position))
		{
			return true;
		}
		return !attachmentPoint.Orientation.IsEqualTo(Orientation);
	}

	private void PopulateModelNamesBoneMap()
	{
		List<string> list = new List<string>();
		foreach (string referenceGeometryName in base.BehaviorProvider.ReferenceGeometryNames)
		{
			IGeometryInstance geometryInstance = base.EntityAdapter.Instances.LoadEntityIfUnique<IGeometryInstance>(referenceGeometryName);
			if (geometryInstance != null)
			{
				IEnumerable<string> enumerable = new List<string>(geometryInstance.BoneNames).OrderBy((string s) => s);
				m_modelNameBonesMap[geometryInstance.ModelName] = enumerable;
				list.AddRange(enumerable);
			}
		}
		IOrderedEnumerable<string> value = list.OrderBy((string s) => s);
		m_modelNameBonesMap[string.Empty] = value;
	}

	private void RegisterForDomNotifications()
	{
		UnregisterForDomNotifications();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
	}

	private void UnregisterForDomNotifications()
	{
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		base.DomNode.ChildInserted -= DomNode_ChildInserted;
		base.DomNode.ChildRemoved -= DomNode_ChildRemoved;
	}

	protected virtual void OnItemInserted(int index, object item)
	{
		this.ItemInserted?.Invoke(this, new ItemInsertedEventArgs<object>(index, item));
	}

	protected virtual void OnItemRemoved(int index, object item)
	{
		this.ItemRemoved?.Invoke(this, new ItemRemovedEventArgs<object>(index, item));
	}
}
