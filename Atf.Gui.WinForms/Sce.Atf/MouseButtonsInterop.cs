using System;
using System.Windows.Forms;
using Sce.Atf.Input;

namespace Sce.Atf;

public class MouseButtonsInterop
{
	private long Value { get; set; }

	private SupportedTypes ValueType { get; set; }

	private MouseButtonsInterop()
	{
	}

	public MouseButtonsInterop(Sce.Atf.Input.MouseButtons buttons)
	{
		Value = (long)buttons;
		ValueType = SupportedTypes.Atf;
	}

	public MouseButtonsInterop(System.Windows.Forms.MouseButtons buttons)
	{
		Value = (long)buttons;
		ValueType = SupportedTypes.WinForms;
	}

	public static implicit operator MouseButtonsInterop(Sce.Atf.Input.MouseButtons buttons)
	{
		return new MouseButtonsInterop(buttons);
	}

	public static implicit operator MouseButtonsInterop(System.Windows.Forms.MouseButtons buttons)
	{
		return new MouseButtonsInterop(buttons);
	}

	public static implicit operator Sce.Atf.Input.MouseButtons(MouseButtonsInterop buttons)
	{
		switch (buttons.ValueType)
		{
		case SupportedTypes.Atf:
		case SupportedTypes.WinForms:
			return (Sce.Atf.Input.MouseButtons)buttons.Value;
		case SupportedTypes.Wpf:
			throw new Exception("Interop for WPF mouse buttons not implemented yet");
		default:
			throw new InvalidOperationException("Unhandled type specified");
		}
	}

	public static implicit operator System.Windows.Forms.MouseButtons(MouseButtonsInterop buttons)
	{
		switch (buttons.ValueType)
		{
		case SupportedTypes.Atf:
		case SupportedTypes.WinForms:
			return (System.Windows.Forms.MouseButtons)buttons.Value;
		case SupportedTypes.Wpf:
			throw new Exception("Interop for WPF mouse buttons not implemented yet");
		default:
			throw new InvalidOperationException("Unhandled type specified");
		}
	}

	public static Sce.Atf.Input.MouseButtons ToAtf(System.Windows.Forms.MouseButtons buttons)
	{
		return (Sce.Atf.Input.MouseButtons)buttons;
	}

	public static System.Windows.Forms.MouseButtons ToWf(Sce.Atf.Input.MouseButtons buttons)
	{
		return (System.Windows.Forms.MouseButtons)buttons;
	}
}
