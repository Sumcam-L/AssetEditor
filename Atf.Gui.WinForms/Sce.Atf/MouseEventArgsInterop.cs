using System.Windows.Forms;
using Sce.Atf.Input;

namespace Sce.Atf;

public class MouseEventArgsInterop
{
	public Sce.Atf.Input.MouseButtons Button { get; private set; }

	public int Clicks { get; private set; }

	public int X { get; private set; }

	public int Y { get; private set; }

	public int Delta { get; private set; }

	public MouseEventArgsInterop(Sce.Atf.Input.MouseButtons button, int clicks, int x, int y, int delta)
	{
		Button = button;
		Clicks = clicks;
		X = x;
		Y = y;
		Delta = delta;
	}

	public MouseEventArgsInterop(System.Windows.Forms.MouseButtons button, int clicks, int x, int y, int delta)
		: this(MouseButtonsInterop.ToAtf(button), clicks, x, y, delta)
	{
	}

	public MouseEventArgsInterop(Sce.Atf.Input.MouseEventArgs args)
		: this(args.Button, args.Clicks, args.X, args.Y, args.Delta)
	{
	}

	public MouseEventArgsInterop(System.Windows.Forms.MouseEventArgs args)
		: this(args.Button, args.Clicks, args.X, args.Y, args.Delta)
	{
	}

	public static implicit operator MouseEventArgsInterop(System.Windows.Forms.MouseEventArgs mouseEventArgs)
	{
		return new MouseEventArgsInterop(mouseEventArgs);
	}

	public static implicit operator Sce.Atf.Input.MouseButtons(MouseEventArgsInterop args)
	{
		return args.Button;
	}

	public static implicit operator System.Windows.Forms.MouseButtons(MouseEventArgsInterop args)
	{
		return new MouseButtonsInterop(args.Button);
	}

	public static implicit operator System.Windows.Forms.MouseEventArgs(MouseEventArgsInterop args)
	{
		return new System.Windows.Forms.MouseEventArgs(args, args.Clicks, args.X, args.Y, args.Delta);
	}

	public static Sce.Atf.Input.MouseEventArgs ToAtf(System.Windows.Forms.MouseEventArgs args)
	{
		return new Sce.Atf.Input.MouseEventArgs(MouseButtonsInterop.ToAtf(args.Button), args.Clicks, args.X, args.Y, args.Delta);
	}

	public static System.Windows.Forms.MouseEventArgs ToWf(Sce.Atf.Input.MouseEventArgs args)
	{
		return new System.Windows.Forms.MouseEventArgs(MouseButtonsInterop.ToWf(args.Button), args.Clicks, args.X, args.Y, args.Delta);
	}
}
