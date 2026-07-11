using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Xml.Schema;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(BaseSchemaLoaderGUI))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class BaseSchemaLoaderGUI : XmlSchemaTypeLoader
{
	private IFileDialogService m_fileDialogService;

	private string m_namespace;

	private XmlSchemaTypeCollection m_typeCollection;

	public string NameSpace => m_namespace;

	public XmlSchemaTypeCollection TypeCollection => m_typeCollection;

	[ImportingConstructor]
	public BaseSchemaLoaderGUI(BaseSchemaLoader baseLoader, IFileDialogService fileDialogService)
	{
		m_fileDialogService = fileDialogService;
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
			DomNodeType type = BaseSchema.DataFileType.Type;
			System.ComponentModel.PropertyDescriptor[] array = new Sce.Atf.Dom.PropertyDescriptor[2]
			{
				new AttributePropertyDescriptor("Relative Path".Localize(), BaseSchema.DataFileType.RelativePathAttribute, "Data Files".Localize(), "Relative path to this data file from its pantry root.".Localize(), isReadOnly: true),
				new AttributePropertyDescriptor("ID".Localize(), BaseSchema.DataFileType.IDAttribute, "Data Files".Localize(), "ID of the data file type.".Localize(), isReadOnly: true)
			};
			System.ComponentModel.PropertyDescriptor[] properties = array;
			type.SetTag(new PropertyDescriptorCollection(properties));
			DomNodeType type2 = BaseSchema.TextElementType.Type;
			array = new Sce.Atf.Dom.PropertyDescriptor[1]
			{
				new AttributePropertyDescriptor("Text".Localize(), BaseSchema.TextElementType.TextAttribute, "Basic".Localize(), "Text associated with this element".Localize(), isReadOnly: false)
			};
			properties = array;
			type2.SetTag(new PropertyDescriptorCollection(properties));
			DomNodeType type3 = BaseSchema.PathValueType.Type;
			array = new Sce.Atf.Dom.PropertyDescriptor[1]
			{
				new AttributePropertyDescriptor("Path".Localize(), BaseSchema.PathValueType.PathAttribute, "Basic".Localize(), "File path.".Localize(), isReadOnly: false, new FileServiceNameEditor(m_fileDialogService))
			};
			properties = array;
			type3.SetTag(new PropertyDescriptorCollection(properties));
		}
	}
}
