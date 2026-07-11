using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Wpf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(AutoDocumentService))]
[ExportMetadata("Order", 100)]
[PartCreationPolicy(CreationPolicy.Shared)]
public class AutoDocumentService : IInitializable
{
	private readonly IDocumentRegistry m_documentRegistry;

	private readonly IDocumentService m_documentService;

	[Import(AllowDefault = true)]
	private IMainWindow m_mainWindow = null;

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService = null;

	[ImportMany]
	private IEnumerable<Lazy<IDocumentClient>> m_documentClients = null;

	private IDocumentClient m_masterClient;

	private IDocument m_autoDocument;

	private string m_openDocuments = string.Empty;

	private bool m_autoLoadDocuments = true;

	private bool m_autoNewDocument = true;

	public bool AutoLoadDocuments
	{
		get
		{
			return m_autoLoadDocuments;
		}
		set
		{
			m_autoLoadDocuments = value;
		}
	}

	public bool AutoNewDocument
	{
		get
		{
			return m_autoNewDocument;
		}
		set
		{
			m_autoNewDocument = value;
		}
	}

	public string AutoDocuments
	{
		get
		{
			return m_openDocuments;
		}
		set
		{
			m_openDocuments = value;
		}
	}

	[ImportingConstructor]
	public AutoDocumentService(IDocumentRegistry documentRegistry, IDocumentService documentService)
	{
		m_documentRegistry = documentRegistry;
		m_documentService = documentService;
	}

	void IInitializable.Initialize()
	{
		if (m_mainWindow != null)
		{
			m_mainWindow.Loaded += mainWindow_Loaded;
			m_mainWindow.Closing += mainWindow_Closing;
		}
		if (m_settingsService != null)
		{
			BoundPropertyDescriptor boundPropertyDescriptor = new BoundPropertyDescriptor(this, () => AutoLoadDocuments, "Auto-load Documents".Localize(), null, "Load previously open documents on application startup".Localize());
			BoundPropertyDescriptor boundPropertyDescriptor2 = new BoundPropertyDescriptor(this, () => AutoNewDocument, "Auto New Document".Localize("Create a new empty document on application startup"), null, "Create a new empty document on application startup".Localize());
			BoundPropertyDescriptor boundPropertyDescriptor3 = new BoundPropertyDescriptor(this, () => AutoDocuments, "AutoDocuments", null, null);
			m_settingsService.RegisterSettings(this, boundPropertyDescriptor2, boundPropertyDescriptor, boundPropertyDescriptor3);
			m_settingsService.RegisterUserSettings("Documents".Localize(), boundPropertyDescriptor2, boundPropertyDescriptor);
		}
	}

	private void mainWindow_Loaded(object sender, EventArgs e)
	{
		bool flag = m_documentRegistry.ActiveDocument != null;
		if (m_autoLoadDocuments && !flag)
		{
			string[] array = m_openDocuments.Split(new char[1] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);
			string[] array2 = array;
			foreach (string uriString in array2)
			{
				if (!Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out var result))
				{
					continue;
				}
				foreach (IDocumentClient value in m_documentClients.GetValues())
				{
					try
					{
						if ((!result.IsAbsoluteUri || File.Exists(result.LocalPath)) && value.CanOpen(result))
						{
							m_documentService.OpenExistingDocument(value, result);
							flag = true;
							break;
						}
					}
					catch
					{
					}
				}
			}
		}
		if (!m_autoNewDocument || flag)
		{
			return;
		}
		IDocumentClient documentClient = null;
		foreach (IDocumentClient value2 in m_documentClients.GetValues())
		{
			if (m_masterClient == null && !value2.Info.MultiDocument)
			{
				m_masterClient = value2;
				documentClient = value2;
			}
			if (documentClient == null)
			{
				documentClient = value2;
			}
		}
		if (documentClient != null)
		{
			m_autoDocument = m_documentService.OpenNewDocument(documentClient);
			if (m_autoDocument != null)
			{
				m_autoDocument.DirtyChanged += autoDocument_DirtyChanged;
			}
		}
	}

	private void mainWindow_Closing(object sender, CancelEventArgs e)
	{
		StringBuilder stringBuilder = new StringBuilder();
		char pathSeparator = Path.PathSeparator;
		foreach (IDocument document in m_documentRegistry.Documents)
		{
			stringBuilder.Append(document.Uri);
			stringBuilder.Append(pathSeparator);
		}
		m_openDocuments = stringBuilder.ToString();
	}

	private void autoDocument_DirtyChanged(object sender, EventArgs e)
	{
		m_autoDocument.DirtyChanged -= autoDocument_DirtyChanged;
		m_autoDocument = null;
	}
}
