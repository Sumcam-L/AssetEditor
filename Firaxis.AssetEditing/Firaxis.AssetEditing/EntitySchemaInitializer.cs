using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Xml.Schema;
using Sce.Atf;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(IEntitySchemaInitializer))]
[Export(typeof(EntitySchemaInitializer))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class EntitySchemaInitializer : XmlSchemaTypeLoader, IEntitySchemaInitializer
{
	private string m_namespace;

	private XmlSchemaTypeCollection m_typeCollection;

	public string NameSpace => m_namespace;

	public XmlSchemaTypeCollection TypeCollection => m_typeCollection;

	[ImportingConstructor]
	public EntitySchemaInitializer()
	{
		base.SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(), "Firaxis.AssetEditing/Entities");
		Load("Entities.xsd");
	}

	protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
	{
		using IEnumerator<XmlSchemaTypeCollection> enumerator = GetTypeCollections().GetEnumerator();
		if (enumerator.MoveNext())
		{
			XmlSchemaTypeCollection current = enumerator.Current;
			m_namespace = current.TargetNamespace;
			m_typeCollection = current;
			EntitySchema.Initialize(current);
		}
	}
}
