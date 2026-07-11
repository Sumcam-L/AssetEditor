using System.ComponentModel;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class CircuitElementInfo : INotifyPropertyChanged
{
	private bool m_showUnconnectedPins = true;

	private bool m_valid = true;

	public bool ShowUnconnectedPins
	{
		get
		{
			return m_showUnconnectedPins;
		}
		set
		{
			if (m_showUnconnectedPins != value)
			{
				m_showUnconnectedPins = value;
				OnPropertyChanged("ShowUnconnectedPins");
			}
		}
	}

	public bool IsValid
	{
		get
		{
			return m_valid;
		}
		set
		{
			if (m_valid != value)
			{
				m_valid = value;
				OnPropertyChanged("IsValid");
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
