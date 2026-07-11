using System.ComponentModel;

namespace Sce.Atf.Applications.NetworkTargetServices;

public abstract class TargetInfo : INotifyPropertyChanged
{
	private string m_name;

	private string m_platform;

	private string m_endPoint;

	private string m_protocol;

	private TargetScope m_scope;

	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			if (m_name != value)
			{
				m_name = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Name"));
			}
		}
	}

	public string Platform
	{
		get
		{
			return m_platform;
		}
		set
		{
			if (m_platform != value)
			{
				m_platform = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Platform"));
			}
		}
	}

	public string Endpoint
	{
		get
		{
			return m_endPoint;
		}
		set
		{
			if (m_endPoint != value)
			{
				m_endPoint = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Endpoint"));
			}
		}
	}

	public string Protocol
	{
		get
		{
			return m_protocol;
		}
		set
		{
			if (m_protocol != value)
			{
				m_protocol = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Protocol"));
			}
		}
	}

	public TargetScope Scope
	{
		get
		{
			return m_scope;
		}
		set
		{
			if (m_scope != value)
			{
				m_scope = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Scope"));
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		this.PropertyChanged?.Invoke(this, e);
	}

	public override string ToString()
	{
		return $"Name: {Name}, Platform: {Platform}, Endpoint: {Endpoint}, Protocol: {Protocol}, Scope: {Scope}";
	}
}
