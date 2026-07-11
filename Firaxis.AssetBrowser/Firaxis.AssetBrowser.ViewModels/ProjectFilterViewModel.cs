using Firaxis.MVVMBase;

namespace Firaxis.AssetBrowser.ViewModels;

public class ProjectFilterViewModel : Notifier
{
	private string _project;

	private bool _visible;

	public string Project
	{
		get
		{
			return _project;
		}
		set
		{
			if (!(_project == value))
			{
				_project = value;
				OnPropertyChanged("Project");
			}
		}
	}

	public bool Visible
	{
		get
		{
			return _visible;
		}
		set
		{
			if (_visible != value)
			{
				_visible = value;
				OnPropertyChanged("Visible");
			}
		}
	}

	public ProjectFilterViewModel(string projectName, bool enabledByDefault)
	{
		Project = projectName;
		Visible = enabledByDefault;
	}

	protected virtual void UpdateProperties()
	{
		OnPropertyChanged("Project");
		OnPropertyChanged("Visible");
	}

	public override string ToString()
	{
		return Project;
	}
}
