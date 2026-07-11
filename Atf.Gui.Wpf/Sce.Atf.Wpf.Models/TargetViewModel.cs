using System;
using System.ComponentModel;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models;

[Serializable]
public class TargetViewModel : NotifyPropertyChangedBase
{
	[NonSerialized]
	private IProtocol m_protocol;

	private bool m_isSelected;

	private static readonly PropertyChangedEventArgs s_isSelectedArgs = ObservableUtil.CreateArgs((TargetViewModel x) => x.IsSelected);

	public ITarget Target { get; private set; }

	public IProtocol Protocol
	{
		get
		{
			return m_protocol;
		}
		set
		{
			if (Target.ProtocolId != value.Id)
			{
				throw new ArgumentException("Invalid protocol for this target");
			}
			m_protocol = value;
		}
	}

	public bool IsSelected
	{
		get
		{
			return m_isSelected;
		}
		set
		{
			m_isSelected = value;
			OnPropertyChanged(s_isSelectedArgs);
		}
	}

	public TargetViewModel(ITarget target, IProtocol protocol)
	{
		Requires.NotNull(target, "target");
		Requires.NotNull(protocol, "protocol");
		Target = target;
		m_protocol = protocol;
	}
}
