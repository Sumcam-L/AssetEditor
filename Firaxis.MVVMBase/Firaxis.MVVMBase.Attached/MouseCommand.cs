using System.Windows;
using System.Windows.Input;

namespace Firaxis.MVVMBase.Attached;

public class MouseCommand : Freezable
{
	public static readonly DependencyProperty ButtonProperty = DependencyProperty.Register("Button", typeof(MouseButton), typeof(MouseCommand), new PropertyMetadata(MouseButton.Left));

	public static readonly DependencyProperty StateProperty = DependencyProperty.Register("InputStyle", typeof(MouseInputStyle), typeof(MouseCommand), new PropertyMetadata(MouseInputStyle.Down));

	public static readonly DependencyProperty OnPreviewProperty = DependencyProperty.Register("OnPreview", typeof(bool), typeof(MouseCommand), new PropertyMetadata(false));

	public static readonly DependencyProperty ClicksProperty = DependencyProperty.Register("Clicks", typeof(int), typeof(MouseCommand), new PropertyMetadata(1));

	public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(RelayCommand), typeof(MouseCommand), new FrameworkPropertyMetadata(null));

	public MouseButton Button
	{
		get
		{
			return (MouseButton)GetValue(ButtonProperty);
		}
		set
		{
			SetValue(ButtonProperty, value);
		}
	}

	public MouseInputStyle InputStyle
	{
		get
		{
			return (MouseInputStyle)GetValue(StateProperty);
		}
		set
		{
			SetValue(StateProperty, value);
		}
	}

	public bool OnPreview
	{
		get
		{
			return (bool)GetValue(OnPreviewProperty);
		}
		set
		{
			SetValue(OnPreviewProperty, value);
		}
	}

	public int Clicks
	{
		get
		{
			return (int)GetValue(ClicksProperty);
		}
		set
		{
			SetValue(ClicksProperty, value);
		}
	}

	public RelayCommand Command
	{
		get
		{
			return (RelayCommand)GetValue(CommandProperty);
		}
		set
		{
			SetValue(CommandProperty, value);
		}
	}

	public MouseCommand()
	{
	}

	public MouseCommand(MouseButton mouseButton, MouseInputStyle inputStyle, bool onPreview, int clicks, RelayCommand command)
	{
		Button = mouseButton;
		InputStyle = inputStyle;
		OnPreview = onPreview;
		Clicks = clicks;
		Command = command;
	}

	protected override Freezable CreateInstanceCore()
	{
		return new MouseCommand();
	}
}
