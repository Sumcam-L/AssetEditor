using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Xml.Schema;
using Sce.Atf.Dom;

namespace Sce.Atf.Applications;

[Export(typeof(SkinSchemaLoader))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SkinSchemaLoader : XmlSchemaTypeLoader
{
	private XmlSchemaTypeCollection m_typeCollection;

	public XmlSchemaTypeCollection TypeCollection => m_typeCollection;

	public SkinSchemaLoader()
	{
		base.SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(), "Sce.Atf.Applications.SkinService/Schemas");
		Load("skin.xsd");
	}

	protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
	{
		using IEnumerator<XmlSchemaTypeCollection> enumerator = GetTypeCollections().GetEnumerator();
		if (enumerator.MoveNext())
		{
			SkinSchema.Initialize(m_typeCollection = enumerator.Current);
			SkinSchema.skinType.Type.Define(new ExtensionInfo<SkinDocument>());
			SkinSchema.skinType.Type.Define(new ExtensionInfo<SkinEditingContext>());
			SkinSchema.skinType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());
			SkinSchema.styleType.Type.Define(new ExtensionInfo<SkinStyleProperties>());
		}
	}
}
