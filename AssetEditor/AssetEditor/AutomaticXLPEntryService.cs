using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Firaxis.AssetEditing;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.Packages;
using Firaxis.Controls;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace AssetEditor;

[Export(typeof(AutomaticXLPEntryService))]
[Export(typeof(IInitializable))]
public class AutomaticXLPEntryService : IInitializable, IDisposable
{
	private class LoadedXLPData
	{
		public readonly string RelativePath;

		public readonly string FullPath;

		public readonly IXLP XLP;

		public LoadedXLPData(string relativePath, string fullPath, IXLP xlp)
		{
			RelativePath = relativePath;
			FullPath = fullPath;
			XLP = xlp;
		}
	}

	private bool m_automaticallyAddNewAssetToXLP = true;

	[ImportMany]
	private Lazy<IDocumentClient>[] m_lazyDocumentClients;

	private bool m_scrapePantryToAddAssetToXLP;

	private readonly ICivTechService m_civTechService;

	private readonly List<IDocumentClient> m_documentClients = new List<IDocumentClient>();

	private readonly IDocumentService m_documentService;

	private readonly IDocumentRegistry m_documentRegistry;

	private readonly AssetBrowserFileCommands m_fileCommands;

	private readonly ISettingsService m_settingsService;

	public bool AutomaticallyAddNewAssetToXLP
	{
		get
		{
			return m_automaticallyAddNewAssetToXLP;
		}
		set
		{
			m_automaticallyAddNewAssetToXLP = value;
		}
	}

	public bool ScrapePantryToAddAssetToXLP
	{
		get
		{
			return m_scrapePantryToAddAssetToXLP;
		}
		set
		{
			m_scrapePantryToAddAssetToXLP = value;
		}
	}

	[ImportingConstructor]
	public AutomaticXLPEntryService(IDocumentService documentService, ISettingsService settingsService, IDocumentRegistry documentRegistry, AssetBrowserFileCommands fileCommands, ICivTechService civTechService)
	{
		m_documentService = documentService;
		m_settingsService = settingsService;
		m_documentRegistry = documentRegistry;
		m_fileCommands = fileCommands;
		m_civTechService = civTechService;
	}

	public void Dispose()
	{
		m_documentService.DocumentSaved -= AddEntryToXLP;
	}

	public void Initialize()
	{
		RegisterUserSettings();
		if (m_lazyDocumentClients != null)
		{
			m_documentClients.AddRange(m_lazyDocumentClients.Select((Lazy<IDocumentClient> lazy) => lazy.Value));
		}
		m_documentService.DocumentSaved += AddEntryToXLP;
	}

	private void AddEntryToXLP(object sender, DocumentEventArgs e)
	{
		if (!AutomaticallyAddNewAssetToXLP || !m_documentService.IsUntitled(e.Document) || !(e.Document is IEntityDocument { InstanceEntity: var instanceEntity }))
		{
			return;
		}
		string[] array = (from xlpClass in m_civTechService.PrimaryProject.Config.XLPClasses.GetValidXLPClasses(instanceEntity)
			select xlpClass.Name).ToArray();
		if (array.Length == 0)
		{
			return;
		}
		XLPDocument[] matchingOpenDocuments = GetMatchingOpenDocuments(array);
		XLPDocument xLPDocument = SelectXLPDocument(matchingOpenDocuments, array, instanceEntity.Name);
		if (xLPDocument != null)
		{
			AddEntryToXLPDoc(xLPDocument, instanceEntity.Name);
			if (!(e.Document is IShadowDocument))
			{
				m_fileCommands.OpenExistingDocument(instanceEntity.Type, instanceEntity.Name);
			}
		}
	}

	private void AddEntryToXLPDoc(XLPDocument xlpDoc, string entryID)
	{
		TransactionContext context = xlpDoc.As<TransactionContext>();
		XLPAdapter xlpAdapter = xlpDoc.As<XLPAdapter>();
		if (xlpAdapter.XLP.FindEntry(entryID) == null)
		{
			context.DoTransaction(delegate
			{
				xlpAdapter.AddEntry(entryID, entryID);
			}, "Add Entry");
		}
		Outputs.WriteLine(OutputMessageType.Info, "Added entry {0} to XLP {1}", entryID, xlpDoc.Uri.LocalPath);
	}

	private IEnumerable<LoadedXLPData> ConvertToLoadedData(IEnumerable<XLPDocument> xlpDocuments, string entryID)
	{
		string xLPRoot = m_civTechService.PrimaryProject.Paths.XLPRoot;
		bool flag = false;
		ICollection<LoadedXLPData> collection = new List<LoadedXLPData>();
		foreach (XLPDocument xlpDocument in xlpDocuments)
		{
			IXLP xLP = xlpDocument.XLP;
			if (xLP.FindEntry(entryID) != null)
			{
				flag = true;
				break;
			}
			string localPath = xlpDocument.Uri.LocalPath;
			string relativePath = localPath.Replace(xLPRoot, "").TrimStart(Path.DirectorySeparatorChar);
			collection.Add(new LoadedXLPData(relativePath, localPath, xLP));
		}
		if (flag)
		{
			collection.Clear();
		}
		return collection;
	}

	private ItemSelectorForm CreateSelectionForm(string[] xlpChoices)
	{
		return new ItemSelectorForm(xlpChoices)
		{
			Title = "XLP Selection Form",
			Caption = "Select the XLP to add the asset to."
		};
	}

	private XLPDocument[] GetMatchingOpenDocuments(IEnumerable<string> xlpClassNames)
	{
		return (from xlpDoc in m_documentRegistry.Documents.OfType<XLPDocument>()
			where xlpClassNames.Contains(xlpDoc.XLP.ClassName)
			select xlpDoc).ToArray();
	}

	private ICollection<LoadedXLPData> GetMatchingXLPs(IEnumerable<string> xlpClassNames, string entryID)
	{
		List<LoadedXLPData> list = new List<LoadedXLPData>();
		CivTechContext civTechContext = Context.EnsureCreated<CivTechContext>();
		string xLPRoot = m_civTechService.PrimaryProject.Paths.XLPRoot;
		bool flag = false;
		foreach (string item in Directory.EnumerateFiles(xLPRoot, "*.xlp", SearchOption.AllDirectories))
		{
			IXLP iXLP = civTechContext.CreateInstance<IXLP>();
			if ((bool)iXLP.DeserializeFromFile(item) && xlpClassNames.Contains(iXLP.ClassName))
			{
				string relativePath = item.Replace(xLPRoot, "").TrimStart(Path.DirectorySeparatorChar);
				list.Add(new LoadedXLPData(relativePath, item, iXLP));
				if (iXLP.FindEntry(entryID) != null)
				{
					flag = true;
					break;
				}
			}
			else
			{
				iXLP.Dispose();
			}
		}
		if (flag)
		{
			list.Select((LoadedXLPData pair) => pair.XLP).DisposeXLPs();
			list.Clear();
		}
		return list;
	}

	private void RegisterUserSettings()
	{
		BoundPropertyDescriptor boundPropertyDescriptor = new BoundPropertyDescriptor(this, () => AutomaticallyAddNewAssetToXLP, "Add New Assets to XLP".Localize(), "Automatic XLP Modification".Localize(), "If true, when making a new asset, it will automatically be added to an XLP that is open if the asset class is valid for that XLP.".Localize());
		BoundPropertyDescriptor boundPropertyDescriptor2 = new BoundPropertyDescriptor(this, () => ScrapePantryToAddAssetToXLP, "Scrape Pantry to Add Asset", "Automatic XLP Modification".Localize(), "If true, when making a new asset, the XLP pantry will be scraped to try to find an XLP to add the newly saved asset to.".Localize());
		m_settingsService.RegisterSettings("Application", boundPropertyDescriptor);
		m_settingsService.RegisterUserSettings("Application", boundPropertyDescriptor);
		m_settingsService.RegisterSettings("Application", boundPropertyDescriptor2);
		m_settingsService.RegisterUserSettings("Application", boundPropertyDescriptor2);
	}

	private XLPDocument SelectXLPDocument(XLPDocument[] xlpDocs, IEnumerable<string> validXLPClassNames, string entryID)
	{
		XLPDocument result = null;
		if (xlpDocs.Length != 0)
		{
			result = ((xlpDocs.Length != 1) ? SelectXLPDocument(xlpDocs, entryID) : xlpDocs[0]);
		}
		else if (ScrapePantryToAddAssetToXLP)
		{
			result = SelectXLPDocumentFromPantry(validXLPClassNames, entryID);
		}
		return result;
	}

	private XLPDocument SelectXLPDocument(IEnumerable<XLPDocument> xlpDocs, string entryID)
	{
		IEnumerable<LoadedXLPData> source = ConvertToLoadedData(xlpDocs, entryID);
		if (!source.Any())
		{
			return null;
		}
		string[] xlpChoices = source.Select((LoadedXLPData data) => data.RelativePath).ToArray();
		ItemSelectorForm selectionForm = CreateSelectionForm(xlpChoices);
		if (selectionForm.ShowDialog() == DialogResult.OK)
		{
			LoadedXLPData selectedData = source.First((LoadedXLPData data) => data.RelativePath == selectionForm.SelectedItem);
			return xlpDocs.First((XLPDocument doc) => doc.Uri.LocalPath == selectedData.FullPath);
		}
		return null;
	}

	private XLPDocument SelectXLPDocumentFromPantry(IEnumerable<string> validXLPClassNames, string entryID)
	{
		ICollection<LoadedXLPData> matchingXLPs = GetMatchingXLPs(validXLPClassNames, entryID);
		if (matchingXLPs.Count == 0)
		{
			return null;
		}
		XLPDocument result = null;
		string[] xlpChoices = matchingXLPs.Select((LoadedXLPData xlpData) => xlpData.RelativePath).ToArray();
		ItemSelectorForm selectionForm = CreateSelectionForm(xlpChoices);
		if (selectionForm.ShowDialog() == DialogResult.OK)
		{
			string text = (from xlpData in matchingXLPs
				where xlpData.RelativePath == selectionForm.SelectedItem
				select xlpData.FullPath).First();
			if (Uri.TryCreate(text, UriKind.Absolute, out var result2))
			{
				IDocumentClient firstClientForPath = m_documentClients.GetFirstClientForPath(text);
				result = m_fileCommands.OpenExistingDocument(firstClientForPath, result2) as XLPDocument;
			}
		}
		matchingXLPs.Select((LoadedXLPData pair) => pair.XLP).DisposeXLPs();
		matchingXLPs.Clear();
		return result;
	}
}
