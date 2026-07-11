using System.Drawing;
using System.Windows.Input;
using System.Windows.Media;
using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;
using UtilityTools.Helpers;

namespace UtilityTools.ViewModels.ParameterViewModels;

public class RGBParameterViewModel : BaseParameterViewModel
{
	private DelegateCommand m_selectColorCommand;

	public string BValue
	{
		get
		{
			return RGBValueInternal.ParameterValue.B.ToString();
		}
		set
		{
			if (byte.TryParse(value, out var result))
			{
				System.Drawing.Color parameterValue = System.Drawing.Color.FromArgb(RGBValueInternal.ParameterValue.R, RGBValueInternal.ParameterValue.G, result);
				RGBValueInternal.ParameterValue = parameterValue;
			}
			OnPropertyChanged("BValue");
			OnPropertyChanged("RGBValue");
		}
	}

	public string GValue
	{
		get
		{
			return RGBValueInternal.ParameterValue.G.ToString();
		}
		set
		{
			if (byte.TryParse(value, out var result))
			{
				System.Drawing.Color parameterValue = System.Drawing.Color.FromArgb(RGBValueInternal.ParameterValue.R, result, RGBValueInternal.ParameterValue.B);
				RGBValueInternal.ParameterValue = parameterValue;
			}
			OnPropertyChanged("GValue");
			OnPropertyChanged("RGBValue");
		}
	}

	public SolidColorBrush RGBValue
	{
		get
		{
			System.Drawing.Color parameterValue = RGBValueInternal.ParameterValue;
			System.Windows.Media.Color color = System.Windows.Media.Color.FromArgb(parameterValue.A, parameterValue.R, parameterValue.G, parameterValue.B);
			return new SolidColorBrush(color);
		}
	}

	public string RValue
	{
		get
		{
			return RGBValueInternal.ParameterValue.R.ToString();
		}
		set
		{
			if (byte.TryParse(value, out var result))
			{
				System.Drawing.Color parameterValue = System.Drawing.Color.FromArgb(result, RGBValueInternal.ParameterValue.G, RGBValueInternal.ParameterValue.B);
				RGBValueInternal.ParameterValue = parameterValue;
			}
			OnPropertyChanged("RValue");
			OnPropertyChanged("RGBValue");
		}
	}

	private IRGBParameter RGBParam => (IRGBParameter)base.Parameter;

	private IRGBValue RGBValueInternal => (IRGBValue)base.Value;

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

	public RGBParameterViewModel(IRGBParameter param, IRGBValue value)
		: base(param, value)
	{
	}

	private void SelectColor(object context)
	{
		System.Drawing.Color outColor = System.Drawing.Color.Transparent;
		if (DialogHelper.SelectColor(ref outColor))
		{
			RGBValueInternal.ParameterValue = outColor;
			OnPropertyChanged("RValue");
			OnPropertyChanged("GValue");
			OnPropertyChanged("BValue");
			OnPropertyChanged("RGBValue");
			OnParameterValueChangedEvent(base.Name, base.Value);
		}
	}
}
