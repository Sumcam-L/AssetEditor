using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Error;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ObjectFieldValueAdapter : FieldValueAdapter, IAssetBrowserTypeProvider
{
	private IEnumerable<System.ComponentModel.PropertyDescriptor> m_descriptors;

	private ObjectValueInfo m_dataObject;

	public string Class
	{
		get
		{
			return GetAttribute<string>(FieldSchema.ObjectFieldValueType.ClassAttribute);
		}
		set
		{
			SetAttribute(FieldSchema.ObjectFieldValueType.ClassAttribute, value);
		}
	}

	private IObjectValue DefaultValue => ObjectParameter.DefaultValue as IObjectValue;

	public override object DefaultDataAsObject => DefaultValue.GetBoundObjectName();

	public string ObjectName
	{
		get
		{
			return GetAttribute<string>(FieldSchema.ObjectFieldValueType.ObjectNameAttribute);
		}
		set
		{
			SetAttribute(FieldSchema.ObjectFieldValueType.ObjectNameAttribute, StaticMethods.SanitizeEntityName(value));
		}
	}

	public IObjectParameter ObjectParameter => base.Parameter as IObjectParameter;

	public string ObjectType
	{
		get
		{
			return GetAttribute<string>(FieldSchema.ObjectFieldValueType.ObjectTypeAttribute);
		}
		set
		{
			SetAttribute(FieldSchema.ObjectFieldValueType.ObjectTypeAttribute, value);
		}
	}

	public InstanceType ObjectTypeValue
	{
		get
		{
			if (ObjectParameter == null)
			{
				return InstanceType.IT_INVALID;
			}
			return ObjectParameter.ObjectType;
		}
	}

	public override object ValueDataAsObject
	{
		get
		{
			return m_dataObject;
		}
		set
		{
			if (value != null && value is ObjectValueInfo objectValueInfo)
			{
				ObjectName = objectValueInfo.name;
				ObjectType = objectValueInfo.type;
				m_dataObject = new ObjectValueInfo(CivTechRegistry.CivTechService, ObjectName, ObjectType, Class, ValidTypes, ValidClassNames);
			}
			else if (value is string)
			{
				ObjectName = (string)value;
				m_dataObject = new ObjectValueInfo(CivTechRegistry.CivTechService, ObjectName, ObjectType, Class, ValidTypes, ValidClassNames);
			}
		}
	}

	public virtual IEnumerable<string> ValidClassNames => ObjectParameter?.AllowedClasses;

	public virtual IEnumerable<InstanceType> ValidTypes
	{
		get
		{
			if (Enum.TryParse<InstanceType>(ObjectType, out var result))
			{
				return new InstanceType[1] { result };
			}
			return Enumerable.Empty<InstanceType>();
		}
	}

	public IEntityFilteringContext EntityFilteringContext
	{
		get
		{
			DomNode domNode = base.DomNode.Ancestry.FirstOrDefault((DomNode node) => node.Type == EntitySchema.CookParametersSetType.Type);
			if (domNode != null)
			{
				CookParameterSetAdapter cookParameterSetAdapter = domNode.As<CookParameterSetAdapter>();
				if (cookParameterSetAdapter != null)
				{
					IEntityFilteringContext entityFilteringContext = cookParameterSetAdapter.EntityFilteringContext;
					SetClassFilter(entityFilteringContext);
					SetTypeFilter(entityFilteringContext);
					IProjectFilterDefinition defaultProjectFilter = CivTechRegistry.EntityFilteringService.GetDefaultProjectFilter();
					if (defaultProjectFilter != null)
					{
						entityFilteringContext.RemoveFilterDefinition<IProjectFilterDefinition>();
						entityFilteringContext.FilterDefinitions.Add(defaultProjectFilter);
					}
					return entityFilteringContext;
				}
			}
			return CivTechRegistry.EntityFilteringService.GetFilteringContext(ValidTypes, ValidClassNames);
		}
	}

	public override IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors => m_descriptors;

	private void SetClassFilter(IEntityFilteringContext context)
	{
		if (ValidClassNames == null || !ValidClassNames.Any())
		{
			context.RemoveFilterDefinition<IClassFilterDefinition>();
			return;
		}
		IClassFilterDefinition orCreateFilterDefinition = context.GetOrCreateFilterDefinition<IClassFilterDefinition>(CivTechRegistry.EntityFilteringService);
		orCreateFilterDefinition.ValidClassNames.Clear();
		foreach (string validClassName in ValidClassNames)
		{
			orCreateFilterDefinition.ValidClassNames.Add(validClassName);
		}
	}

	private void SetTypeFilter(IEntityFilteringContext context)
	{
		if (ValidTypes == null || !ValidTypes.Any())
		{
			context.RemoveFilterDefinition<IInstanceTypeFilterDefinition>();
			return;
		}
		IInstanceTypeFilterDefinition orCreateFilterDefinition = context.GetOrCreateFilterDefinition<IInstanceTypeFilterDefinition>(CivTechRegistry.EntityFilteringService);
		orCreateFilterDefinition.ValidInstanceTypes.Clear();
		foreach (InstanceType validType in ValidTypes)
		{
			orCreateFilterDefinition.ValidInstanceTypes.Add(validType);
		}
	}

	public override void InitializeEditor(Func<bool> readOnlyFunctor)
	{
		base.InitializeEditor(readOnlyFunctor);
		m_descriptors = new List<System.ComponentModel.PropertyDescriptor>(new System.ComponentModel.PropertyDescriptor[1] { CreateProxyPropertyDescriptorIfNeeded(new AttributeFieldWithLabelPropertyDescriptor("Parameter".Localize(), FieldSchema.FieldValueType.NameAttribute, FieldSchema.ObjectFieldValueType.ObjectNameAttribute, "Value".Localize(), "Value of the parameter.".Localize(), readOnlyFunctor, new ObjectCookParameterEditor(FiraxisATFRegistry.AssetBrowserDialogService, FiraxisATFRegistry.FileDialogService, FiraxisATFRegistry.AssetBrowserFileCommands, FiraxisATFRegistry.ImportService, CivTechRegistry.CivTechService, FiraxisATFRegistry.DocumentClients, FiraxisATFRegistry.RegistryMediator, FiraxisATFRegistry.BatchEntitySourceControl, FiraxisATFRegistry.AssetPreviewerService)), Name) });
	}

	public override void AddNativeField(IValueSet valSet, IParameter valParam)
	{
		base.AddNativeField(valSet, valParam);
		UpdateNativeValue(ObjectName, ObjectType, DefaultValue.GetBoundObjectType());
	}

	public string AssignDefaultMaterial(IObjectParameter objParam, string triangleGroupName, string stateName, IEnumerable<string> materialNames, IInstanceSet instances, string gamePantry)
	{
		if (objParam.ObjectType != InstanceType.IT_MATERIAL)
		{
			return null;
		}
		PlatformAssert.If(base.Value.ParameterName != objParam.Name, "Value parameter and parameter passed in do not match.");
		string text = ObjectName;
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		if (objParam.Name.Equals("BurnMaterial"))
		{
			text = ((!stateName.Equals("Ruined") && !stateName.Equals("Pillaged")) ? string.Empty : "DefaultBurnMaterial");
		}
		else
		{
			bool flag = materialNames?.Any() ?? false;
			IObjectValue objectValue = objParam.DefaultValue as IObjectValue;
			IMaterialInstance materialInstance = null;
			string text2 = (string.IsNullOrEmpty(stateName) ? triangleGroupName : (triangleGroupName + "_" + stateName));
			if (!string.IsNullOrEmpty(objectValue.GetBoundObjectName()))
			{
				text = objectValue.GetBoundObjectName();
			}
			else if (flag)
			{
				if (materialNames.Contains(text2))
				{
					text = text2;
				}
				else if (materialNames.Contains(triangleGroupName))
				{
					text = triangleGroupName;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				materialInstance = instances.LoadEntityIfUnique<IMaterialInstance>(text2);
				if (materialInstance == null)
				{
					materialInstance = instances.LoadEntityIfUnique<IMaterialInstance>(triangleGroupName);
				}
				if (materialInstance != null)
				{
					if (objParam.AllowedClasses.Contains(materialInstance.ClassName))
					{
						text = materialInstance.Name;
					}
				}
				else if (flag)
				{
					foreach (string materialName in materialNames)
					{
						materialInstance = instances.LoadEntityIfUnique<IMaterialInstance>(materialName);
						if (materialInstance != null && objParam.AllowedClasses.Contains(materialInstance.ClassName))
						{
							text = materialInstance.Name;
							break;
						}
					}
				}
			}
			if (materialInstance == null && !string.IsNullOrEmpty(text))
			{
				materialInstance = instances.LoadEntityIfUnique<IMaterialInstance>(text);
				if (materialInstance == null || !objParam.AllowedClasses.Contains(materialInstance.ClassName))
				{
					text = string.Empty;
				}
			}
		}
		ObjectName = text;
		ObjectType = objParam.ObjectType.ToString();
		return text;
	}

	public override void AssignDefaultValue()
	{
		UpdateParameterInformation(ObjectParameter);
		ObjectType = DefaultValue.GetBoundObjectType().ToString();
		ObjectName = DefaultValue.GetBoundObjectName();
	}

	public override bool RequiresUpdate(IValue val)
	{
		bool num = base.RequiresUpdate(val);
		bool flag = false;
		if (!num && val is IObjectValue objectValue)
		{
			flag = objectValue.GetBoundObjectName() == ObjectName && objectValue.GetBoundObjectType().ToString() == ObjectType;
		}
		if (!num)
		{
			return !flag;
		}
		return true;
	}

	public override void UpdateDomFromNative(IValue value)
	{
		base.UpdateDomFromNative(value);
		base.DomNode.AttributeChanged -= HandleDomNodeAttributeChanged;
		if (value is IObjectValue objectValue)
		{
			bool flag = false;
			if (ObjectName != objectValue.GetBoundObjectName())
			{
				ObjectName = objectValue.GetBoundObjectName();
				flag = true;
			}
			if (ObjectType != objectValue.GetBoundObjectType().ToString())
			{
				ObjectType = objectValue.GetBoundObjectType().ToString();
				flag = true;
			}
			if (flag)
			{
				m_dataObject = new ObjectValueInfo(CivTechRegistry.CivTechService, ObjectName, ObjectType, Class, ValidTypes, ValidClassNames);
			}
		}
		base.DomNode.AttributeChanged += HandleDomNodeAttributeChanged;
	}

	public override void UpdateNativeFromDom()
	{
		UpdateNativeValue(ObjectName, ObjectType, DefaultValue.GetBoundObjectType());
	}

	public override void CopyValue(IValue val)
	{
		base.CopyValue(val);
		IObjectValue objectValue = (IObjectValue)val;
		ObjectName = objectValue.GetBoundObjectName();
		ObjectType = objectValue.GetBoundObjectType().ToString();
		m_dataObject = new ObjectValueInfo(CivTechRegistry.CivTechService, ObjectName, ObjectType, Class, ValidTypes, ValidClassNames);
	}

	public void UpdateParameterInformation(IObjectParameter param)
	{
		base.DomNode.AttributeChanged -= HandleDomNodeAttributeChanged;
		Class = param.AllowedClasses.FirstOrDefault();
		base.DomNode.AttributeChanged += HandleDomNodeAttributeChanged;
	}

	protected virtual void HandleDomNodeAttributeChanged(object sender, AttributeEventArgs e)
	{
		if ((e.AttributeInfo == FieldSchema.ObjectFieldValueType.ObjectNameAttribute || e.AttributeInfo == FieldSchema.ObjectFieldValueType.ObjectTypeAttribute) && !string.IsNullOrEmpty(ObjectType))
		{
			InstanceType result = InstanceType.IT_COUNT;
			if (Enum.TryParse<InstanceType>(ObjectType, out result))
			{
				((IObjectValue)base.Value).BindObject(ObjectName, result);
				m_dataObject = new ObjectValueInfo(CivTechRegistry.CivTechService, ObjectName, ObjectType, Class, ValidTypes, ValidClassNames);
			}
			else
			{
				Outputs.WriteLine(OutputMessageType.Error, "Unrecognized object type \"{0}\"", ObjectType);
			}
		}
	}

	protected override void OnNodeSet()
	{
		base.DomNode.AttributeChanged += HandleDomNodeAttributeChanged;
		base.OnNodeSet();
	}

	private void UpdateNativeValue(string objName, string objTypeStr, InstanceType defInstType)
	{
		InstanceType result = defInstType;
		Enum.TryParse<InstanceType>(objTypeStr, out result);
		(base.Value as IObjectValue).BindObject(objName, result);
	}
}
