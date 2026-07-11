using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Xml.Schema;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(FieldSchemaLoaderGUI))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class FieldSchemaLoaderGUI : XmlSchemaTypeLoader, IInitializable
{
	private string m_namespace;

	private XmlSchemaTypeCollection m_typeCollection;

	public string NameSpace => m_namespace;

	public XmlSchemaTypeCollection TypeCollection => m_typeCollection;

	[ImportingConstructor]
	public FieldSchemaLoaderGUI(FieldSchemaLoader baseFieldLoader)
	{
	}

	void IInitializable.Initialize()
	{
		base.SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(), "Firaxis.AssetEditing/Fields");
		Load("FieldTypes.xsd");
	}

	protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
	{
		using IEnumerator<XmlSchemaTypeCollection> enumerator = GetTypeCollections().GetEnumerator();
		if (enumerator.MoveNext())
		{
			XmlSchemaTypeCollection current = enumerator.Current;
			m_namespace = current.TargetNamespace;
			m_typeCollection = current;
			DefineCurveGUI();
			DefineCollectionGUI();
		}
	}

	private EmbeddedCollectionEditor.ItemInserter BuildCollectionValueInserter(DomNodeType nodeType, PropertyEditorControlContext context, DynamicFieldPropertyDescriptorBase childDescriptor)
	{
		DomNode domNode = context.LastSelectedObject.As<DomNode>();
		domNode.GetRoot().As<TransactionContext>();
		ArtDefElementAdapter adapter = domNode.As<ArtDefElementAdapter>();
		Func<object> insertItemFunc = delegate
		{
			if (adapter.GetAdapterByName(childDescriptor.Name) is CollectionFieldValueAdapter collectionFieldValueAdapter)
			{
				collectionFieldValueAdapter.AddValue(-1);
			}
			return EmptyArray<EmbeddedCollectionEditor.ItemInserter>.Instance;
		};
		return new EmbeddedCollectionEditor.ItemInserter(nodeType.Name, insertItemFunc);
	}

	private DomNodeType GetCollectionAdapterChildDomNodeType(PropertyEditorControlContext context, DynamicFieldPropertyDescriptorBase childDescriptor)
	{
		IEnumerable<object> source = context.GetValue().As<IEnumerable<object>>();
		DomNodeType result = null;
		context.LastSelectedObject.As<DomNode>().GetRoot().As<TransactionContext>();
		ArtDefElementAdapter artDefElementAdapter = context.LastSelectedObject.As<ArtDefElementAdapter>();
		if (source.Any())
		{
			result = source.First().As<DomNode>().Type;
		}
		else if (artDefElementAdapter.Fields.FirstOrDefault((IFieldValueAdapter fva) => fva.Name == childDescriptor.Name) is CollectionFieldValueAdapter collectionFieldValueAdapter)
		{
			result = FieldValueHelper.GetFieldDomNodeType((collectionFieldValueAdapter.Parameter as ICollectionParameter).EntryValueType);
		}
		return result;
	}

	private void DefineCurveGUI()
	{
		EmbeddedCollectionEditor embeddedCollectionEditor = new EmbeddedCollectionEditor();
		embeddedCollectionEditor.ShowCollectionToolbar = false;
		embeddedCollectionEditor.ShowItemLabels = false;
		embeddedCollectionEditor.ItemLabelsType = EmbeddedCollectionEditor.ItemLabelType.kNone;
		DomNodeType type = FieldSchema.CurveSegmentDefinitionType.Type;
		System.ComponentModel.PropertyDescriptor[] array = new Sce.Atf.Dom.PropertyDescriptor[2]
		{
			new AttributeFieldPropertyDescriptor("Start".Localize(), FieldSchema.CurveSegmentDefinitionType.StartingPointAttribute, "Value".Localize(), "Start".Localize(), isReadOnly: false, new NumericEditor(typeof(float))),
			new ChildPropertyDescriptor("Curve".Localize(), FieldSchema.CurveSegmentDefinitionType.CurveSegmentChild, "Value".Localize(), "Curve Data".Localize(), isReadOnly: true, embeddedCollectionEditor, null)
		};
		System.ComponentModel.PropertyDescriptor[] properties = array;
		type.SetTag(new PropertyDescriptorCollection(properties));
		DomNodeType type2 = FieldSchema.ConstantCurveSegmentType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[1]
		{
			new AttributeFieldPropertyDescriptor("Const Value".Localize(), FieldSchema.ConstantCurveSegmentType.ConstantValueAttribute, "Value".Localize(), "Const Value".Localize(), isReadOnly: false, new NumericEditor(typeof(float)))
		};
		properties = array;
		type2.SetTag(new PropertyDescriptorCollection(properties));
		DomNodeType type3 = FieldSchema.LinearCurveSegmentType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[2]
		{
			new AttributeFieldPropertyDescriptor("First Value".Localize(), FieldSchema.LinearCurveSegmentType.FirstValueAttribute, "Value".Localize(), "First Value".Localize(), isReadOnly: false, new NumericEditor(typeof(float))),
			new AttributeFieldPropertyDescriptor("Last Value".Localize(), FieldSchema.LinearCurveSegmentType.LastValueAttribute, "Value".Localize(), "Last Value".Localize(), isReadOnly: false, new NumericEditor(typeof(float)))
		};
		properties = array;
		type3.SetTag(new PropertyDescriptorCollection(properties));
	}

	private void DefineCollectionGUI()
	{
		EmbeddedCollectionEditor embeddedCollectionEditor = new EmbeddedCollectionEditor();
		embeddedCollectionEditor.ShowItemLabels = false;
		embeddedCollectionEditor.GetItemInsertersFunc = delegate(PropertyEditorControlContext context)
		{
			if (!(context.GetValue() is IList<DomNode>))
			{
				return EmptyArray<EmbeddedCollectionEditor.ItemInserter>.Instance;
			}
			DynamicFieldPropertyDescriptorBase dynamicFieldPropertyDescriptorBase = context.Descriptor.As<DynamicFieldPropertyDescriptorBase>();
			if (dynamicFieldPropertyDescriptorBase != null)
			{
				EmbeddedCollectionEditor.ItemInserter[] array2 = new EmbeddedCollectionEditor.ItemInserter[1];
				DomNodeType collectionAdapterChildDomNodeType = GetCollectionAdapterChildDomNodeType(context, dynamicFieldPropertyDescriptorBase);
				EmbeddedCollectionEditor.ItemInserter itemInserter = BuildCollectionValueInserter(collectionAdapterChildDomNodeType, context, dynamicFieldPropertyDescriptorBase);
				array2[0] = itemInserter;
				return array2;
			}
			return EmptyArray<EmbeddedCollectionEditor.ItemInserter>.Instance;
		};
		embeddedCollectionEditor.RemoveItemFunc = delegate(PropertyEditorControlContext context, object item)
		{
			DynamicFieldPropertyDescriptorBase dynamicFieldPropertyDescriptorBase = context.Descriptor.As<DynamicFieldPropertyDescriptorBase>();
			if (dynamicFieldPropertyDescriptorBase != null)
			{
				context.LastSelectedObject.As<DomNode>();
				if (context.LastSelectedObject.As<ArtDefElementAdapter>().GetAdapterByName(dynamicFieldPropertyDescriptorBase.Name) is CollectionFieldValueAdapter collectionFieldValueAdapter)
				{
					IFieldValueAdapter item2 = item.Cast<DomNode>().As<IFieldValueAdapter>();
					int index = collectionFieldValueAdapter.Values.IndexOf(item2);
					collectionFieldValueAdapter.RemoveValue(index);
				}
			}
		};
		DomNodeType type = FieldSchema.CollectionFieldValueType.Type;
		System.ComponentModel.PropertyDescriptor[] array = new Sce.Atf.Dom.PropertyDescriptor[1]
		{
			new ChildFieldPropertyDescriptor("Collection".Localize(), FieldSchema.CollectionFieldValueType.ValueChild, "Collection".Localize(), "Values in this collection".Localize(), isReadOnly: false, embeddedCollectionEditor, null)
		};
		System.ComponentModel.PropertyDescriptor[] properties = array;
		type.SetTag(new PropertyDescriptorCollection(properties));
		EmbeddedCollectionEditor embeddedCollectionEditor2 = new EmbeddedCollectionEditor();
		embeddedCollectionEditor2.ShowCollectionToolbar = false;
		embeddedCollectionEditor2.ShowItemLabels = true;
		embeddedCollectionEditor2.ItemLabelsType = EmbeddedCollectionEditor.ItemLabelType.kName;
		DomNodeType type2 = FieldSchema.TupleFieldValueType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[1]
		{
			new ChildFieldPropertyDescriptor("Tuple".Localize(), FieldSchema.TupleFieldValueType.ValueChild, "Tuple".Localize(), "Values in this tuple".Localize(), isReadOnly: false, embeddedCollectionEditor2, null)
		};
		properties = array;
		type2.SetTag(new PropertyDescriptorCollection(properties));
	}
}
