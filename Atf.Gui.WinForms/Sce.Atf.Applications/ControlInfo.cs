using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public class ControlInfo
{
	public class DockingInfo
	{
		private StandardDockAreas m_dockAreas = StandardDockAreas.Default;

		public StandardDockAreas DockAreas
		{
			get
			{
				return m_dockAreas;
			}
			set
			{
				m_dockAreas = value;
			}
		}

		public int Order { get; set; }

		public object GroupTag { get; set; }
	}

	private IControlHostClient m_client;

	private Control m_control;

	private Control m_hostControl;

	private string m_name;

	private string m_description;

	private StandardControlGroup m_group;

	private Image m_image;

	private Icon m_icon;

	private DockStyle m_originalDock;

	private bool m_inActiveGroup;

	private bool m_showInMenu = true;

	private string m_menuText;

	private object m_menuGroupOverride = null;

	private ControlInitialVisibility m_controlVisibility = ControlInitialVisibility.InitiallyVisible;

	private readonly string m_helpUrl;

	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			if (value != m_name)
			{
				this.Changing.Raise(this, EventArgs.Empty);
				m_name = value;
				this.Changed.Raise(this, EventArgs.Empty);
			}
		}
	}

	public string Description
	{
		get
		{
			return m_description;
		}
		set
		{
			if (value != m_description)
			{
				this.Changing.Raise(this, EventArgs.Empty);
				m_description = value;
				this.Changed.Raise(this, EventArgs.Empty);
			}
		}
	}

	public StandardControlGroup Group
	{
		get
		{
			return m_group;
		}
		set
		{
			m_group = value;
		}
	}

	public Image Image
	{
		get
		{
			return m_image;
		}
		set
		{
			if (value != m_image)
			{
				this.Changing.Raise(this, EventArgs.Empty);
				m_image = value;
				this.Changed.Raise(this, EventArgs.Empty);
			}
		}
	}

	public Icon Icon
	{
		get
		{
			return m_icon;
		}
		set
		{
			if (value != m_icon)
			{
				this.Changing.Raise(this, EventArgs.Empty);
				m_icon = value;
				this.Changed.Raise(this, EventArgs.Empty);
			}
		}
	}

	public IControlHostClient Client
	{
		get
		{
			return m_client;
		}
		set
		{
			m_client = value;
		}
	}

	public Control Control
	{
		get
		{
			return m_control;
		}
		set
		{
			m_control = value;
		}
	}

	public Control HostControl
	{
		get
		{
			return m_hostControl;
		}
		set
		{
			m_hostControl = value;
		}
	}

	public DockStyle OriginalDock
	{
		get
		{
			return m_originalDock;
		}
		set
		{
			m_originalDock = value;
		}
	}

	public bool InActiveGroup
	{
		get
		{
			return m_inActiveGroup;
		}
		set
		{
			m_inActiveGroup = value;
		}
	}

	public bool ShowInMenu
	{
		get
		{
			return m_showInMenu;
		}
		set
		{
			m_showInMenu = value;
		}
	}

	public string MenuText
	{
		get
		{
			return m_menuText;
		}
		set
		{
			m_menuText = value;
		}
	}

	public object MenuGroupOverride
	{
		get
		{
			return m_menuGroupOverride;
		}
		set
		{
			m_menuGroupOverride = value;
		}
	}

	public bool? IsDocument { get; set; }

	public Func<bool> IsDirtyDocument { get; set; } = () => false;

	public Func<bool> IsReadOnlyDocument { get; set; } = () => false;

	public ControlInitialVisibility ControlVisibility
	{
		get
		{
			return m_controlVisibility;
		}
		set
		{
			m_controlVisibility = value;
		}
	}

	public bool UseCustomAppearance { get; set; }

	public DockingInfo Docking { get; set; }

	public string HelpUrl => m_helpUrl;

	public event EventHandler Changing;

	public event EventHandler Changed;

	public ControlInfo(string name, string description, StandardControlGroup group)
		: this(name, description, group, null)
	{
	}

	public ControlInfo(string name, string description, StandardControlGroup group, string helpUrl)
	{
		m_name = name;
		m_menuText = "@" + m_name;
		m_description = description;
		m_group = group;
		m_helpUrl = helpUrl;
		m_icon = null;
		m_image = null;
	}

	public ControlInfo(string name, string description, StandardControlGroup group, Image image, string helpUrl)
		: this(name, description, group, helpUrl)
	{
		m_image = image;
	}

	public ControlInfo(string name, string description, StandardControlGroup group, Icon icon, string helpUrl)
		: this(name, description, group, helpUrl)
	{
		m_icon = icon;
	}
}
