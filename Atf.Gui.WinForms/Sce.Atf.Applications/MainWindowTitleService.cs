using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(MainWindowTitleService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class MainWindowTitleService : IInitializable
{
	private readonly IMainWindow m_mainWindow;

	private readonly IDocumentRegistry m_documentRegistry;

	protected IMainWindow MainWindow => m_mainWindow;

	protected IDocumentRegistry DocumentRegistry => m_documentRegistry;

	[ImportingConstructor]
	public MainWindowTitleService(IMainWindow mainWindow, IDocumentRegistry documentRegistry)
	{
		m_mainWindow = mainWindow;
		m_documentRegistry = documentRegistry;
		m_documentRegistry.ActiveDocumentChanging += documentRegistry_ActiveDocumentChanging;
		m_documentRegistry.ActiveDocumentChanged += documentRegistry_ActiveDocumentChanged;
	}

	void IInitializable.Initialize()
	{
	}

	private void documentRegistry_ActiveDocumentChanging(object sender, EventArgs e)
	{
		IDocument activeDocument = m_documentRegistry.ActiveDocument;
		if (activeDocument != null)
		{
			activeDocument.UriChanged -= ActiveDocument_UriChanged;
			activeDocument.DirtyChanged -= ActiveDocument_DirtyChanged;
		}
	}

	private void documentRegistry_ActiveDocumentChanged(object sender, EventArgs e)
	{
		IDocument activeDocument = m_documentRegistry.ActiveDocument;
		if (activeDocument != null)
		{
			activeDocument.UriChanged += ActiveDocument_UriChanged;
			activeDocument.DirtyChanged += ActiveDocument_DirtyChanged;
		}
		UpdateMainWindow(m_mainWindow, m_documentRegistry.ActiveDocument);
	}

	private void ActiveDocument_UriChanged(object sender, UriChangedEventArgs e)
	{
		UpdateMainWindow(m_mainWindow, m_documentRegistry.ActiveDocument);
	}

	private void ActiveDocument_DirtyChanged(object sender, EventArgs e)
	{
		UpdateMainWindow(m_mainWindow, m_documentRegistry.ActiveDocument);
	}

	protected virtual void UpdateMainWindow(IMainWindow mainWindow, IDocument activeDocument)
	{
		string text = Application.ProductName + " - ";
		IDocument activeDocument2 = m_documentRegistry.ActiveDocument;
		if (activeDocument2 != null && activeDocument2.Uri != null)
		{
			Uri uri = activeDocument2.Uri;
			text = ((!uri.IsFile) ? (text + activeDocument2.Uri.ToString()) : (text + activeDocument2.Uri.LocalPath));
			if (activeDocument2.Dirty)
			{
				text += "*";
			}
			if (activeDocument2.IsReadOnly)
			{
				text += "(Read Only)";
			}
		}
		mainWindow.Text = text;
	}
}
