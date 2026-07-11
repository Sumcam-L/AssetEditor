using System;
using System.Collections.Generic;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ProxyPropertyDescriptor : FieldPropertyDescriptorBase, IAdaptable
{
	private FieldPropertyDescriptorBase m_targetDescriptor;

	private readonly string m_targetName;

	public override AttributeInfo AttributeInfo => m_targetDescriptor.AttributeInfo;

	public override Type ClrType => AttributeInfo.Type.ClrType;

	public FieldPropertyDescriptorBase TargetFieldDescriptor => m_targetDescriptor;

	public string TargetFieldName => m_targetName;

	public ProxyPropertyDescriptor(FieldPropertyDescriptorBase fldPropDesc, string fieldName)
		: base(fldPropDesc)
	{
		m_targetDescriptor = fldPropDesc;
		m_targetName = fieldName;
	}

	public ProxyPropertyDescriptor(FieldPropertyDescriptorBase fldPropDesc, string fieldName, object editor)
		: base(fldPropDesc, editor)
	{
		m_targetDescriptor = fldPropDesc;
		m_targetName = fieldName;
	}

	public object GetAdapter(Type type)
	{
		if (type.IsAssignableFrom(GetType()))
		{
			return this;
		}
		if (type.IsAssignableFrom(TargetFieldDescriptor.GetType()))
		{
			return TargetFieldDescriptor;
		}
		return null;
	}

	public override object GetEditor(Type editorBaseType)
	{
		object editor = m_targetDescriptor.GetEditor(editorBaseType);
		if (editor == null)
		{
			editor = base.GetEditor(editorBaseType);
		}
		return editor;
	}

	public override string GetDisplayName(object component)
	{
		DomNode node = GetNode(component);
		if (node != null)
		{
			string text = TargetFieldDescriptor.GetDisplayName(node);
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
			return TargetFieldDescriptor.Name;
		}
		return m_targetName;
	}

	public override string GetCategoryName(object component)
	{
		DomNode node = GetNode(component);
		return ComputeCategoryName(node, !node.Is<IFieldContainerAdapter>());
	}

	protected virtual string ComputeCategoryName(object component, bool stripLeafCategory)
	{
		IList<string> list = new List<string>();
		DomNode domNode = component.As<DomNode>();
		if (domNode == null)
		{
			BugSubmitter.SilentAssert(false, "Failed to determine category for component \"{0}\" @summary Failed to determine category name for non-IFieldValueAdapter node @assign bwhitman", component);
			return Category;
		}
		if (domNode.Parent != null && domNode.Parent.Is<CookParameterSetAdapter>() && !domNode.Is<IFieldContainerAdapter>())
		{
			return Category;
		}
		if (domNode.As<IFieldValueAdapter>() == null)
		{
			bool condition = domNode.Type.Equals(FieldSchema.ArtDefRefValueType.Type);
			string fmtText = "Failed to determine category for node:\n\"{0}\" @summary Failed to determine category name for non-IFieldValueAdapter node @assign bwhitman";
			BugSubmitter.SilentAssert(condition, fmtText, string.Join("\n", from n in domNode.GetPath()
				select "\t" + n.Type.Name));
			return Category;
		}
		string nodeName = GetNodeName(domNode);
		bool condition2 = nodeName != null;
		string fmtText2 = "Failed to determine node category name:\n\"{0}\" @summary Failed to determine node category name for leaf node @assign bwhitman";
		BugSubmitter.SilentAssert(condition2, fmtText2, string.Join("\n", from n in domNode.GetPath()
			select "\t" + n.Type.Name));
		if (!string.IsNullOrEmpty(nodeName))
		{
			list.Add(nodeName);
		}
		string nodeName2;
		do
		{
			domNode = domNode.Parent;
			nodeName2 = GetNodeName(domNode);
			if (!string.IsNullOrEmpty(nodeName2))
			{
				list.Add(nodeName2);
			}
		}
		while (!string.IsNullOrEmpty(nodeName2));
		if (list.Count > 1 && stripLeafCategory)
		{
			list.RemoveAt(0);
		}
		return string.Join("\\", list.Reverse());
	}

	private string GetNodeName(DomNode node)
	{
		if (node == null)
		{
			return null;
		}
		if (node.Parent != null)
		{
			CollectionFieldValueAdapter collectionFieldValueAdapter = node.Parent.As<CollectionFieldValueAdapter>();
			if (collectionFieldValueAdapter != null && !collectionFieldValueAdapter.CollectionParameter.HasNamedEntries)
			{
				IFieldValueAdapter fieldValueAdapter = node.As<IFieldValueAdapter>();
				if (fieldValueAdapter == null)
				{
					return null;
				}
				return (collectionFieldValueAdapter.Fields.IndexOf(fieldValueAdapter) + 1).ToString();
			}
		}
		return node.As<INamedAdapter>()?.Name;
	}

	public override DomNode GetNode(object component)
	{
		IFieldContainerAdapter fieldContainerAdapter = component.As<IFieldContainerAdapter>();
		if (fieldContainerAdapter != null)
		{
			IFieldValueAdapter fieldValueAdapter = fieldContainerAdapter?.Fields?.FirstOrDefault((IFieldValueAdapter fld) => fld.Name.Equals(TargetFieldName));
			if (fieldValueAdapter == null)
			{
				return null;
			}
			return TargetFieldDescriptor.GetNode(fieldValueAdapter.DomNode);
		}
		DomNode domNode = component.As<DomNode>();
		if (domNode == null)
		{
			return null;
		}
		return TargetFieldDescriptor.GetNode(domNode);
	}
}
