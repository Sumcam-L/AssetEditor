using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Xml.Schema;
using Sce.Atf;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(ArtDefSchemaLoader))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ArtDefSchemaLoader : XmlSchemaTypeLoader
{
	private string m_namespace;

	private XmlSchemaTypeCollection m_typeCollection;

	public string NameSpace => m_namespace;

	public XmlSchemaTypeCollection TypeCollection => m_typeCollection;

	[ImportingConstructor]
	public ArtDefSchemaLoader()
	{
		base.SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(), "Firaxis.AssetEditing/ArtDef");
		Load("ArtDef.xsd");
	}

	protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
	{
		using IEnumerator<XmlSchemaTypeCollection> enumerator = GetTypeCollections().GetEnumerator();
		if (enumerator.MoveNext())
		{
			XmlSchemaTypeCollection current = enumerator.Current;
			m_namespace = current.TargetNamespace;
			m_typeCollection = current;
			ArtDefSchema.Initialize(current);
			ArtDefSchema.ArtDefType.Type.Define(new ExtensionInfo<ArtDefDocument>());
			ArtDefSchema.ArtDefType.Type.Define(new ExtensionInfo<ArtDefContext>());
			ArtDefSchema.ArtDefType.Type.Define(new ExtensionInfo<GlobalHistoryContext>());
			ArtDefSchema.ArtDefType.Type.Define(new ExtensionInfo<ArtDefSetTreeView>());
			ArtDefSchema.ArtDefType.Type.Define(new ExtensionInfo<DomNodeQueryable>());
			ArtDefSchema.ArtDefType.Type.Define(new ExtensionInfo<ArtDefSetAdapter>());
			ArtDefSchema.ArtDefCollectionType.Type.Define(new ExtensionInfo<ArtDefCollectionAdapter>());
			ArtDefSchema.ArtDefElementType.Type.Define(new ExtensionInfo<ArtDefElementAdapter>());
			DomNodeType type = ArtDefSchema.ArtDefElementType.Type;
			System.ComponentModel.PropertyDescriptor[] array = new Sce.Atf.Dom.PropertyDescriptor[1]
			{
				new ArtDefElementAttributePropertyDescriptor("Name".Localize(), ArtDefSchema.ArtDefElementType.NameAttribute, "Name".Localize(), "Name of the element".Localize(), isReadOnly: false)
			};
			System.ComponentModel.PropertyDescriptor[] properties = array;
			type.SetTag(new PropertyDescriptorCollection(properties));
		}
	}
}
