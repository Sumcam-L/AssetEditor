using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(IControlHostClient))]
[Export(typeof(PropertyEditor))]
[PartCreationPolicy(CreationPolicy.Any)]
public class PropertyEditor : IInitializable, IControlHostClient, IDisposable
{
	[ImportMany]
	private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders;

	private Sce.Atf.Controls.PropertyEditing.PropertyGrid m_propertyGrid;

	private ControlInfo m_controlInfo;

	private SelectionPropertyEditingContext m_defaultContext = new SelectionPropertyEditingContext();

	public Sce.Atf.Controls.PropertyEditing.PropertyGrid PropertyGrid => m_propertyGrid;

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

	protected IControlHostService ControlHostService { get; private set; }

	protected IContextRegistry ContextRegistry { get; private set; }

	protected ICommandService CommandService { get; private set; }

	[Import(AllowDefault = true)]
	public ISettingsService SettingsService { get; set; }

	[ImportingConstructor]
	public PropertyEditor(ICommandService commandService, IControlHostService controlHostService, IContextRegistry contextRegistry)
	{
		CommandService = commandService;
		ControlHostService = controlHostService;
		ContextRegistry = contextRegistry;
	}

	protected virtual void Configure(out Sce.Atf.Controls.PropertyEditing.PropertyGrid propertyGrid, out ControlInfo controlInfo)
	{
		propertyGrid = new Sce.Atf.Controls.PropertyEditing.PropertyGrid();
		controlInfo = new ControlInfo("Property Editor".Localize(), "Edits selected object properties".Localize(), StandardControlGroup.Right, "https://github.com/SonyWWS/ATF/wiki/Property-Editing-in-ATF".Localize());
	}

	public virtual void Initialize()
	{
		Configure(out m_propertyGrid, out m_controlInfo);
		m_propertyGrid.PropertyGridView.ContextRegistry = ContextRegistry;
		m_propertyGrid.MouseUp += propertyGrid_MouseUp;
		ContextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;
		ControlHostService.RegisterControl(m_propertyGrid, m_controlInfo, this);
		if (SettingsService != null)
		{
			SettingsService.RegisterSettings(this, new BoundPropertyDescriptor(m_propertyGrid, () => m_propertyGrid.Settings, "Settings", null, null));
		}
	}

	public void Dispose()
	{
		m_propertyGrid.Dispose();
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

	protected virtual void OnPropertyGridMouseUp(MouseEventArgs e)
	{
		if (CommandService != null)
		{
			Point point = new Point(e.X, e.Y);
			IPropertyEditingContext editingContext;
			object obj = m_propertyGrid.GetDescriptorAt(point, out editingContext);
			if (obj == null)
			{
				obj = GetContext();
			}
			IEnumerable<object> commandTags = from x in m_contextMenuCommandProviders.GetCommands(editingContext, obj)
				where !IsStandardEditCommand(x)
				select x;
			Point screenPoint = m_propertyGrid.PointToScreen(point);
			CommandService.RunContextMenu(commandTags, screenPoint);
		}
	}

	private bool IsStandardEditCommand(object commandTag)
	{
		if (commandTag is StandardCommand)
		{
			if ((StandardCommand)commandTag == StandardCommand.EditCut)
			{
				return true;
			}
			if ((StandardCommand)commandTag == StandardCommand.EditCopy)
			{
				return true;
			}
			if ((StandardCommand)commandTag == StandardCommand.EditPaste)
			{
				return true;
			}
			if ((StandardCommand)commandTag == StandardCommand.EditDelete)
			{
				return true;
			}
		}
		return false;
	}

	private void propertyGrid_MouseUp(object sender, MouseEventArgs e)
	{
		OnPropertyGridMouseUp(e);
	}

	private void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
	{
		IPropertyEditingContext context = GetContext();
		m_propertyGrid.Bind(context);
	}

	private IPropertyEditingContext GetContext()
	{
		IPropertyEditingContext propertyEditingContext = ContextRegistry.GetMostRecentContext<IPropertyEditingContext>();
		if (propertyEditingContext != null)
		{
			m_defaultContext.SelectionContext = null;
		}
		else
		{
			ISelectionContext mostRecentContext = ContextRegistry.GetMostRecentContext<ISelectionContext>();
			m_defaultContext.SelectionContext = mostRecentContext;
			if (mostRecentContext != null)
			{
				propertyEditingContext = m_defaultContext;
			}
		}
		return propertyEditingContext;
	}
}
