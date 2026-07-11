using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Xml;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(IDocumentClient))]
[Export(typeof(AssetEditor))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class AssetEditor : BaseEntityEditor
{
	public static DocumentClientInfo DocumentClientInfo = new EntityDocumentClientInfo("Asset".Localize(), new string[1] { ".ast" }, new InstanceType[1], Sce.Atf.Resources.DocumentImage, Sce.Atf.Resources.FolderImage, "NewAsset", multiDocument: true, Resources.AssetFileIcon);

	private Dictionary<string, string> m_ActiveAssetEditorLayouts = new Dictionary<string, string>();

	private readonly Dictionary<string, string> m_appliedAssetEditorLayouts = new Dictionary<string, string>();

	[Import(AllowDefault = true)]
	private IAssetBrowserDialogService AssetBrowserService { get; set; }

	[Import(AllowDefault = true)]
	private AssetBrowserFileCommands AssetBrowserFileCommands { get; set; }

	public override DocumentClientInfo Info => DocumentClientInfo;

	public string AssetEditorlayoutState { get; set; }

	public Dictionary<string, string> ActiveAssetEditorLayouts
	{
		get
		{
			string.IsNullOrEmpty(base.EditorLayoutState);
			return m_ActiveAssetEditorLayouts;
		}
		set
		{
			m_ActiveAssetEditorLayouts = value;
		}
	}

	protected override DomNodeType DomNodeEntityType => EntitySchema.AssetType.Type;

	protected override ChildInfo DomNodeRootElement => EntitySchema.AssetEntityRootElement;

	[ImportingConstructor]
	public AssetEditor(IContextRegistry contextRegistry, EntitySchemaLoader importedEntitySchemaLoader, IDocumentRegistryMediator documentRegistryMediator)
		: base(contextRegistry, importedEntitySchemaLoader, documentRegistryMediator)
	{
	}

	private void AssetBrowserFileCommands_DocumentClosing(object sender, DocumentClosingEventArgs e)
	{
		if (e.Document is AssetDocument assetDocument)
		{
			AssetEditorControl assetEditorControl = assetDocument.As<AssetContext>().GUI as AssetEditorControl;
			if (!ActiveAssetEditorLayouts.ContainsKey(assetDocument.EntityAdapter.ClassName))
			{
				ActiveAssetEditorLayouts.Add(assetDocument.EntityAdapter.ClassName, assetEditorControl.EditorLayoutState);
				return;
			}
			ActiveAssetEditorLayouts[assetDocument.EntityAdapter.ClassName] = assetEditorControl.EditorLayoutState;
			UpdateLayoutString();
		}
	}

	private void AssetBrowserFilecommands_DocumentOpened(object sender, DocumentEventArgs e)
	{
		if (!(e.Document is AssetDocument assetDocument))
		{
			return;
		}
		AssetEditorControl assetEditorControl = assetDocument.As<AssetContext>().GUI as AssetEditorControl;
		if (ActiveAssetEditorLayouts.Count == 0 && !string.IsNullOrEmpty(base.EditorLayoutState))
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(base.EditorLayoutState);
			foreach (XmlNode item in xmlDocument.DocumentElement.GetElementsByTagName("layout"))
			{
				dictionary.Add(item.Attributes["entityclass"].Value, item.InnerXml);
			}
			ActiveAssetEditorLayouts = dictionary;
		}
		if (ActiveAssetEditorLayouts.ContainsKey(assetDocument.EntityAdapter.ClassName))
		{
			string className = assetDocument.EntityAdapter.ClassName;
			PaintTimingLog.Write("AssetEditor: skip saved editor layout class=" + className);
		}
		else
		{
			ActiveAssetEditorLayouts.Add(assetDocument.EntityAdapter.ClassName, assetEditorControl.EditorLayoutState);
		}
	}

	private void UpdateLayoutString()
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
		XmlElement xmlElement = xmlDocument.CreateElement("ActiveAssetEditorLayouts");
		xmlDocument.AppendChild(xmlElement);
		foreach (KeyValuePair<string, string> activeAssetEditorLayout in ActiveAssetEditorLayouts)
		{
			if (activeAssetEditorLayout.Key == null)
			{
				continue;
			}
			XmlElement xmlElement2 = xmlDocument.CreateElement("layout");
			xmlElement.PrependChild(xmlElement2);
			XmlDocument xmlDocument2 = new XmlDocument();
			xmlDocument2.LoadXml(activeAssetEditorLayout.Value);
			XmlNode node = null;
			foreach (XmlNode childNode in xmlDocument2.ChildNodes)
			{
				if (childNode.Name == "DockPanel")
				{
					node = childNode;
					break;
				}
			}
			xmlElement2.SetAttribute("entityclass", activeAssetEditorLayout.Key);
			XmlNode newChild = xmlElement2.OwnerDocument.ImportNode(node, deep: true);
			xmlElement2.AppendChild(newChild);
		}
		string innerXml = xmlDocument.InnerXml;
		base.EditorLayoutState = innerXml;
	}

	protected override string GetEditorName()
	{
		return "assetInstanceEditor";
	}

	public override void Initialize()
	{
		base.Initialize();
		if (AssetBrowserFileCommands != null)
		{
			AssetBrowserFileCommands.DocumentClosing += AssetBrowserFileCommands_DocumentClosing;
			AssetBrowserFileCommands.DocumentOpened += AssetBrowserFilecommands_DocumentOpened;
		}
		if (base.SettingsService != null)
		{
			BoundPropertyDescriptor boundPropertyDescriptor = new BoundPropertyDescriptor(this, () => AssetEditorlayoutState, "AssetEditorlayoutState", null, null);
			base.SettingsService.RegisterSettings(this, boundPropertyDescriptor);
		}
	}

	protected override BaseEntityPropertyContext InitializeContext(DomNode node, BaseInstanceEntityDocument document, ControlInfo controlInfo)
	{
		AssetContext assetContext = node.As<AssetContext>();
		assetContext.AssetBrowserService = AssetBrowserService;
		assetContext.AssetEditor = this;
		return base.InitializeContext(node, document, controlInfo);
	}

	protected override BaseInstanceEntityDocument InitializeDocument(DomNode node, Uri uri, IInstanceEntity entity, IInstanceSet instances)
	{
		node.As<AssetDocument>().DocumentService = base.DocumentService;
		UpdateLibraryName(entity as IAssetInstance);
		return base.InitializeDocument(node, uri, entity, instances);
	}

	private void UpdateLibraryName(IAssetInstance asset)
	{
		Action<IValue> action = delegate(IValue value)
		{
			if (value is IBLPEntryValue iBLPEntryValue)
			{
				iBLPEntryValue.LibraryName = iBLPEntryValue.XLPClass;
			}
		};
		foreach (IAttachmentPoint item in asset.AttachmentPointSet.Items)
		{
			IEnumerable<IBLPEntryValue> enumerable = item.CookParameters.ItemsOfType<IBLPEntryValue>();
			if (!enumerable.Any())
			{
				continue;
			}
			foreach (IBLPEntryValue item2 in enumerable)
			{
				action(item2);
			}
		}
	}
}
