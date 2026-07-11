using System;
using System.Windows.Input;
using System.Windows.Media;

namespace Firaxis.MVVMBase;

public class ToolbarButton : Notifier
{
	private DelegateCommand m_command;

	private bool _visible;

	public string Name { get; }

	public ImageSource Thumbnail { get; }

	public ICommand Command => m_command;

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

	public ToolbarButton(string name, ImageSource thumbnail, Action<object> execute)
		: this(name, thumbnail, execute, null, visible: true)
	{
	}

	public ToolbarButton(string name, ImageSource thumbnail, Action<object> execute, Predicate<object> canExecute)
		: this(name, thumbnail, execute, canExecute, visible: true)
	{
	}

	public ToolbarButton(string name, ImageSource thumbnail, Action<object> execute, Predicate<object> canExecute, bool visible)
	{
		m_command = new DelegateCommand(execute, canExecute);
		Name = name;
		Thumbnail = thumbnail;
		Visible = visible;
	}

	public void UpdateCanExecute()
	{
		m_command.RaiseCanExecuteChanged();
	}
}
