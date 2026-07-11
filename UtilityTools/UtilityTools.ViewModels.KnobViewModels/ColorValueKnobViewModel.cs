using System.Drawing;
using System.Windows.Input;
using System.Windows.Media;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.MVVMBase;
using UtilityTools.Helpers;

namespace UtilityTools.ViewModels.KnobViewModels;

public class ColorValueKnobViewModel : KnobViewModel
{
	private DelegateCommand m_selectColorCommand;

	public int BValue
	{
		get
		{
			return ColorValueKnob.BValue;
		}
		set
		{
			if (ColorValueKnob.BValue != value)
			{
				ColorValueKnob.BValue = value;
				OnPropertyChanged("BValue");
				OnPropertyChanged("Value");
			}
		}
	}

	public int GValue
	{
		get
		{
			return ColorValueKnob.GValue;
		}
		set
		{
			if (ColorValueKnob.GValue != value)
			{
				ColorValueKnob.GValue = value;
				OnPropertyChanged("GValue");
				OnPropertyChanged("Value");
			}
		}
	}

	public virtual bool IsEnabled => !ColorValueKnob.IsReadOnly;

	public int RValue
	{
		get
		{
			return ColorValueKnob.RValue;
		}
		set
		{
			if (ColorValueKnob.RValue != value)
			{
				ColorValueKnob.RValue = value;
				OnPropertyChanged("RValue");
				OnPropertyChanged("Value");
			}
		}
	}

	public System.Drawing.Color Value
	{
		get
		{
			return ColorValueKnob.Value;
		}
		set
		{
			if (ColorValueKnob.Value != value)
			{
				ColorValueKnob.Value = value;
				OnPropertyChanged("ValueBrush");
				OnPropertyChanged("Value");
				OnPropertyChanged("BValue");
				OnPropertyChanged("GValue");
				OnPropertyChanged("RValue");
			}
		}
	}

	public SolidColorBrush ValueBrush => new SolidColorBrush(new System.Windows.Media.Color
	{
		A = Value.A,
		B = Value.B,
		G = Value.G,
		R = Value.R
	});

	private IColorValueKnob ColorValueKnob => (IColorValueKnob)base.Knob;

	public ICommand SelectColorCommand
	{
		get
		{
			if (m_selectColorCommand == null)
			{
				m_selectColorCommand = new DelegateCommand(SelectColor);
			}
			return m_selectColorCommand;
		}
	}

	public ColorValueKnobViewModel(IColorValueKnob knob)
		: base(knob)
	{
	}

	protected override void RaisePropertyChangedEvents()
	{
		base.RaisePropertyChangedEvents();
		OnPropertyChanged("Value");
		OnPropertyChanged("RValue");
		OnPropertyChanged("GValue");
		OnPropertyChanged("BValue");
	}

	private void SelectColor(object context)
	{
		System.Drawing.Color outColor = System.Drawing.Color.Transparent;
		if (DialogHelper.SelectColor(ref outColor))
		{
			Value = outColor;
		}
	}
}
