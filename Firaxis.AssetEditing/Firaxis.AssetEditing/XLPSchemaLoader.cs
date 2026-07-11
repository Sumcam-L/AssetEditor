using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Xml.Schema;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(XLPSchemaLoader))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class XLPSchemaLoader : XmlSchemaTypeLoader
{
	private string m_namespace;

	private XmlSchemaTypeCollection m_typeCollection;

	public string NameSpace => m_namespace;

	public XmlSchemaTypeCollection TypeCollection => m_typeCollection;

	[ImportingConstructor]
	public XLPSchemaLoader()
	{
		base.SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(), "Firaxis.AssetEditing/XLP");
		Load("XLP.xsd");
	}

	protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
	{
		using IEnumerator<XmlSchemaTypeCollection> enumerator = GetTypeCollections().GetEnumerator();
		if (enumerator.MoveNext())
		{
			XmlSchemaTypeCollection current = enumerator.Current;
			m_namespace = current.TargetNamespace;
			m_typeCollection = current;
			XLPSchema.Initialize(current);
			XLPSchema.XLPType.Type.Define(new ExtensionInfo<XLPDocument>());
			XLPSchema.XLPType.Type.Define(new ExtensionInfo<XLPContext>());
			XLPSchema.XLPType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());
			XLPSchema.XLPType.Type.Define(new ExtensionInfo<ReferenceValidator>());
			XLPSchema.XLPType.Type.Define(new ExtensionInfo<UniqueIdValidator>());
			XLPSchema.XLPType.Type.Define(new ExtensionInfo<DomNodeQueryable>());
			XLPSchema.XLPType.Type.Define(new ExtensionInfo<XLPAdapter>());
			XLPSchema.XLPType.Type.Define(new ExtensionInfo<PlatformSelectorContext>());
			XLPSchema.XLPEntryType.Type.Define(new ExtensionInfo<XLPEntryAdapter>());
			AdapterCreator<CustomTypeDescriptorNodeAdapter> creator = new AdapterCreator<CustomTypeDescriptorNodeAdapter>();
			XLPSchema.XLPEntryType.Type.AddAdapterCreator(creator);
			XLPSchema.XLPType.Type.AddAdapterCreator(creator);
		}
	}
}
