using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class CircuitGroupInfo : INotifyPropertyChanged
{
	private Size m_minSize;

	private Point m_offset;

	private bool m_showExpandedGroupPins;

	private bool m_editing;

	private IEnumerable<ICircuitPin> m_hiddenInputPins = EmptyEnumerable<ICircuitPin>.Instance;

	private IEnumerable<ICircuitPin> m_hiddenOutputPins = EmptyEnumerable<ICircuitPin>.Instance;

	public Size MinimumSize
	{
		get
		{
			return m_minSize;
		}
		set
		{
			this.PropertyChanged.NotifyIfChanged(ref m_minSize, value, () => MinimumSize);
		}
	}

	public Point Offset
	{
		get
		{
			return m_offset;
		}
		set
		{
			this.PropertyChanged.NotifyIfChanged(ref m_offset, value, () => Offset);
		}
	}

	public bool ShowExpandedGroupPins
	{
		get
		{
			return m_showExpandedGroupPins;
		}
		set
		{
			this.PropertyChanged.NotifyIfChanged(ref m_showExpandedGroupPins, value, () => ShowExpandedGroupPins);
		}
	}

	public bool IsEditing
	{
		get
		{
			return m_editing;
		}
		set
		{
			this.PropertyChanged.NotifyIfChanged(ref m_editing, value, () => IsEditing);
		}
	}

	public IEnumerable<ICircuitPin> HiddenInputPins
	{
		get
		{
			return m_hiddenInputPins;
		}
		set
		{
			this.PropertyChanged.NotifyIfChanged(ref m_hiddenInputPins, value, () => HiddenInputPins);
		}
	}

	public IEnumerable<ICircuitPin> HiddenOutputPins
	{
		get
		{
			return m_hiddenOutputPins;
		}
		set
		{
			this.PropertyChanged.NotifyIfChanged(ref m_hiddenOutputPins, value, () => HiddenOutputPins);
		}
	}

	public int PickingPriority { get; set; }

	public event PropertyChangedEventHandler PropertyChanged;

	public CircuitGroupInfo()
	{
		m_showExpandedGroupPins = CircuitDefaultStyle.ShowExpandedGroupPins;
	}
}
