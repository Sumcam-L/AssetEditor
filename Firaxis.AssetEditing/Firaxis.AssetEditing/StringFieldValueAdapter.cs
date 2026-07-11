using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class StringFieldValueAdapter : FieldValueAdapter
{
	private IEnumerable<System.ComponentModel.PropertyDescriptor> m_descriptors;

	public override object DefaultDataAsObject => (base.Parameter.DefaultValue as IStringValue).ParameterValue;

	public string ParameterValue
	{
		get
		{
			return GetAttribute<string>(FieldSchema.StringFieldValueType.ValueAttribute);
		}
		set
		{
			SetAttribute(FieldSchema.StringFieldValueType.ValueAttribute, value);
		}
	}

	public override IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors => m_descriptors;

	public override object ValueDataAsObject
	{
		get
		{
			return ParameterValue;
		}
		set
		{
			ParameterValue = (string)value;
		}
	}

	private IEnumParameter EnumParameter => base.Parameter as IEnumParameter;

	private IStringParameter StringParameter => base.Parameter as IStringParameter;

	private IStringValue StringValue => base.Value as IStringValue;

	public override void AddNativeField(IValueSet valSet, IParameter valParam)
	{
		base.AddNativeField(valSet, valParam);
		StringValue.ParameterValue = ParameterValue;
	}

	public override void AssignDefaultValue()
	{
		ParameterValue = (base.Parameter.DefaultValue as IStringValue).ParameterValue;
	}

	public override void InitializeEditor(Func<bool> readOnlyFunctor)
	{
		base.InitializeEditor(readOnlyFunctor);
		if (base.Parameter != null)
		{
			if (base.Parameter.ParameterType == ParameterType.PT_ENUM)
			{
				m_descriptors = new List<System.ComponentModel.PropertyDescriptor>(new System.ComponentModel.PropertyDescriptor[1] { CreateProxyPropertyDescriptorIfNeeded(new AttributeFieldPropertyDescriptor(BindDynamicValueOrDefault(base.Parameter.Name, "Value".Localize()), FieldSchema.StringFieldValueType.ValueAttribute, BindDynamicValueOrDefault(base.Parameter.Category, "Value".Localize()), BindDynamicValueOrDefault(base.Parameter.Description, "Value description".Localize()), readOnlyFunctor, new BoundEnumUITypeEditor(EnumParameter), new BoundEnumTypeConverter(EnumParameter)), Name) });
			}
			else if (base.Parameter.ParameterType == ParameterType.PT_STRING)
			{
				string uIHints = ((IStringParameter)base.Parameter).UIHints;
				if (uIHints.Length == 0)
				{
					m_descriptors = new List<System.ComponentModel.PropertyDescriptor>(new System.ComponentModel.PropertyDescriptor[1] { CreateProxyPropertyDescriptorIfNeeded(new AttributeFieldPropertyDescriptor(BindDynamicValueOrDefault(base.Parameter.Name, "Value".Localize()), FieldSchema.StringFieldValueType.ValueAttribute, BindDynamicValueOrDefault(base.Parameter.Category, "Value".Localize()), BindDynamicValueOrDefault(base.Parameter.Description, "Value description".Localize()), readOnlyFunctor), Name) });
				}
				else
				{
					string[] names = GenerateEnumStrVals(uIHints).ToArray();
					m_descriptors = new List<System.ComponentModel.PropertyDescriptor>(new System.ComponentModel.PropertyDescriptor[1] { CreateProxyPropertyDescriptorIfNeeded(new AttributeFieldPropertyDescriptor(BindDynamicValueOrDefault(base.Parameter.Name, "Value".Localize()), FieldSchema.StringFieldValueType.ValueAttribute, BindDynamicValueOrDefault(base.Parameter.Category, "Value".Localize()), BindDynamicValueOrDefault(base.Parameter.Description, "Value description".Localize()), readOnlyFunctor, new EnumUITypeEditor(names), new ExclusiveEnumTypeConverter(names)), Name) });
				}
			}
		}
		else
		{
			m_descriptors = new List<System.ComponentModel.PropertyDescriptor>(new System.ComponentModel.PropertyDescriptor[1]
			{
				new AttributeFieldPropertyDescriptor("String".Localize(), FieldSchema.StringFieldValueType.ValueAttribute, "Value".Localize(), "String value".Localize(), readOnlyFunctor)
			});
		}
	}

	public override void UpdateDomFromNative(IValue val)
	{
		base.UpdateDomFromNative(val);
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		CopyValue(val);
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	public override void UpdateNativeFromDom()
	{
		StringValue.ParameterValue = ParameterValue;
	}

	public override void CopyValue(IValue val)
	{
		base.CopyValue(val);
		ParameterValue = ((IStringValue)val).ParameterValue;
	}

	protected override System.ComponentModel.PropertyDescriptor[] GetPropertyDescriptors()
	{
		return PropertyDescriptors.AsIEnumerable<System.ComponentModel.PropertyDescriptor>().ToArray();
	}

	protected override void OnNodeSet()
	{
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.OnNodeSet();
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == FieldSchema.StringFieldValueType.ValueAttribute)
		{
			StringValue.ParameterValue = (string)e.NewValue;
		}
	}

	private IFieldValueAdapter FindSourceField(string srcPath)
	{
		string[] pathTree = srcPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Split(Path.AltDirectorySeparatorChar);
		if (pathTree.Length == 0)
		{
			return null;
		}
		DomNode domNode = base.DomNode;
		int idx = 0;
		while (domNode != null && idx < pathTree.Length - 1)
		{
			domNode = ((!(pathTree[idx] == "..")) ? domNode.Children.FirstOrDefault((DomNode dn) => dn.As<INamedAdapter>().Name == pathTree[idx]) : domNode.Parent);
			int num = idx + 1;
			idx = num;
		}
		if (domNode == null)
		{
			return null;
		}
		IFieldValueAdapter fieldValueAdapter = domNode.As<IFieldContainerAdapter>().Fields.FirstOrDefault((IFieldValueAdapter fva) => fva.Name == pathTree.Last());
		if (fieldValueAdapter == null)
		{
			Outputs.WriteLine(OutputMessageType.Error, "ArtDef structure did not match the path \"" + srcPath + "\" used in the template's UIHInt field");
		}
		return fieldValueAdapter;
	}

	private IEnumerable<string> GenerateEnumStrVals(string hint)
	{
		string text = ParseHintType(hint);
		string text2 = ParseSourceType(hint);
		string srcPath = ParseSourcePath(hint);
		string text3 = ParseSourceComponent(hint);
		if (text != "Enumeration" || text2 != "Asset" || text3 != "Attachments")
		{
			yield break;
		}
		IFieldValueAdapter fieldValueAdapter = FindSourceField(srcPath);
		if (fieldValueAdapter == null || !(fieldValueAdapter is BLPEntryFieldValueAdapter bLPEntryFieldValueAdapter))
		{
			yield break;
		}
		ArtDefDocument artDefDoc = base.DomNode.GetRoot().As<ArtDefDocument>();
		if (!(bLPEntryFieldValueAdapter.LoadReferencedEntity(artDefDoc) is IAssetInstance assetInstance))
		{
			yield break;
		}
		IEnumerable<IAttachmentPoint> enumerable = assetInstance.AttachmentPointSet.Items.OrderBy((IAttachmentPoint x) => x.Name);
		foreach (IAttachmentPoint item in enumerable)
		{
			yield return item.Name;
		}
	}

	private string ParseHintType(string hint)
	{
		int num = hint.IndexOf('[');
		if (num < 0)
		{
			return string.Empty;
		}
		return hint.Substring(0, num);
	}

	private string ParseSourceComponent(string hint)
	{
		int num = hint.IndexOf(')');
		if (num < 0)
		{
			return string.Empty;
		}
		num++;
		int num2 = hint.IndexOf(']');
		if (num2 < 0)
		{
			return string.Empty;
		}
		return hint.Substring(num, num2 - num);
	}

	private string ParseSourcePath(string hint)
	{
		int num = hint.IndexOf('(');
		if (num < 0)
		{
			return string.Empty;
		}
		num++;
		int num2 = hint.IndexOf(')');
		if (num2 < 0)
		{
			return string.Empty;
		}
		return hint.Substring(num, num2 - num);
	}

	private string ParseSourceType(string hint)
	{
		int num = hint.IndexOf('[');
		if (num < 0)
		{
			return string.Empty;
		}
		num++;
		int num2 = hint.IndexOf('(');
		if (num2 < 0)
		{
			return string.Empty;
		}
		return hint.Substring(num, num2 - num);
	}
}
