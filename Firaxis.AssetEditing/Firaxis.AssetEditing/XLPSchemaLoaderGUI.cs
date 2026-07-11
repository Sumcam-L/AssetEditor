using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Xml.Schema;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.Packages;
using Sce.Atf;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(XLPSchemaLoaderGUI))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class XLPSchemaLoaderGUI : XmlSchemaTypeLoader
{
	private ICivTechService m_civTechService;

	private IEntityFilteringService m_entityFilteringService;

	private IAssetBrowserDialogService m_fileDialogService;

	private string m_namespace;

	private XmlSchemaTypeCollection m_typeCollection;

	public string NameSpace => m_namespace;

	public XmlSchemaTypeCollection TypeCollection => m_typeCollection;

	[ImportingConstructor]
	public XLPSchemaLoaderGUI(XLPSchemaLoader ensureLoadOrder, IAssetBrowserDialogService fileDialogService, IEntityFilteringService entFiltSvc, ICivTechService civTechSvc)
	{
		m_fileDialogService = fileDialogService;
		m_entityFilteringService = entFiltSvc;
		m_civTechService = civTechSvc;
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
			string[] array = m_civTechService.PrimaryProject.Config.XLPClasses.Items.Select((IXLPClass xlpc) => xlpc.Name).ToArray();
			Array.Sort(array);
			DomNodeType type = XLPSchema.XLPType.Type;
			System.ComponentModel.PropertyDescriptor[] array2 = new Sce.Atf.Dom.PropertyDescriptor[3]
			{
				new AttributePropertyDescriptor("XLP Class".Localize(), XLPSchema.XLPType.ClassNameAttribute, "Basic".Localize(), "Class for this XLP".Localize(), isReadOnly: false, new EnumUITypeEditor(array), new ExclusiveEnumTypeConverter(array)),
				new AttributePropertyDescriptor("Module Name".Localize(), XLPSchema.XLPType.ModuleNameAttribute, "Basic".Localize(), "Module for this XLP".Localize(), isReadOnly: true),
				new AttributePropertyDescriptor("Package Name".Localize(), XLPSchema.XLPType.PackageNameAttribute, "Basic".Localize(), "Package for this XLP".Localize(), isReadOnly: false)
			};
			System.ComponentModel.PropertyDescriptor[] properties = array2;
			type.SetTag(new PropertyDescriptorCollection(properties));
			DomNodeType type2 = XLPSchema.XLPEntryType.Type;
			array2 = new Sce.Atf.Dom.PropertyDescriptor[2]
			{
				new AttributePropertyDescriptor("Entry ID".Localize(), XLPSchema.XLPEntryType.EntryIDAttribute, "Basic".Localize(), "ID for this entry".Localize(), isReadOnly: false),
				new AttributePropertyDescriptor("Entity Name".Localize(), XLPSchema.XLPEntryType.ObjectNameAttribute, "Basic".Localize(), "Entity name for this entry".Localize(), isReadOnly: false, new AssetBrowserNameEditor(m_civTechService, m_entityFilteringService, m_fileDialogService))
			};
			properties = array2;
			type2.SetTag(new PropertyDescriptorCollection(properties));
		}
	}
}
