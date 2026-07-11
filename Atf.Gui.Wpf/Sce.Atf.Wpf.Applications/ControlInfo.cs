using System.ComponentModel;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Docking;

namespace Sce.Atf.Wpf.Applications;

public class ControlInfo : IControlInfo
{
	private string m_description;

	private object m_imageKey;

	private ICommandItem m_command;

	private static readonly string s_isVisiblePropertyName = TypeUtil.GetProperty((IDockContent x) => x.IsVisible).Name;

	public string Name
	{
		get
		{
			return DockContent.Header;
		}
		set
		{
			DockContent.Header = value;
			if (m_command != null)
			{
				m_command.Text = value;
				m_command.Description = "Show/Hide ".Localize() + value;
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
			m_description = value;
		}
	}

	public object ImageSourceKey
	{
		get
		{
			return m_imageKey;
		}
		set
		{
			m_imageKey = value;
			DockContent.Icon = m_imageKey;
		}
	}

	public string Id { get; private set; }

	public StandardControlGroup Group { get; private set; }

	public IControlHostClient Client { get; private set; }

	public object Content => DockContent.Content;

	public IDockContent DockContent { get; private set; }

	public ICommandItem Command
	{
		get
		{
			return m_command;
		}
		set
		{
			m_command = value;
			if (m_command != null)
			{
				m_command.IsChecked = DockContent.IsVisible;
				m_command.Text = Name;
				m_command.Description = "Show/Hide ".Localize() + Name;
			}
		}
	}

	public ControlInfo(string id, StandardControlGroup group, IDockContent dockContent, IControlHostClient client)
		: this(null, null, id, group, null, dockContent, client)
	{
	}

	public ControlInfo(string name, string description, string id, StandardControlGroup group, IDockContent dockContent, IControlHostClient client)
		: this(name, description, id, group, null, dockContent, client)
	{
	}

	public ControlInfo(string name, string description, string id, StandardControlGroup group, object imageKey, IDockContent dockContent, IControlHostClient client)
	{
		Requires.NotNullOrEmpty(id, "id");
		Requires.NotNull(dockContent, "dockContent");
		Requires.NotNull(client, "client");
		DockContent = dockContent;
		dockContent.PropertyChanged += DockContent_PropertyChanged;
		Name = name;
		Description = description;
		Id = id;
		Group = group;
		ImageSourceKey = imageKey;
		Client = client;
	}

	private void DockContent_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (Command != null && e.PropertyName == s_isVisiblePropertyName)
		{
			Command.IsChecked = ((IDockContent)sender).IsVisible;
		}
	}
}
