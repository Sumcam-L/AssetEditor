using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(AutoDocumentService))]
[Export(typeof(IAutoDocumentService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class AutoDocumentService : IInitializable, IPartImportsSatisfiedNotification, IAutoDocumentService
{
	private bool m_useSinglePantryMode = false;

	private readonly IDocumentRegistry m_documentRegistry;

	private readonly IDocumentService m_documentService;

	[Import(AllowDefault = true)]
	private IMainWindow m_mainWindow;

	[Import(AllowDefault = true)]
	private Form m_mainForm;

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService;

	[Import(AllowDefault = true)]
	private CommandLineArgsService m_commandLineArgsService;

	[ImportMany]
	private IEnumerable<Lazy<IDocumentClient>> m_documentClients;

	private IDocumentClient m_masterClient;

	private string m_openDocuments = string.Empty;

	private bool m_autoLoadDocuments = true;

	private bool m_autoNewDocument = true;

	[DefaultValue(true)]
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

	[DefaultValue(true)]
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

	public bool UseSinglePantryMode
	{
		get
		{
			return m_useSinglePantryMode;
		}
		set
		{
			m_useSinglePantryMode = value;
		}
	}

	public event EventHandler AutoDocumentsOpening;

	public event EventHandler AutoDocumentsOpened;

	[ImportingConstructor]
	public AutoDocumentService(IDocumentRegistry documentRegistry, IDocumentService documentService)
	{
		m_documentRegistry = documentRegistry;
		m_documentService = documentService;
		m_documentService.DocumentOpened += AddToAutoDocuments;
		m_documentService.DocumentClosed += RemoveFromAutoDocuments;
	}

	private void AddToAutoDocuments(object sender, DocumentEventArgs e)
	{
		if (e.Document != null)
		{
			string documentString = GetDocumentString(e.Document);
			AutoDocuments += documentString;
			m_settingsService.SaveSettings();
		}
	}

	private void RemoveFromAutoDocuments(object sender, DocumentEventArgs e)
	{
		if (e.Document != null)
		{
			string documentString = GetDocumentString(e.Document);
			AutoDocuments = AutoDocuments.Replace(documentString, string.Empty);
			m_settingsService.SaveSettings();
		}
	}

	private string GetDocumentString(IDocument document)
	{
		return $"{document.Uri}{Path.PathSeparator}";
	}

	void IPartImportsSatisfiedNotification.OnImportsSatisfied()
	{
		if (m_mainWindow == null && m_mainForm != null)
		{
			m_mainWindow = new MainFormAdapter(m_mainForm);
		}
		if (m_mainWindow == null)
		{
			throw new InvalidOperationException("Can't get main window");
		}
		m_mainWindow.Loaded += OpenAutoDocuments;
		m_mainWindow.Loaded += AutoDocumentsLoaded;
		m_mainWindow.Closing += HandleMainWindowClosing;
	}

	private void HandleMainWindowClosing(object sender, CancelEventArgs e)
	{
		m_documentService.DocumentOpened -= AddToAutoDocuments;
		m_documentService.DocumentClosed -= RemoveFromAutoDocuments;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (IDocument document in m_documentRegistry.Documents)
		{
			stringBuilder.Append(GetDocumentString(document));
		}
		AutoDocuments = stringBuilder.ToString();
		m_mainWindow.Closing -= HandleMainWindowClosing;
	}

	void IInitializable.Initialize()
	{
		if (m_settingsService != null)
		{
			BoundPropertyDescriptor boundPropertyDescriptor = new BoundPropertyDescriptor(this, () => AutoLoadDocuments, "Auto-load Documents".Localize(), null, "Load previously open documents on application startup".Localize());
			BoundPropertyDescriptor boundPropertyDescriptor2 = new BoundPropertyDescriptor(this, () => AutoNewDocument, "Auto New Document".Localize("Create a new empty document on application startup"), null, "Create a new empty document on application startup".Localize());
			BoundPropertyDescriptor boundPropertyDescriptor3 = new BoundPropertyDescriptor(this, () => AutoDocuments, "AutoDocuments", null, null);
			m_settingsService.RegisterSettings(this, boundPropertyDescriptor2, boundPropertyDescriptor, boundPropertyDescriptor3);
			m_settingsService.RegisterUserSettings("Documents".Localize(), boundPropertyDescriptor2, boundPropertyDescriptor);
		}
	}

	private void OpenAutoDocuments(object sender, EventArgs e)
	{
		bool flag = m_documentRegistry.ActiveDocument != null;
		if (m_autoLoadDocuments && !flag)
		{
			bool flag2 = true;
			if (m_commandLineArgsService != null && m_commandLineArgsService.Parameters.Count > 0)
			{
				flag2 = false;
			}
			if (flag2)
			{
				this.AutoDocumentsOpening.Raise(this, e);
				string[] array = AutoDocuments.Split(new char[1] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);
				AutoDocuments = string.Empty;
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
		}
		if (m_autoNewDocument && !flag)
		{
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
				m_documentService.OpenNewDocument(documentClient);
			}
		}
		m_mainWindow.Loaded -= OpenAutoDocuments;
	}

	private void AutoDocumentsLoaded(object sender, EventArgs e)
	{
		m_mainWindow.Loaded -= AutoDocumentsLoaded;
		if (m_autoLoadDocuments)
		{
			this.AutoDocumentsOpened.Raise(this, e);
		}
	}
}
