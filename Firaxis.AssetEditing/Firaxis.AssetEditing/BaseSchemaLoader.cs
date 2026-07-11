using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Xml.Schema;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(BaseSchemaLoader))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class BaseSchemaLoader : XmlSchemaTypeLoader
{
	private string m_namespace;

	private XmlSchemaTypeCollection m_typeCollection;

	public string NameSpace => m_namespace;

	public XmlSchemaTypeCollection TypeCollection => m_typeCollection;

	[ImportingConstructor]
	public BaseSchemaLoader()
	{
		base.SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(), "Firaxis.AssetEditing/Base");
		Load("BaseTypes.xsd");
	}

	protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
	{
		using IEnumerator<XmlSchemaTypeCollection> enumerator = GetTypeCollections().GetEnumerator();
		if (enumerator.MoveNext())
		{
			XmlSchemaTypeCollection current = enumerator.Current;
			m_namespace = current.TargetNamespace;
			m_typeCollection = current;
			BaseSchema.Initialize(current);
			BaseSchema.DataFileType.Type.Define(new ExtensionInfo<DataFileAdapter>());
			BaseSchema.TextElementType.Type.Define(new ExtensionInfo<TextElementAdapter>());
			BaseSchema.PathValueType.Type.Define(new ExtensionInfo<PathValueAdapter>());
			AdapterCreator<CustomTypeDescriptorNodeAdapter> creator = new AdapterCreator<CustomTypeDescriptorNodeAdapter>();
			BaseSchema.TextElementType.Type.AddAdapterCreator(creator);
			BaseSchema.DataFileType.Type.AddAdapterCreator(creator);
			BaseSchema.PathValueType.Type.AddAdapterCreator(creator);
		}
	}
}
