using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Dom;

namespace Sce.Atf.Applications;

[Export(typeof(DomNodePropertySearchService))]
[Export(typeof(IInitializable))]
public class DomNodePropertySearchService : IInitializable, IControlHostClient
{
	private ISearchUI m_searchUI;

	private IReplaceUI m_replaceUI;

	private IResultsUI m_resultsUI;

	protected UserControl m_rootControl;

	protected IContextRegistry m_contextRegistry;

	protected IControlHostService m_controlHostService;

	protected virtual ISearchUI SearchUI
	{
		get
		{
			return m_searchUI;
		}
		private set
		{
			m_searchUI = value;
		}
	}

	protected virtual IReplaceUI ReplaceUI
	{
		get
		{
			return m_replaceUI;
		}
		private set
		{
			m_replaceUI = value;
		}
	}

	protected virtual IResultsUI ResultsUI
	{
		get
		{
			return m_resultsUI;
		}
		private set
		{
			m_resultsUI = value;
		}
	}

	[ImportingConstructor]
	public DomNodePropertySearchService(IContextRegistry contextRegistry, IControlHostService controlHostService)
	{
		m_contextRegistry = contextRegistry;
		m_controlHostService = controlHostService;
		m_rootControl = new UserControl();
		m_rootControl.Name = "Search and Replace";
		m_rootControl.SuspendLayout();
		m_rootControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		DomNodeSearchToolStrip domNodeSearchToolStrip = (DomNodeSearchToolStrip)(SearchUI = new DomNodeSearchToolStrip());
		SearchUI.Control.Dock = DockStyle.None;
		m_rootControl.Controls.Add(SearchUI.Control);
		SearchUI.UIChanged += UIElement_Changed;
		ReplaceUI = new DomNodeReplaceToolStrip
		{
			DomNodeSearchToolStrip = domNodeSearchToolStrip
		};
		ReplaceUI.Control.Dock = DockStyle.None;
		m_rootControl.Controls.Add(ReplaceUI.Control);
		ReplaceUI.UIChanged += UIElement_Changed;
		ResultsUI = new DomNodeSearchResultsListView(m_contextRegistry);
		ResultsUI.Control.Dock = DockStyle.None;
		m_rootControl.Controls.Add(ResultsUI.Control);
		ResultsUI.UIChanged += UIElement_Changed;
		m_rootControl.Layout += controls_Layout;
		m_rootControl.ResumeLayout();
	}

	public void Show()
	{
		m_controlHostService.Show(m_rootControl);
	}

	private void controls_Layout(object sender, LayoutEventArgs e)
	{
		DoLayout();
	}

	protected virtual void DoLayout()
	{
		if (SearchUI != null)
		{
			SearchUI.Control.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			Rectangle bounds = SearchUI.Control.Bounds;
			if (ResultsUI != null)
			{
				Rectangle bounds2 = bounds;
				bounds2.Y += bounds.Height;
				bounds2.Width = m_rootControl.Width - (m_rootControl.Margin.Left + m_rootControl.Margin.Right);
				bounds2.Height = m_rootControl.Height - (m_rootControl.Margin.Top + m_rootControl.Margin.Bottom + bounds.Height + 2);
				ResultsUI.Control.Bounds = bounds2;
				ResultsUI.Control.Anchor = AnchorStyles.None;
			}
			if (ReplaceUI != null)
			{
				Rectangle bounds3 = bounds;
				bounds3.X += bounds.Width;
				ReplaceUI.Control.Bounds = bounds3;
				ReplaceUI.Control.Anchor = AnchorStyles.None;
			}
		}
	}

	void IInitializable.Initialize()
	{
		Initialize();
	}

	protected virtual void Initialize()
	{
		m_controlHostService.RegisterControl(m_rootControl, "Search and Replace".Localize(), "Search for elements managed within the currently selected subwindow, and optionally replace their values".Localize(), StandardControlGroup.Left, this);
		m_contextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;
	}

	public void UIElement_Changed(object sender, EventArgs e)
	{
		DoLayout();
	}

	protected virtual void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
	{
		SearchUI.Bind(m_contextRegistry.GetActiveContext<IQueryableContext>());
		ResultsUI.Bind(m_contextRegistry.GetActiveContext<IQueryableResultContext>());
		ReplaceUI.Bind(m_contextRegistry.GetActiveContext<IQueryableReplaceContext>());
	}

	public virtual void Activate(Control control)
	{
	}

	public virtual void Deactivate(Control control)
	{
	}

	public virtual bool Close(Control control)
	{
		return true;
	}
}
