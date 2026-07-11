using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications;

[Export(typeof(GridPropertyEditor))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Any)]
public class GridPropertyEditor : IDisposable, IControlHostClient, IInitializable
{
	private readonly IControlHostService m_controlHostService;

	private readonly IContextRegistry m_contextRegistry;

	private readonly ICommandService m_commandService;

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService;

	[ImportMany]
	private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders;

	private readonly GridControl m_gridControl;

	private readonly ControlInfo m_controlInfo;

	private SelectionPropertyEditingContext m_defaultContext = new SelectionPropertyEditingContext();

	public GridControl GridControl => m_gridControl;

	public ControlInfo ControlInfo => m_controlInfo;

	public SelectionPropertyEditingContext DefaultPropertyEditingContext
	{
		get
		{
			return m_defaultContext;
		}
		set
		{
			m_defaultContext = value;
		}
	}

	[ImportingConstructor]
	public GridPropertyEditor(ICommandService commandService, IControlHostService controlHostService, IContextRegistry contextRegistry)
	{
		m_commandService = commandService;
		m_controlHostService = controlHostService;
		m_contextRegistry = contextRegistry;
		Configure(out m_gridControl, out m_controlInfo);
		m_gridControl.MouseUp += gridControl_MouseUp;
	}

	protected virtual void Configure(out GridControl gridControl, out ControlInfo controlInfo)
	{
		gridControl = new GridControl();
		controlInfo = new ControlInfo("Grid Property Editor".Localize(), "Edits selected object properties".Localize(), StandardControlGroup.Bottom, "https://github.com/SonyWWS/ATF/wiki/Property-Editing-in-ATF".Localize());
	}

	void IInitializable.Initialize()
	{
		m_contextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;
		m_controlHostService.RegisterControl(m_gridControl, m_controlInfo, this);
		if (m_settingsService != null)
		{
			m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(m_gridControl, () => m_gridControl.Settings, "Settings", "", ""));
		}
	}

	public void Dispose()
	{
		m_gridControl.Dispose();
	}

	void IControlHostClient.Activate(Control control)
	{
	}

	void IControlHostClient.Deactivate(Control control)
	{
	}

	bool IControlHostClient.Close(Control control)
	{
		return true;
	}

	private void gridControl_MouseUp(object sender, MouseEventArgs e)
	{
		OnGridControlMouseUp(e);
	}

	protected virtual void OnGridControlMouseUp(MouseEventArgs e)
	{
		if (m_commandService != null)
		{
			Point point = new Point(e.X, e.Y);
			object obj = m_gridControl.GetDescriptorAt(point);
			if (obj == null)
			{
				obj = GetContext();
			}
			object editingContext = m_gridControl.GridView.EditingContext;
			IEnumerable<object> commands = m_contextMenuCommandProviders.GetCommands(editingContext, obj);
			Point screenPoint = m_gridControl.PointToScreen(point);
			m_commandService.RunContextMenu(commands, screenPoint);
		}
	}

	private void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
	{
		IPropertyEditingContext context = GetContext();
		m_gridControl.Bind(context);
	}

	private IPropertyEditingContext GetContext()
	{
		IPropertyEditingContext propertyEditingContext = m_contextRegistry.GetMostRecentContext<IPropertyEditingContext>();
		if (propertyEditingContext != null)
		{
			m_defaultContext.SelectionContext = null;
		}
		else
		{
			ISelectionContext mostRecentContext = m_contextRegistry.GetMostRecentContext<ISelectionContext>();
			m_defaultContext.SelectionContext = mostRecentContext;
			if (mostRecentContext != null)
			{
				propertyEditingContext = m_defaultContext;
			}
		}
		return propertyEditingContext;
	}
}
