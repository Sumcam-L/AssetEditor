namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public class RangeSliderValueEditorContext : SliderValueEditorContext
{
	private bool m_rangeEnabled;

	private double m_rangeStart;

	private double m_rangeStop;

	public bool RangeEnabled
	{
		get
		{
			return m_rangeEnabled;
		}
		set
		{
			if (m_rangeEnabled != value)
			{
				m_rangeEnabled = value;
				RaisePropertyChanged("RangeEnabled");
			}
		}
	}

	public bool RelativeModeEnabled { get; private set; }

	public double RangeStart
	{
		get
		{
			return m_rangeStart;
		}
		set
		{
			if (!NumericUtil.AreClose(m_rangeStart, value))
			{
				m_rangeStart = value;
				RaisePropertyChanged("RangeStart");
			}
		}
	}

	public double RangeStop
	{
		get
		{
			return m_rangeStop;
		}
		set
		{
			if (!NumericUtil.AreClose(m_rangeStop, value))
			{
				m_rangeStop = value;
				RaisePropertyChanged("RangeStop");
			}
		}
	}

	public RangeSliderValueEditorContext(PropertyNode node)
		: base(node)
	{
		RangeStart = base.Min;
		RangeStop = base.Max;
		RangeEnabled = true;
	}

	public override void CommitEdit()
	{
		base.CommitEdit();
		if (RangeEnabled)
		{
			OnRangeChanged();
		}
	}

	protected virtual void OnRangeChanged()
	{
	}
}
