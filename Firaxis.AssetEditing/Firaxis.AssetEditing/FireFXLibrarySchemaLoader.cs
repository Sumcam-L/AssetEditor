using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Xml.Schema;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(FireFXLibrarySchemaLoader))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class FireFXLibrarySchemaLoader : XmlSchemaTypeLoader
{
	private string m_namespace;

	private XmlSchemaTypeCollection m_typeCollection;

	public string NameSpace => m_namespace;

	public XmlSchemaTypeCollection TypeCollection => m_typeCollection;

	[ImportingConstructor]
	public FireFXLibrarySchemaLoader()
	{
		base.SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(), "Firaxis.AssetEditing/FireFX");
		Load("FireFXLibrary.xsd");
	}

	protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
	{
		using IEnumerator<XmlSchemaTypeCollection> enumerator = GetTypeCollections().GetEnumerator();
		if (enumerator.MoveNext())
		{
			XmlSchemaTypeCollection current = enumerator.Current;
			m_namespace = current.TargetNamespace;
			m_typeCollection = current;
			FireFXLibrarySchema.Initialize(current);
			FireFXLibrarySchema.FireFXLibraryType.Type.Define(new ExtensionInfo<FireFXLibraryDocument>());
			FireFXLibrarySchema.FireFXLibraryType.Type.Define(new ExtensionInfo<FireFXLibraryContext>());
			FireFXLibrarySchema.FireFXLibraryType.Type.Define(new ExtensionInfo<DomNodeQueryable>());
			AdapterCreator<CustomTypeDescriptorNodeAdapter> creator = new AdapterCreator<CustomTypeDescriptorNodeAdapter>();
			FireFXLibrarySchema.FireFXLibraryType.Type.AddAdapterCreator(creator);
		}
	}
}
